using System;
using UnityEngine;

/// <summary>
/// Drives Pixie, the 2D companion character: follows the player on a loose leash with a
/// "magical" hover instead of a rigid ground-lock or a rotation-locked spot behind them,
/// switches her walk animation between front-view and back-view depending on whether the
/// player is currently seeing her front or her back, and swaps to her speaking animation
/// whenever DialogueManager reports her as the current speaker.
///
/// Setup:
/// - Put this on Pixie's root object. Her SpriteRenderer/Animator can live on this object or
///   a child — if they're on a child that should always face the camera, assign that child
///   as <see cref="visualRoot"/> and it'll be billboarded every frame.
/// - The Animator needs three bool parameters: "IsWalking", "IsBackView", "IsSpeaking".
///   Wire your controller so IsSpeaking (via an Any State transition) overrides walking, and
///   IsWalking/IsBackView pick between Idle-Front, Idle-Back, Walk-Front and Walk-Back.
/// - She only moves once <see cref="BeginFollowing"/> is called, since the player doesn't
///   meet her immediately. Animation and billboarding run regardless, though — so if you
///   wire her up to react to dialogue (see below) before she's following, her talking
///   animation still plays correctly during that first conversation. The easiest hookup:
///   assign <see cref="firstEncounterSequence"/> to whatever DialogueSequence plays when the
///   player first meets her (played via a DialogueTriggerZone or DialogueInteractable at her
///   spot) — this script starts following automatically the moment that sequence finishes.
///   You can also call BeginFollowing() directly from your own cutscene/quest code instead.
/// - Once following, she doesn't track the player's rotation — turning in place doesn't drag
///   her around behind them. She holds her current spot until the player moves far enough
///   away (<see cref="followDistance"/>), then catches up to a point that distance away along
///   the direction she was already sitting, nudged sideways by <see cref="sideOffset"/> so she
///   doesn't settle dead center behind the camera.
/// - For DialogueManager to know a line is hers, give that DialogueLine a speakerName matching
///   <see cref="pixieSpeakerName"/> (case-insensitive). Lines with any other speaker (or none)
///   just hold her current idle/walk pose while the conversation plays out. By default she
///   pauses in place to deliver her own lines — turn on <see cref="allowWalkAndTalk"/> if you'd
///   rather she keep following while she talks.
/// - For a section like a boss fight where you don't want her weaving around the player, call
///   <see cref="HoldAt"/> and she'll drift gently near a fixed anchor point instead of
///   following, until <see cref="ReleaseHold"/> is called (or <see cref="BeginFollowing"/> is
///   called again). The easiest hookup needs no extra scripting: assign
///   <see cref="holdOnSequenceEnd"/> to the DialogueSequence that plays right before the fight
///   and <see cref="holdOnSequenceEndPoint"/> to where she should wait — she'll automatically
///   move there the moment that sequence finishes. Assign
///   <see cref="releaseHoldOnSequenceEnd"/> the same way (e.g. a post-fight victory line) to
///   have her automatically resume following once that sequence ends.
/// </summary>
public class PixieFollower : MonoBehaviour
{
    // ── References ───────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The player to follow. Auto-found by tag if left empty.")]
    [SerializeField] private Transform player;

    [Tooltip("Tag used to find the player if the field above is left empty.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Animator driving Pixie's sprite. Auto-found on this object or its children if left empty.")]
    [SerializeField] private Animator animator;

    [Tooltip("Transform billboarded to face the camera each frame (keeps a flat 2D sprite "
           + "readable from any angle). Usually the object holding the SpriteRenderer. "
           + "Defaults to this transform if left empty.")]
    [SerializeField] private Transform visualRoot;

    [Tooltip("Camera used both for billboarding and, indirectly, for reasoning about what the "
           + "player currently sees. Falls back to Camera.main if left empty.")]
    [SerializeField] private Camera referenceCamera;

    // ── Follow ────────────────────────────────────────────────────────────────

    [Header("Follow")]
    [Tooltip("Leash range: how far the player can get from Pixie's current spot before she "
           + "moves to catch up. Below this distance she just holds her position — she does "
           + "NOT swing around behind the player as they turn in place, only when they've "
           + "actually walked far enough away.")]
    [SerializeField] private float followDistance = 2f;

    [Tooltip("Sideways nudge (relative to the player's current right) applied to where she "
           + "settles when catching up, so she doesn't end up lined up dead center behind the "
           + "player/camera. Only affects the catch-up point, not her held position.")]
    [SerializeField] private float sideOffset = 0.75f;

    [Tooltip("Horizontal follow smoothing. Higher = lazier/floatier, lower = snappier.")]
    [SerializeField] private float followSmoothTime = 0.35f;

    [Tooltip("If Pixie ever ends up farther than this from the player (e.g. the player just "
           + "grappled or launched across a gap), she snaps straight to the follow point "
           + "instead of visibly racing to catch up. Staying close to the player matters more "
           + "than a smooth catch-up here.")]
    [SerializeField] private float teleportDistance = 15f;

    [Tooltip("Minimum horizontal speed before she's considered 'walking' for animation purposes.")]
    [SerializeField] private float walkSpeedThreshold = 0.3f;

    // ── Hover / Grounding ────────────────────────────────────────────────────

    [Header("Hover / Grounding")]
    [Tooltip("Layers counted as ground when looking for something to hover above.")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Tooltip("How far straight down (from just above the player) the ground-check ray searches "
           + "for something to hover above. Generous on purpose — she's floaty, not "
           + "raycast-precise. This only extends the search downward; it never changes how high "
           + "above the player the ray starts, so it's safe to raise for deep drops without "
           + "risking her ray reaching up into a ceiling or the floor above.")]
    [SerializeField] private float groundCheckRange = 10f;

    [Tooltip("How high above the detected ground she hovers.")]
    [SerializeField] private float hoverHeight = 1.2f;

    [Tooltip("Vertical follow smoothing. Kept slower than the horizontal smoothing on purpose "
           + "so she floats over height changes instead of stepping with them.")]
    [SerializeField] private float heightSmoothTime = 0.5f;

    // ── View Switching ───────────────────────────────────────────────────────

    [Header("View Switching")]
    [Tooltip("How far 'ahead' of the player (dot product with player.forward, -1 to 1) Pixie "
           + "needs to be before she counts as being in front of them.")]
    [SerializeField][Range(-1f, 1f)] private float inFrontThreshold = 0.15f;

    [Tooltip("How closely Pixie's movement direction needs to line up with the player's facing "
           + "(dot product, -1 to 1) before she counts as 'moving with' them.")]
    [SerializeField][Range(-1f, 1f)] private float movingWithPlayerThreshold = 0.3f;

    // ── Hold Area ─────────────────────────────────────────────────────────────

    [Header("Hold Area")]
    [Tooltip("Speed of her gentle drift around the hold anchor while holding — keeps her from "
           + "looking like a statue during a long fight. Set the radius passed to HoldAt to 0 "
           + "for a fixed spot instead.")]
    [SerializeField] private float holdWanderSpeed = 0.15f;

    // ── Dialogue ──────────────────────────────────────────────────────────────

    [Header("Dialogue")]
    [Tooltip("Matched against DialogueLine.speakerName (case-insensitive) to know when a line "
           + "is hers and she should play her speaking animation.")]
    [SerializeField] private string pixieSpeakerName = "Pixie";

    [Tooltip("While ANY dialogue is playing (not just her own lines), hold her in place instead "
           + "of following — stops her drifting around mid-conversation. Ignored entirely while "
           + "allowWalkAndTalk is on.")]
    [SerializeField] private bool freezeDuringAnyDialogue = true;

    [Tooltip("If true, she keeps following/walking while she delivers her lines instead of "
           + "pausing to talk. Turn this on for sections with a lot of dialogue and not much "
           + "standing still. Overrides freezeDuringAnyDialogue above.")]
    [SerializeField] private bool allowWalkAndTalk = false;

    [Tooltip("Optional. The sequence that plays when the player first meets Pixie. The moment "
           + "this sequence ends, she automatically starts following — no extra wiring needed. "
           + "Leave empty if you'd rather call BeginFollowing() yourself.")]
    [SerializeField] private DialogueSequence firstEncounterSequence;

    [Tooltip("Optional. When this sequence finishes, Pixie stops following and heads to "
           + "holdOnSequenceEndPoint to wait there instead (see HoldAt) — handy for a boss-fight "
           + "intro line where you don't want her weaving around the arena mid-fight. Leave "
           + "empty if you don't need this.")]
    [SerializeField] private DialogueSequence holdOnSequenceEnd;

    [Tooltip("Where she moves to and waits once holdOnSequenceEnd finishes. Required if "
           + "holdOnSequenceEnd is set.")]
    [SerializeField] private Transform holdOnSequenceEndPoint;

    [Tooltip("Wander radius passed to HoldAt when holdOnSequenceEnd triggers the hold. 0 = "
           + "stand dead still at holdOnSequenceEndPoint.")]
    [SerializeField] private float holdOnSequenceEndRadius = 0.5f;

    [Tooltip("Optional. When this sequence finishes (e.g. a post-fight victory line), she's "
           + "released from the hold and automatically resumes following. Leave empty if you'd "
           + "rather call ReleaseHold() yourself.")]
    [SerializeField] private DialogueSequence releaseHoldOnSequenceEnd;

    // ── Public State ─────────────────────────────────────────────────────────

    public bool IsFollowing { get; private set; }
    public bool IsSpeaking { get; private set; }

    /// <summary>True while she's holding near a fixed anchor (see HoldAt) instead of following the player.</summary>
    public bool IsHolding { get; private set; }

    /// <summary>Fired once, the moment BeginFollowing() actually starts her following.</summary>
    public event Action FollowingBegan;

    // ── Private State ────────────────────────────────────────────────────────

    private Vector3 horizontalVelocity;
    private float heightVelocity;
    private bool isWalking;
    private bool isBackView;
    private Transform holdAnchor;
    private float holdRadius;
    private float holdNoiseSeed;
    private bool isCatchingUp;
    private Vector3 catchUpDir;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (player == null)
        {
            GameObject tagged = GameObject.FindGameObjectWithTag(playerTag);
            if (tagged != null) player = tagged.transform;
        }

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (visualRoot == null) visualRoot = transform;
        if (referenceCamera == null) referenceCamera = Camera.main;

        // Per-instance offset so, if there's ever more than one hold-drifting object, they
        // don't all trace the exact same Perlin curve in lockstep.
        holdNoiseSeed = UnityEngine.Random.Range(0f, 1000f);
    }

    private void Start()
    {
        // By the time Start runs, DialogueManager.Awake() is guaranteed to have finished.
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("PixieFollower could not find DialogueManager.Instance!");
            return;
        }

        DialogueManager.Instance.LineStarted += HandleLineStarted;
        DialogueManager.Instance.DialogueEnded += HandleDialogueEnded;
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance == null) return;

        DialogueManager.Instance.LineStarted -= HandleLineStarted;
        DialogueManager.Instance.DialogueEnded -= HandleDialogueEnded;
    }

    private void Update()
    {
        if (player == null) return;

        if (IsHolding)
        {
            // Holding near a fixed anchor (boss fight etc). She still reacts to her own
            // dialogue lines by pausing, same as while following, but she's not chasing the
            // player at all here so there's no leash/catch-up logic to run.
            if (IsSpeaking && !allowWalkAndTalk)
            {
                horizontalVelocity = Vector3.zero;
            }
            else
            {
                UpdateHoldPosition();
            }

            isWalking = false;
            UpdateBillboard();
            UpdateAnimator();
            return;
        }

        // Note this is no longer gated on IsFollowing. Before she's attached (i.e. during the
        // first-encounter sequence itself) she should still billboard and react to
        // IsSpeaking — she just shouldn't move. Gating the whole method on IsFollowing meant
        // her talking animation never actually reached the Animator until BeginFollowing()
        // ran at the end of the sequence, so she sat there in her default pose while
        // "speaking" every one of her lines.
        //
        // allowWalkAndTalk only ever applies to the dialogue-driven holds below — it should
        // never let her start walking before BeginFollowing() has actually been called.
        bool dialogueHold = !allowWalkAndTalk && (IsSpeaking || (freezeDuringAnyDialogue
            && DialogueManager.Instance != null
            && DialogueManager.Instance.IsPlaying));

        bool holdInPlace = !IsFollowing || dialogueHold;

        if (holdInPlace)
        {
            // Stay put and let the idle/speak animation carry it, rather than walking and
            // talking at the same time.
            horizontalVelocity = Vector3.zero;
            isWalking = false;
        }
        else
        {
            UpdateFollowPosition();
            UpdateView();
        }

        UpdateBillboard();
        UpdateAnimator();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Starts her following the player. Safe to call more than once — later calls are ignored
    /// once she's already following.
    /// </summary>
    public void BeginFollowing()
    {
        // Calling this directly (rather than ReleaseHold()) is a valid way to snap her out of
        // a hold too, so make sure that state gets cleared either way.
        if (IsHolding)
        {
            IsHolding = false;
            holdAnchor = null;
        }

        if (IsFollowing) return;

        IsFollowing = true;
        horizontalVelocity = Vector3.zero;
        heightVelocity = 0f;
        isCatchingUp = false;
        FollowingBegan?.Invoke();
    }

    /// <summary>Stops her following in place — e.g. for a later cutscene where she needs to hold position.</summary>
    public void StopFollowing() => IsFollowing = false;

    /// <summary>
    /// Stops her following the player and has her drift near a fixed anchor instead — e.g. so
    /// she doesn't weave around during a boss fight. While holding she hovers over the ground
    /// beneath the anchor the same way she does while following, and gently wanders within
    /// <paramref name="radius"/> of it at <see cref="holdWanderSpeed"/> so she doesn't look
    /// frozen. Pass a radius of 0 to have her sit dead still at the anchor instead.
    /// Safe to call again with a new anchor to re-target an existing hold.
    /// </summary>
    public void HoldAt(Transform anchor, float radius = 1f)
    {
        if (anchor == null)
        {
            Debug.LogWarning("PixieFollower.HoldAt was called with a null anchor — ignoring.", this);
            return;
        }

        holdAnchor = anchor;
        holdRadius = Mathf.Max(0f, radius);
        IsHolding = true;
        IsFollowing = false;
        horizontalVelocity = Vector3.zero;
    }

    /// <summary>
    /// Releases her from the current hold anchor (see <see cref="HoldAt"/>) and has her resume
    /// following the player from wherever she's currently standing. Does nothing if she isn't
    /// currently holding.
    /// </summary>
    public void ReleaseHold()
    {
        if (!IsHolding) return;

        IsHolding = false;
        holdAnchor = null;
        horizontalVelocity = Vector3.zero;
        heightVelocity = 0f;
        isCatchingUp = false;
        IsFollowing = true;
    }

    // ── Movement ─────────────────────────────────────────────────────────────

    private void UpdateFollowPosition()
    {
        Vector3 playerFlat = new Vector3(player.position.x, 0f, player.position.z);
        Vector3 currentFlat = new Vector3(transform.position.x, 0f, transform.position.z);

        Vector3 offset = currentFlat - playerFlat;
        float distanceFromPlayer = offset.magnitude;

        Vector3 targetFlat;

        if (distanceFromPlayer > followDistance)
        {
            // Out of leash range — catch up, but along the direction she's already sitting
            // relative to the player, not the player's current facing. That's the key
            // difference from the old behind-the-player calculation: spinning the player in
            // place changes player.forward every frame, which used to drag her around in a
            // circle. Now she only moves once she's actually been left behind, and she closes
            // the gap from wherever she happens to be.
            //
            // The direction is cached once, right when she first falls out of range, rather
            // than recomputed every frame from her live position. Recomputing it every frame
            // created a feedback loop: as SmoothDamp nudged her toward the sideOffset catch-up
            // point, her offset-from-player angle shifted a little, which shifted the target a
            // little, which pulled her further sideways — a slow inward spiral toward the
            // offset point instead of a straight approach. Locking the direction in for the
            // duration of this catch-up removes that feedback: the target becomes a fixed
            // point relative to the player, and she settles onto it in a straight line.
            if (!isCatchingUp)
            {
                catchUpDir = distanceFromPlayer > 0.0001f
                    ? offset / distanceFromPlayer
                    : -new Vector3(player.forward.x, 0f, player.forward.z).normalized; // degenerate case: she's exactly on top of the player
                isCatchingUp = true;
            }

            Vector3 side = new Vector3(player.right.x, 0f, player.right.z).normalized;
            targetFlat = playerFlat + catchUpDir * followDistance + side * sideOffset;
        }
        else
        {
            // Within leash range — hold the spot she's already at. This is what stops her
            // rotating around the player and ending up permanently behind/off-screen. Clearing
            // the flag here means the next time she falls behind, the direction gets
            // re-cached fresh from wherever she's holding.
            isCatchingUp = false;
            targetFlat = currentFlat;
        }

        float groundedY = FindGroundedHeight(targetFlat);
        float targetHeight = groundedY + hoverHeight;

        if (Vector3.Distance(currentFlat, targetFlat) > teleportDistance)
        {
            transform.position = new Vector3(targetFlat.x, targetHeight, targetFlat.z);
            horizontalVelocity = Vector3.zero;
            heightVelocity = 0f;
            isCatchingUp = false;
            return;
        }

        Vector3 smoothedFlat = Vector3.SmoothDamp(currentFlat, targetFlat, ref horizontalVelocity, followSmoothTime);
        float smoothedHeight = Mathf.SmoothDamp(transform.position.y, targetHeight, ref heightVelocity, heightSmoothTime);

        transform.position = new Vector3(smoothedFlat.x, smoothedHeight, smoothedFlat.z);
    }

    /// <summary>
    /// Looks for ground beneath the given point so Pixie can hover a fixed height above it.
    /// Falls back to the player's own height when nothing is found nearby (a chasm, a gap the
    /// player just grappled over, etc.) — she'd rather stay near the player than nose-dive
    /// looking for a floor that isn't there.
    /// </summary>
    private float FindGroundedHeight(Vector3 point)
    {
        // point.x/point.z are meaningful, but point.y is always 0 here — UpdateFollowPosition
        // and UpdateHoldPosition both flatten their targets before calling this.
        //
        // The ray origin's height must NOT scale with groundCheckRange. It used to start at
        // "player height + groundCheckRange * 0.5", so raising groundCheckRange also raised the
        // origin — high enough, on a level with a ceiling/upper floor/overhang above the player,
        // to end up above that overhead geometry. Raycasting down from up there hits the
        // underside... no, the topside of that ceiling first (closest hit wins), which then
        // gets treated as "ground" and hovered above — exactly the "floats into nowhere, comes
        // back down when I lower the range" symptom. A fixed, small clearance instead means
        // groundCheckRange only ever extends how far DOWN the search goes, never how far up the
        // origin sits, so it can't climb above overhead geometry.
        const float rayOriginClearance = 1f;
        Vector3 rayOrigin = new Vector3(point.x, player.position.y + rayOriginClearance, point.z);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckRange, groundMask))
            return hit.point.y;

        return player.position.y;
    }

    /// <summary>
    /// Moves her toward a gently wandering point near <see cref="holdAnchor"/> instead of
    /// chasing the player — used while <see cref="IsHolding"/> is true (see
    /// <see cref="HoldAt"/>). Reuses the same hover-above-ground and smoothing/teleport logic
    /// as <see cref="UpdateFollowPosition"/> so she reads consistently whether she's following
    /// or holding.
    /// </summary>
    private void UpdateHoldPosition()
    {
        Vector3 anchorFlat = new Vector3(holdAnchor.position.x, 0f, holdAnchor.position.z);
        Vector3 targetFlat = anchorFlat;

        if (holdRadius > 0.0001f)
        {
            // Slow Perlin-noise drift within holdRadius of the anchor, so a long boss fight
            // doesn't leave her looking like a frozen statue. Two independent samples (t, seed)
            // and (seed, t) so X and Z wander separately instead of tracing a neat circle.
            float t = Time.time * holdWanderSpeed;
            float noiseX = Mathf.PerlinNoise(t, holdNoiseSeed) * 2f - 1f;
            float noiseZ = Mathf.PerlinNoise(holdNoiseSeed, t) * 2f - 1f;
            targetFlat += new Vector3(noiseX, 0f, noiseZ) * holdRadius;
        }

        float groundedY = FindGroundedHeight(targetFlat);
        float targetHeight = groundedY + hoverHeight;

        Vector3 currentFlat = new Vector3(transform.position.x, 0f, transform.position.z);

        if (Vector3.Distance(currentFlat, targetFlat) > teleportDistance)
        {
            transform.position = new Vector3(targetFlat.x, targetHeight, targetFlat.z);
            horizontalVelocity = Vector3.zero;
            heightVelocity = 0f;
            return;
        }

        Vector3 smoothedFlat = Vector3.SmoothDamp(currentFlat, targetFlat, ref horizontalVelocity, followSmoothTime);
        float smoothedHeight = Mathf.SmoothDamp(transform.position.y, targetHeight, ref heightVelocity, heightSmoothTime);

        transform.position = new Vector3(smoothedFlat.x, smoothedHeight, smoothedFlat.z);
    }

    /// <summary>
    /// Decides which walk/idle view to show: back-view only when she's both ahead of the
    /// player (along the player's facing direction) and moving in roughly the same direction
    /// as the player is facing — i.e. the player would actually be looking at her back.
    /// Everything else defaults to the front view.
    /// </summary>
    private void UpdateView()
    {
        Vector3 toPixieFlat = new Vector3(
            transform.position.x - player.position.x,
            0f,
            transform.position.z - player.position.z);

        bool inFront = toPixieFlat.sqrMagnitude > 0.0001f
            && Vector3.Dot(toPixieFlat.normalized, player.forward) > inFrontThreshold;

        float speed = horizontalVelocity.magnitude;
        isWalking = speed > walkSpeedThreshold;

        bool movingWithPlayer = isWalking
            && Vector3.Dot(horizontalVelocity.normalized, player.forward) > movingWithPlayerThreshold;

        isBackView = inFront && movingWithPlayer;
    }

    /// <summary>Keeps the sprite facing the camera on the horizontal plane, like any billboarded 2D character in a 3D scene.</summary>
    private void UpdateBillboard()
    {
        if (referenceCamera == null) return;

        Vector3 toCamera = referenceCamera.transform.position - visualRoot.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude < 0.0001f) return;

        visualRoot.rotation = Quaternion.LookRotation(toCamera.normalized);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsBackView", isBackView);
        animator.SetBool("IsSpeaking", IsSpeaking);
    }

    // ── Dialogue Event Handlers ──────────────────────────────────────────────

    private void HandleLineStarted(DialogueLine line)
    {
        IsSpeaking = !string.IsNullOrEmpty(line.speakerName)
            && string.Equals(line.speakerName.Trim(), pixieSpeakerName, StringComparison.OrdinalIgnoreCase);
    }

    private void HandleDialogueEnded(DialogueSequence sequence)
    {
        IsSpeaking = false;

        if (!IsFollowing && firstEncounterSequence != null && sequence == firstEncounterSequence)
        {
            BeginFollowing();
            return;
        }

        if (holdOnSequenceEnd != null && sequence == holdOnSequenceEnd)
        {
            if (holdOnSequenceEndPoint != null)
                HoldAt(holdOnSequenceEndPoint, holdOnSequenceEndRadius);
            else
                Debug.LogWarning("PixieFollower has holdOnSequenceEnd set but no "
                    + "holdOnSequenceEndPoint — nothing to hold at.", this);

            return;
        }

        if (releaseHoldOnSequenceEnd != null && sequence == releaseHoldOnSequenceEnd)
            ReleaseHold();
    }
}