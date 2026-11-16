using UnityEngine;

/// <summary>
/// Drives all first-person movement audio off PlayerController's public state — footsteps,
/// a charge-up loop, a grapple/swing loop, and a speed-driven wind loop. Reads PlayerController
/// every frame rather than subscribing to events, since all four sounds care about continuous
/// state (are we grounded and moving, are we charging, how fast are we going) rather than
/// one-off triggers.
///
/// Put this on the same GameObject as PlayerController. Assign an AudioClip to each looping
/// AudioSource in the Inspector (Loop = on, Play On Awake = off) — this script starts them
/// playing at volume 0 and only ever fades their volume, so a sound never "pops" in mid-clip
/// when it turns on.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerAudio : MonoBehaviour
{
    // ── Footsteps ─────────────────────────────────────────────────────────────

    [Header("Footsteps")]
    [Tooltip("One-shot footstep sounds. A random clip is picked each step; use several for variety.")]
    [SerializeField] private AudioClip[] footstepClips;

    [Tooltip("AudioSource that PlayOneShot fires footsteps through. Loop should be OFF.")]
    [SerializeField] private AudioSource footstepSource;

    [Tooltip("Horizontal distance (meters) the player must cover between footsteps. Lower = quicker cadence.")]
    [SerializeField] private float strideLength = 2f;

    [Tooltip("Below this horizontal speed, footsteps stop even if grounded (matches the walk-animation threshold in PlayerController).")]
    [SerializeField] private float minSpeedForFootsteps = 0.3f;

    [SerializeField][Range(0f, 1f)] private float footstepVolume = 0.8f;

    [Tooltip("Random pitch range applied to each step so footsteps don't sound identical on repeat.")]
    [SerializeField] private Vector2 footstepPitchRange = new Vector2(0.95f, 1.05f);

    private float distanceSinceLastStep;
    private Vector3 lastPosition;

    // ── Charge ────────────────────────────────────────────────────────────────

    [Header("Charge (Launch Windup)")]
    [Tooltip("Looping charge-up AudioSource. Loop should be ON, Play On Awake OFF — this script calls Play() itself at volume 0.")]
    [SerializeField] private AudioSource chargeSource;

    [SerializeField][Range(0f, 1f)] private float chargeMaxVolume = 1f;

    [Tooltip("How fast charge volume fades in/out, in volume-units per second.")]
    [SerializeField] private float chargeFadeSpeed = 4f;

    [Tooltip("Pitch at the start of a charge (chargeAmount = 0).")]
    [SerializeField] private float chargeMinPitch = 0.85f;

    [Tooltip("Pitch at full charge (chargeAmount = 1) — gives the classic rising-whine feel.")]
    [SerializeField] private float chargeMaxPitch = 1.4f;

    // ── Grapple ───────────────────────────────────────────────────────────────

    [Header("Grapple (Swinging)")]
    [Tooltip("Looping grapple/swing AudioSource. Loop should be ON, Play On Awake OFF.")]
    [SerializeField] private AudioSource grappleSource;

    [SerializeField][Range(0f, 1f)] private float grappleMaxVolume = 1f;

    [Tooltip("How fast grapple volume fades in/out, in volume-units per second.")]
    [SerializeField] private float grappleFadeSpeed = 5f;

    // ── Wind ──────────────────────────────────────────────────────────────────

    [Header("Wind (Speed-Based)")]
    [Tooltip("Looping wind AudioSource. Loop should be ON, Play On Awake OFF.")]
    [SerializeField] private AudioSource windSource;

    [Tooltip("Speed (m/s) below which wind is silent. Reads PlayerController.Speed (full 3D speed), so a straight-up launch or a fast fall counts too, not just horizontal movement.")]
    [SerializeField] private float windSpeedThreshold = 10f;

    [Tooltip("Speed (m/s) at which wind volume hits its cap. Speeds above this don't get any louder.")]
    [SerializeField] private float windSpeedForMaxVolume = 30f;

    [SerializeField][Range(0f, 1f)] private float windMaxVolume = 1f;

    [Tooltip("How fast wind volume eases toward its target, in volume-units per second. Lower = smoother/laggier swells.")]
    [SerializeField] private float windFadeSpeed = 3f;

    // ═════════════════════════════════════════════════════════════════════════

    private PlayerController pc;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
    }

    private void Start()
    {
        lastPosition = transform.position;

        // Start the looping sources playing now, at silence, so turning them "on" later is
        // just a volume fade rather than a Play() call — no restart pop, no missed attack transient.
        PrimeLoopingSource(chargeSource);
        PrimeLoopingSource(grappleSource);
        PrimeLoopingSource(windSource);
    }

    private void PrimeLoopingSource(AudioSource source)
    {
        if (source == null || source.clip == null) return;
        source.loop = true;
        source.volume = 0f;
        if (!source.isPlaying) source.Play();
    }

    private void Update()
    {
        HandleFootsteps();
        HandleChargeAudio();
        HandleGrappleAudio();
        HandleWindAudio();
    }

    // ── Footsteps ─────────────────────────────────────────────────────────────

    private void HandleFootsteps()
    {
        // Only accumulate stride distance while grounded and moving — an airborne player
        // (jumping, launching, swinging) shouldn't rack up a "free" footstep on landing.
        if (!pc.IsGrounded || pc.HorizontalSpeed < minSpeedForFootsteps)
        {
            distanceSinceLastStep = 0f;
            lastPosition = transform.position;
            return;
        }

        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        distanceSinceLastStep += delta.magnitude;
        lastPosition = transform.position;

        if (distanceSinceLastStep >= strideLength)
        {
            distanceSinceLastStep = 0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        if (footstepSource == null || footstepClips == null || footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepSource.pitch = Random.Range(footstepPitchRange.x, footstepPitchRange.y);
        footstepSource.PlayOneShot(clip, footstepVolume);
    }

    // ── Charge ────────────────────────────────────────────────────────────────

    private void HandleChargeAudio()
    {
        if (chargeSource == null) return;

        float target = pc.IsCharging ? chargeMaxVolume : 0f;
        chargeSource.volume = Mathf.MoveTowards(chargeSource.volume, target, chargeFadeSpeed * Time.deltaTime);

        if (pc.IsCharging)
            chargeSource.pitch = Mathf.Lerp(chargeMinPitch, chargeMaxPitch, pc.ChargeAmount);
    }

    // ── Grapple ───────────────────────────────────────────────────────────────

    private void HandleGrappleAudio()
    {
        if (grappleSource == null) return;

        float target = pc.IsGrappling ? grappleMaxVolume : 0f;
        grappleSource.volume = Mathf.MoveTowards(grappleSource.volume, target, grappleFadeSpeed * Time.deltaTime);
    }

    // ── Wind ──────────────────────────────────────────────────────────────────

    private void HandleWindAudio()
    {
        if (windSource == null) return;

        // Full 3D speed, not just horizontal — a straight-up launch or a long fall should
        // whistle too, and this stays independent of whatever state (charging/grappling/free
        // air control) produced the speed, so it layers under those sounds automatically.
        float speed = pc.Speed;

        float target;
        if (speed <= windSpeedThreshold)
        {
            target = 0f;
        }
        else
        {
            // Smoothstep rather than a linear ramp: quiet and gentle just above the threshold,
            // then builds more noticeably as speed climbs toward the cap.
            float t = Mathf.InverseLerp(windSpeedThreshold, windSpeedForMaxVolume, speed);
            t = t * t * (3f - 2f * t);
            target = Mathf.Lerp(0f, windMaxVolume, t);
        }

        windSource.volume = Mathf.MoveTowards(windSource.volume, target, windFadeSpeed * Time.deltaTime);
    }
}