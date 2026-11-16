using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays a random one-liner when the player respawns. Put this on the same object as
/// PlayerRespawner (or anywhere in the scene and assign the reference).
///
/// A quip is skipped when:
///  - the Sir Nightmare fight has begun (State is anything but Dormant),
///  - the player already respawned within minSecondsBetweenFalls (stops quips piling up
///    while someone is stuck on a hard jump),
///  - other dialogue is playing, checked immediately and again just before the quip plays.
///
/// Pixie quips are only in the pool while a PixieFollower exists in the scene AND is
/// following the player — so the tutorial (no Pixie) and any pre-meeting section only get
/// solo quips.
/// </summary>
public class RespawnQuipPlayer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Auto-found on this object, then in the scene, if left empty.")]
    [SerializeField] private PlayerRespawner respawner;

    [Tooltip("Optional. Quips stop for good once this fight leaves Dormant. Auto-found in the "
           + "scene if left empty; levels without a boss just won't find one.")]
    [SerializeField] private SirNightmareFight bossFight;

    [Header("Quips")]
    [Tooltip("Lines that work with or without Pixie around (the Sandman talking to himself).")]
    [SerializeField] private DialogueSequence[] soloQuips;

    [Tooltip("Conversational lines between Pixie and the Sandman. Only used while Pixie is "
           + "present and following.")]
    [SerializeField] private DialogueSequence[] pixieQuips;

    [Header("Timing")]
    [Tooltip("Seconds after the teleport before the quip starts — lets the fade finish and the "
           + "player see where they landed.")]
    [SerializeField] private float playDelay = 0.8f;

    [Tooltip("If the previous respawn was less than this many seconds ago, no quip plays.")]
    [SerializeField] private float minSecondsBetweenFalls = 20f;

    private Coroutine pendingQuip;
    private float lastRespawnTime = float.NegativeInfinity;
    private DialogueSequence lastQuip;
    private PixieFollower pixie;
    private readonly List<DialogueSequence> candidates = new List<DialogueSequence>();

    private void Awake()
    {
        if (respawner == null) respawner = GetComponent<PlayerRespawner>();
        if (respawner == null) respawner = FindFirstObjectByType<PlayerRespawner>();
    }

    private void OnEnable()
    {
        if (respawner != null) respawner.Respawned += HandleRespawned;
    }

    private void OnDisable()
    {
        if (respawner != null) respawner.Respawned -= HandleRespawned;
    }

    private void HandleRespawned()
    {
        // A new respawn always cancels a quip still waiting out its delay.
        if (pendingQuip != null)
        {
            StopCoroutine(pendingQuip);
            pendingQuip = null;
        }

        bool fellRecently = Time.time - lastRespawnTime < minSecondsBetweenFalls;
        lastRespawnTime = Time.time;

        if (fellRecently || BossFightActive() || DialogueBusy()) return;

        pendingQuip = StartCoroutine(PlayAfterDelay());
    }

    private IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(playDelay);
        pendingQuip = null;

        // Conditions can change during the delay (a trigger zone at the checkpoint may have
        // started its own dialogue, or the boss fight may have begun) — story beats win.
        if (BossFightActive() || DialogueBusy()) yield break;

        DialogueSequence quip = PickQuip();
        if (quip != null && DialogueManager.Instance.PlayDialogue(quip))
            lastQuip = quip;
    }

    private DialogueSequence PickQuip()
    {
        candidates.Clear();
        AddPlayable(soloQuips);
        if (PixiePresent()) AddPlayable(pixieQuips);

        // Avoid the same line twice in a row when there's a choice.
        if (candidates.Count > 1 && lastQuip != null) candidates.Remove(lastQuip);

        return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
    }

    private void AddPlayable(DialogueSequence[] source)
    {
        if (source == null) return;

        foreach (DialogueSequence s in source)
        {
            if (s == null) continue;
            if (s.playOnce && DialogueManager.Instance.HasPlayed(s)) continue;
            candidates.Add(s);
        }
    }

    private bool PixiePresent()
    {
        // Looked up lazily rather than cached once, in case she's spawned in after this
        // component wakes up. Only runs on respawn, so the cost doesn't matter.
        if (pixie == null) pixie = FindFirstObjectByType<PixieFollower>();
        return pixie != null && pixie.isActiveAndEnabled && pixie.IsFollowing;
    }

    private bool BossFightActive()
    {
        if (bossFight == null) bossFight = FindFirstObjectByType<SirNightmareFight>();
        return bossFight != null && bossFight.State != SirNightmareFight.FightState.Dormant;
    }

    private bool DialogueBusy() =>
        DialogueManager.Instance == null || DialogueManager.Instance.IsPlaying;
}
