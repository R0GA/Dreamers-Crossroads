using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Drives the Sir Nightmare encounter. Place this at the CENTRE of the arena (platform sweep
/// order is calculated around this transform).
///
/// Fight loop, per phase:
///   1. Sir Nightmare attacks: orbs at the player, and strikes that crumble platforms.
///   2. Dream bubbles drift into the arena. Swinging through one pops it and makes him drowsier.
///   3. Once drowsy enough, his attacks stop and a Lullaby Anchor appears on him for a limited
///      window. Grapple in and interact to sing him under -> next phase (or victory).
///   4. Miss the window and he snaps awake, losing only some of his drowsiness.
///
/// No health, no damage. Progress is the drowsiness meter; failure is losing footing and time.
/// </summary>
public class SirNightmareFight : MonoBehaviour
{
    public enum FightState { Dormant, Transition, Attacking, Drowsy, Defeated }

    public enum AttackType
    {
        AimedVolley,        // a few orbs fired at the player, slightly led
        Fan,                // a spread of orbs — dodge through the gaps
        HomingOrb,          // one slow orb that curves toward the player
        PlatformScatter,    // random platforms telegraph then crumble
        PlatformHunt,       // targets the platform nearest below the player (+ one random)
        PlatformSweep       // a wave of crumbling that rolls around the arena
    }

    [Serializable]
    public class Phase
    {
        public string name = "Phase";

        [Header("Progress")]
        [Min(1)] public int bubblesToDrowse = 3;
        [Tooltip("Platforms that collapse permanently when this phase starts — shrinks the arena as the level falls apart.")]
        public CrumblingPlatform[] collapseOnStart;

        [Header("Pacing")]
        public Vector2 delayBetweenAttacks = new Vector2(3f, 4.5f);
        public AttackType[] attacks = { AttackType.AimedVolley, AttackType.PlatformScatter };

        [Header("Orbs")]
        public float orbSpeed = 11f;
        [Min(1)] public int volleyCount = 2;
        public float volleyInterval = 0.45f;
        [Min(1)] public int fanOrbs = 5;
        public float fanArcDegrees = 50f;
        public float homingTurnRate = 55f;
        public float homingDuration = 3.5f;

        [Header("Platforms")]
        public float platformWarnTime = 1.6f;
        public float platformDownTime = 5f;
        [Min(1)] public int scatterCount = 2;
        public float sweepStep = 0.25f;

        [Header("Bubbles")]
        public int maxActiveBubbles = 2;
        [Tooltip("Seconds between bubble spawns. Longer = scarcer bubbles = more tension.")]
        public float bubbleSpawnDelay = 3f;
    }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Scene References")]
    [SerializeField] private PlayerController player;
    [Tooltip("Where orbs spawn — Sir Nightmare's hand or maw.")]
    [SerializeField] private Transform muzzle;
    [Tooltip("Optional. Triggers used: Wake, Throw, Slam, Flinch, Drowsy, Defeated.")]
    [SerializeField] private Animator animator;
    [SerializeField] private LullabyAnchor lullabyAnchor;
    [Tooltip("Every platform in the arena. Auto-filled at runtime if left empty.")]
    [SerializeField] private CrumblingPlatform[] platforms;
    [Tooltip("Empty transforms placed in gaps between platforms, at swing height. Bubbles spawn here.")]
    [SerializeField] private Transform[] bubbleSpawnPoints;

    [Header("Prefabs")]
    [SerializeField] private NightmareOrb orbPrefab;
    [SerializeField] private DreamBubble bubblePrefab;

    [Header("Fight Rules")]
    [SerializeField] private Phase[] phases;
    [Tooltip("Seconds the Lullaby Anchor stays available once he's drowsy.")]
    [SerializeField] private float drowsyWindow = 12f;
    [Tooltip("Drowsiness he keeps if the window is missed (0 = back to zero, 1 = instantly drowsy again).")]
    [SerializeField, Range(0f, 1f)] private float drowsinessOnMiss = 0.5f;
    [Tooltip("At most this fraction of the strikeable platforms can be down at once, so there's always somewhere to land.")]
    [SerializeField, Range(0.1f, 0.8f)] private float maxCrumbledFraction = 0.4f;
    [Tooltip("How much attacks lead the player's movement. 0 = aim at where they are, 1 = perfect prediction (unfair).")]
    [SerializeField, Range(0f, 1f)] private float aimLead = 0.5f;
    [SerializeField] private float openingDelay = 2f;
    [SerializeField] private float phaseTransitionTime = 3f;

    [Header("Events")]
    public UnityEvent onFightStarted;
    public UnityEvent<int> onPhaseStarted;
    public UnityEvent onDrowsy;
    public UnityEvent onWokeUp;
    public UnityEvent onDefeated;

    // ── Public State ──────────────────────────────────────────────────────────

    public FightState State { get; private set; } = FightState.Dormant;
    public int PhaseIndex { get; private set; }

    /// <summary>0-1 progress toward the drowsy window. Drive a sand/eyelid visual or a UI slider from DrowsinessChanged.</summary>
    public float Drowsiness { get; private set; }
    public event Action<float> DrowsinessChanged;

    // ── Private ───────────────────────────────────────────────────────────────

    private CharacterController playerCC;
    private Coroutine attackRoutine, bubbleRoutine, drowsyRoutine, phaseRoutine;
    private readonly List<NightmareOrb> activeOrbs = new List<NightmareOrb>();
    private readonly Dictionary<DreamBubble, Transform> activeBubbles = new Dictionary<DreamBubble, Transform>();
    private AttackType lastAttack = (AttackType)(-1);

    private void Reset()
    {
        // Sensible starting values so the fight works out of the box; tune in the Inspector.
        phases = new[]
        {
            new Phase
            {
                name = "Restless", bubblesToDrowse = 3,
                delayBetweenAttacks = new Vector2(3f, 4.5f),
                attacks = new[] { AttackType.AimedVolley, AttackType.PlatformScatter },
                orbSpeed = 11f, volleyCount = 2, platformWarnTime = 1.6f, platformDownTime = 5f,
                scatterCount = 2, maxActiveBubbles = 2, bubbleSpawnDelay = 3f
            },
            new Phase
            {
                name = "Fitful", bubblesToDrowse = 4,
                delayBetweenAttacks = new Vector2(2.2f, 3.5f),
                attacks = new[] { AttackType.AimedVolley, AttackType.Fan, AttackType.HomingOrb,
                                  AttackType.PlatformScatter, AttackType.PlatformHunt },
                orbSpeed = 13f, volleyCount = 3, fanOrbs = 5, fanArcDegrees = 50f,
                platformWarnTime = 1.3f, platformDownTime = 5.5f, scatterCount = 3,
                maxActiveBubbles = 2, bubbleSpawnDelay = 3.5f
            },
            new Phase
            {
                name = "Terror", bubblesToDrowse = 5,
                delayBetweenAttacks = new Vector2(1.6f, 2.6f),
                attacks = new[] { AttackType.Fan, AttackType.HomingOrb, AttackType.PlatformHunt,
                                  AttackType.PlatformSweep, AttackType.PlatformScatter },
                orbSpeed = 15f, volleyCount = 3, fanOrbs = 7, fanArcDegrees = 70f,
                platformWarnTime = 1.0f, platformDownTime = 6f, scatterCount = 4, sweepStep = 0.2f,
                maxActiveBubbles = 2, bubbleSpawnDelay = 4f
            }
        };
    }

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (player == null || lullabyAnchor == null || phases == null || phases.Length == 0)
        {
            Debug.LogError("SirNightmareFight: assign the Player, Lullaby Anchor and at least one Phase.", this);
            enabled = false;
            return;
        }

        playerCC = player.GetComponent<CharacterController>();

        if (platforms == null || platforms.Length == 0)
            platforms = FindObjectsByType<CrumblingPlatform>(FindObjectsSortMode.None);

        // Sorted by angle around the arena centre so PlatformSweep can roll around it
        Vector3 centre = transform.position;
        Array.Sort(platforms, (a, b) => AngleAround(centre, a).CompareTo(AngleAround(centre, b)));

        lullabyAnchor.Sung += OnLullabySung;
        lullabyAnchor.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (lullabyAnchor != null) lullabyAnchor.Sung -= OnLullabySung;
    }

    private static float AngleAround(Vector3 centre, CrumblingPlatform p)
    {
        Vector3 d = p.transform.position - centre;
        return Mathf.Atan2(d.z, d.x);
    }

    // ── Fight Flow ────────────────────────────────────────────────────────────

    [ContextMenu("Start Fight")]
    public void StartFight()
    {
        if (State != FightState.Dormant || !enabled) return;
        onFightStarted.Invoke();
        phaseRoutine = StartCoroutine(BeginPhase(0));
    }

    private IEnumerator BeginPhase(int index)
    {
        PhaseIndex = index;
        State = FightState.Transition;
        Phase phase = phases[index];

        SetDrowsiness(0f);
        Trigger("Wake");

        if (phase.collapseOnStart != null)
            foreach (var p in phase.collapseOnStart)
                if (p != null) p.Collapse();

        onPhaseStarted.Invoke(index);

        yield return new WaitForSeconds(index == 0 ? openingDelay : phaseTransitionTime);

        StartAttacking();
    }

    private void StartAttacking()
    {
        State = FightState.Attacking;
        attackRoutine = StartCoroutine(AttackLoop());
        bubbleRoutine = StartCoroutine(BubbleLoop());
    }

    private void StopFightRoutines()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (bubbleRoutine != null) StopCoroutine(bubbleRoutine);
        attackRoutine = bubbleRoutine = null;
    }

    private void SetDrowsiness(float value)
    {
        Drowsiness = Mathf.Clamp01(value);
        DrowsinessChanged?.Invoke(Drowsiness);
    }

    private void OnBubblePopped(DreamBubble bubble)
    {
        activeBubbles.Remove(bubble);
        if (State != FightState.Attacking) return;

        SetDrowsiness(Drowsiness + 1f / phases[PhaseIndex].bubblesToDrowse);
        Trigger("Flinch");

        if (Drowsiness >= 0.999f) EnterDrowsy();
    }

    private void OnBubbleExpired(DreamBubble bubble) => activeBubbles.Remove(bubble);

    private void EnterDrowsy()
    {
        StopFightRoutines();
        State = FightState.Drowsy;

        ClearOrbs();
        ClearBubbles();

        Trigger("Drowsy");
        lullabyAnchor.gameObject.SetActive(true);
        onDrowsy.Invoke();

        drowsyRoutine = StartCoroutine(DrowsyWindow());
    }

    private IEnumerator DrowsyWindow()
    {
        yield return new WaitForSeconds(drowsyWindow);

        // Missed it — he snaps awake, keeping part of his drowsiness
        player.ReleaseGrappleFrom(lullabyAnchor.transform);
        lullabyAnchor.gameObject.SetActive(false);
        Trigger("Wake");
        onWokeUp.Invoke();
        SetDrowsiness(drowsinessOnMiss);

        StartAttacking();
    }

    private void OnLullabySung()
    {
        if (State != FightState.Drowsy) return;
        if (drowsyRoutine != null) StopCoroutine(drowsyRoutine);

        // The anchor is about to vanish — don't leave the rope pinned to it
        player.ReleaseGrappleFrom(lullabyAnchor.transform);
        lullabyAnchor.gameObject.SetActive(false);

        if (PhaseIndex + 1 >= phases.Length) Defeat();
        else phaseRoutine = StartCoroutine(BeginPhase(PhaseIndex + 1));
    }

    private void Defeat()
    {
        State = FightState.Defeated;
        StopFightRoutines();
        ClearOrbs();
        ClearBubbles();
        Trigger("Defeated");
        onDefeated.Invoke();
    }

    // ── Attack Loop ───────────────────────────────────────────────────────────

    private IEnumerator AttackLoop()
    {
        while (State == FightState.Attacking)
        {
            Phase phase = phases[PhaseIndex];
            yield return new WaitForSeconds(UnityEngine.Random.Range(phase.delayBetweenAttacks.x, phase.delayBetweenAttacks.y));

            if (phase.attacks == null || phase.attacks.Length == 0) continue;

            // Yielding the IEnumerator directly (not StartCoroutine) keeps the attack inside this
            // coroutine, so StopFightRoutines() cancels a half-finished attack too.
            yield return RunAttack(PickAttack(phase), phase);
        }
    }

    private AttackType PickAttack(Phase phase)
    {
        AttackType pick = phase.attacks[UnityEngine.Random.Range(0, phase.attacks.Length)];

        // Avoid the same attack twice in a row when there's a choice
        for (int i = 0; i < 3 && pick == lastAttack && phase.attacks.Length > 1; i++)
            pick = phase.attacks[UnityEngine.Random.Range(0, phase.attacks.Length)];

        lastAttack = pick;
        return pick;
    }

    private IEnumerator RunAttack(AttackType type, Phase phase)
    {
        switch (type)
        {
            case AttackType.AimedVolley:     return AimedVolley(phase);
            case AttackType.Fan:             return Fan(phase);
            case AttackType.HomingOrb:       return HomingOrb(phase);
            case AttackType.PlatformScatter: return PlatformScatter(phase);
            case AttackType.PlatformHunt:    return PlatformHunt(phase);
            case AttackType.PlatformSweep:   return PlatformSweep(phase);
            default:                         return null;
        }
    }

    private IEnumerator AimedVolley(Phase phase)
    {
        Trigger("Throw");
        for (int i = 0; i < phase.volleyCount; i++)
        {
            FireOrb(AimDirection(phase.orbSpeed), phase.orbSpeed, phase, homing: false);
            yield return new WaitForSeconds(phase.volleyInterval);
        }
    }

    private IEnumerator Fan(Phase phase)
    {
        Trigger("Throw");
        Vector3 centre = AimDirection(phase.orbSpeed);

        for (int i = 0; i < phase.fanOrbs; i++)
        {
            float t = phase.fanOrbs == 1 ? 0f : i / (phase.fanOrbs - 1f) - 0.5f; // -0.5 .. 0.5
            Vector3 dir = Quaternion.AngleAxis(t * phase.fanArcDegrees, Vector3.up) * centre;
            FireOrb(dir, phase.orbSpeed, phase, homing: false);
        }

        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator HomingOrb(Phase phase)
    {
        Trigger("Throw");
        float slow = phase.orbSpeed * 0.55f; // slow enough to outmanoeuvre with a swing
        FireOrb(AimDirection(slow), slow, phase, homing: true);
        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator PlatformScatter(Phase phase)
    {
        Trigger("Slam");

        List<CrumblingPlatform> candidates = Strikeable();
        Shuffle(candidates);

        int count = Mathf.Min(phase.scatterCount, StrikeBudget(), candidates.Count);
        for (int i = 0; i < count; i++)
            candidates[i].Strike(phase.platformWarnTime, phase.platformDownTime);

        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator PlatformHunt(Phase phase)
    {
        Trigger("Slam");

        if (StrikeBudget() > 0)
        {
            // Nearest strikeable platform at or below the player's feet — the one they're on or about to land on
            Vector3 pp = player.transform.position;
            CrumblingPlatform best = null;
            float bestDist = float.MaxValue;

            foreach (var p in Strikeable())
            {
                Vector3 c = p.transform.position;
                if (c.y > pp.y + 1f) continue;

                float d = new Vector2(c.x - pp.x, c.z - pp.z).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = p; }
            }

            // Slightly shorter warning than a scatter strike: standing still gets punished, moving doesn't
            if (best != null)
                best.Strike(phase.platformWarnTime * 0.85f, phase.platformDownTime);
        }

        // Plus one random extra so the player can't just hop to the nearest neighbour every time
        if (StrikeBudget() > 0)
        {
            List<CrumblingPlatform> rest = Strikeable();
            if (rest.Count > 0)
                rest[UnityEngine.Random.Range(0, rest.Count)].Strike(phase.platformWarnTime, phase.platformDownTime);
        }

        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator PlatformSweep(Phase phase)
    {
        Trigger("Slam");

        int count = Mathf.Min(phase.scatterCount + 2, StrikeBudget());
        int start = UnityEngine.Random.Range(0, platforms.Length);
        int struck = 0;

        for (int i = 0; i < platforms.Length && struck < count; i++)
        {
            CrumblingPlatform p = platforms[(start + i) % platforms.Length];
            if (!p.CanBeStruck) continue;

            p.Strike(phase.platformWarnTime, phase.platformDownTime);
            struck++;
            yield return new WaitForSeconds(phase.sweepStep);
        }
    }

    // ── Attack Helpers ────────────────────────────────────────────────────────

    private Vector3 AimDirection(float speed)
    {
        Vector3 origin = muzzle.position;
        Vector3 target = playerCC.bounds.center;

        // CharacterController.velocity is the actual velocity from its last Move(), so no extra state needed
        float travelTime = Vector3.Distance(origin, target) / Mathf.Max(speed, 0.01f);
        target += playerCC.velocity * (travelTime * aimLead);

        return (target - origin).normalized;
    }

    private void FireOrb(Vector3 dir, float speed, Phase phase, bool homing)
    {
        if (orbPrefab == null || muzzle == null) return;

        NightmareOrb orb = Instantiate(orbPrefab, muzzle.position, Quaternion.LookRotation(dir));
        orb.Launch(dir, speed,
                   homing ? playerCC : null,
                   homing ? phase.homingTurnRate : 0f,
                   homing ? phase.homingDuration : 0f);

        activeOrbs.RemoveAll(o => o == null);
        activeOrbs.Add(orb);
    }

    private List<CrumblingPlatform> Strikeable()
    {
        var list = new List<CrumblingPlatform>();
        foreach (var p in platforms)
            if (p != null && p.CanBeStruck) list.Add(p);
        return list;
    }

    /// <summary>How many more platforms may go down right now without exceeding maxCrumbledFraction.</summary>
    private int StrikeBudget()
    {
        int total = 0, down = 0;
        foreach (var p in platforms)
        {
            if (p == null || p.Immune || p.IsPermanentlyCollapsed) continue;
            total++;
            if (!p.IsSolid) down++;
        }
        return Mathf.Max(0, Mathf.FloorToInt(total * maxCrumbledFraction) - down);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ── Bubbles ───────────────────────────────────────────────────────────────

    private IEnumerator BubbleLoop()
    {
        while (State == FightState.Attacking)
        {
            Phase phase = phases[PhaseIndex];

            if (activeBubbles.Count < phase.maxActiveBubbles && TrySpawnBubble())
                yield return new WaitForSeconds(phase.bubbleSpawnDelay);
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    private bool TrySpawnBubble()
    {
        if (bubblePrefab == null || bubbleSpawnPoints == null || bubbleSpawnPoints.Length == 0) return false;

        var free = new List<Transform>();
        foreach (var s in bubbleSpawnPoints)
            if (s != null && !activeBubbles.ContainsValue(s)) free.Add(s);
        if (free.Count == 0) return false;

        Transform slot = free[UnityEngine.Random.Range(0, free.Count)];
        DreamBubble bubble = Instantiate(bubblePrefab, slot.position, Quaternion.identity);
        bubble.Popped += OnBubblePopped;
        bubble.Expired += OnBubbleExpired;
        activeBubbles[bubble] = slot;
        return true;
    }

    private void ClearBubbles()
    {
        foreach (var bubble in activeBubbles.Keys)
            if (bubble != null) Destroy(bubble.gameObject);
        activeBubbles.Clear();
    }

    private void ClearOrbs()
    {
        foreach (var orb in activeOrbs)
            if (orb != null) orb.Dismiss();
        activeOrbs.Clear();
    }

    // ── Misc ──────────────────────────────────────────────────────────────────

    private void Trigger(string trigger)
    {
        if (animator != null) animator.SetTrigger(trigger);
    }

    private void OnDrawGizmosSelected()
    {
        if (bubbleSpawnPoints == null) return;

        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.8f);
        foreach (var s in bubbleSpawnPoints)
            if (s != null) Gizmos.DrawWireSphere(s.position, 1.2f);
    }
}
