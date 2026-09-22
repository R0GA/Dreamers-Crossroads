using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Bitflags for the player's traversal abilities. PlayerController gates input on these;
/// AbilityCrystal (and anything else — cutscenes, debug menus, save data) grants them by
/// calling PlayerController.UnlockAbility(...).
/// </summary>
[Flags]
public enum PlayerAbility
{
    None = 0,
    Jump = 1 << 0,
    Grapple = 1 << 1,
    Launch = 1 << 2,

    All = Jump | Grapple | Launch
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // ── References ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Tooltip("Point the grapple beam visually fires from (e.g. a hand bone or weapon muzzle). "
           + "Falls back to the camera position if left empty.")]
    [SerializeField] private Transform grappleOrigin;
    [SerializeField] private UnityEngine.VFX.VisualEffect sandVFX;
    [SerializeField] private Animator viewmodelAnimator;
    [Tooltip("Empty RectTransform childed to your UI Viewmodel at the firing point.")]
    [SerializeField] private RectTransform uiGrappleEmitter;

    [Tooltip("How far in front of the camera lens (in meters) the 3D beam should spawn.")]
    [SerializeField] private float emitterForwardOffset = 0.5f;

    // ── Look ──────────────────────────────────────────────────────────────────

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxPitch = 89f;

    // ── Ground Movement ───────────────────────────────────────────────────────

    [Header("Ground Movement")]
    [Tooltip("Top horizontal speed while grounded (m/s).")]
    [SerializeField] private float maxGroundSpeed = 7f;

    [Tooltip("How quickly horizontal speed builds on the ground. Higher = snappier.")]
    [SerializeField] private float groundAcceleration = 80f;

    [Tooltip("How aggressively friction bleeds off horizontal speed.")]
    [SerializeField] private float friction = 8f;

    [Tooltip("Below this speed, friction treats the player as moving at stopSpeed so they "
           + "come to a complete stop rather than slowing forever.")]
    [SerializeField] private float stopSpeed = 1.5f;

    // ── Air Movement ──────────────────────────────────────────────────────────

    [Header("Air Movement")]
    [Tooltip("Wish-speed cap in the air. Keep LOW (~0.85). High airAcceleration + low cap "
           + "is what produces Source-style air strafing.")]
    [SerializeField] private float maxAirSpeed = 0.85f;

    [Tooltip("Raw acceleration in the air. Must be high to work against the small maxAirSpeed.")]
    [SerializeField] private float airAcceleration = 800f;

    // ── Jump ──────────────────────────────────────────────────────────────────

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;

    [Tooltip("Holding jump fires again on landing, skipping friction and preserving speed.")]
    [SerializeField] private bool allowBunnyHop = false;

    [Tooltip("Seconds after leaving a ledge the player can still jump.")]
    [SerializeField][Range(0f, 0.3f)] private float coyoteTime = 0.12f;

    [Tooltip("Seconds before landing a jump press is queued and fires automatically.")]
    [SerializeField][Range(0f, 0.3f)] private float jumpBufferTime = 0.15f;

    // ── Ground Detection ──────────────────────────────────────────────────────

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private LayerMask groundMask = ~0;

    // ── Grapple ───────────────────────────────────────────────────────────────

    [Header("Grapple")]
    [Tooltip("Maximum range of the grapple beam.")]
    [SerializeField] private float maxGrappleDistance = 30f;

    [Tooltip("Constant inward pull force applied along the rope. Higher = stronger magnetism "
           + "toward the anchor; lower = looser, more pendulum-like swing.")]
    [SerializeField] private float grapplePullForce = 18f;

    [Tooltip("Damps velocity toward the anchor to prevent the player oscillating back and "
           + "forth. Increase if swinging feels bouncy; decrease for a springier rope.")]
    [SerializeField] private float grappleDamping = 4f;

    [Tooltip("How fast the rope shortens while the Grapple button is held.")]
    [SerializeField] private float reelSpeed = 10f;

    [Tooltip("Rope cannot shorten below this length.")]
    [SerializeField] private float minRopeLength = 1.5f;

    [Tooltip("Extra upward velocity added on top of jumpHeight when releasing off a grapple.")]
    [SerializeField] private float slingshotBoost = 4f;

    [Tooltip("Wish-speed cap while swinging. A bit higher than normal air control so the "
           + "player has meaningful directional input during a swing arc.")]
    [SerializeField] private float grappleAirControl = 2f;

    [SerializeField] private LayerMask grappleMask = ~0;

    // ── Grapple Feedback UI ───────────────────────────────────────────────────

    [Header("Grapple Feedback UI")]
    [Tooltip("Crosshair image whose rotation/color reflects what the player is currently looking at.")]
    [SerializeField] private Image crosshairImage;

    [Tooltip("Crosshair color during normal gameplay — not looking at anything grappleable.")]
    [SerializeField] private Color crosshairDefaultColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);

    [Tooltip("Crosshair color once within usable grapple range (maxGrappleDistance) — full opacity, white.")]
    [SerializeField] private Color crosshairTargetColor = Color.white;

    [Tooltip("How far (in degrees) the crosshair rotates when looking at a grapple anchor, "
           + "whether or not it's currently in range.")]
    [SerializeField] private float crosshairAnchorRotation = 45f;

    [Tooltip("How quickly the crosshair's rotation and color ease toward their target each frame. Higher = snappier.")]
    [SerializeField] private float crosshairTransitionSpeed = 12f;

    [Tooltip("Grapple anchors only trigger the crosshair rotation within this distance. Keep this a "
           + "bit larger than maxGrappleDistance so the crosshair gives some early warning as the "
           + "player approaches, without reacting to anchors clear across the map.")]
    [SerializeField] private float grappleHighlightRange = 45f;

    // ── Charge Launch ─────────────────────────────────────────────────────────

    [Header("Charge Launch")]
    [Tooltip("Launch speed on an instant release (zero charge).")]
    [SerializeField] private float minLaunchSpeed = 8f;

    [Tooltip("Launch speed at a full charge.")]
    [SerializeField] private float maxLaunchSpeed = 35f;

    [Tooltip("Seconds to reach a full charge.")]
    [SerializeField] private float chargeTime = 1.2f;

    [Tooltip("Movement speed multiplier while charging. Lower = more planted windup feel.")]
    [SerializeField][Range(0f, 1f)] private float chargeMoveScale = 0.25f;

    [Tooltip("Whether the player gets one bonus launch charge while airborne, usable once per "
           + "air-time and restored on landing (in addition to normal grounded charging).")]
    [SerializeField] private bool allowAirLaunch = true;

    // ── Launch Feedback UI ────────────────────────────────────────────────────

    [Header("Launch Feedback UI")]
    [Tooltip("Root object for the charge bar (e.g. its background panel). Enabled only while charging.")]
    [SerializeField] private GameObject chargeBarRoot;

    [Tooltip("Slider whose value (0-1) tracks charge progress. Using a Slider instead of a "
           + "fillAmount Image lets the fill graphic use a 9-sliced sprite that resizes cleanly "
           + "instead of stretching. Set the Slider's Min/Max Value to 0/1, and turn off its "
           + "Interactable and Navigation since it's display-only.")]
    [SerializeField] private Slider chargeBarSlider;

    // ── Interaction ───────────────────────────────────────────────────────────

    [Header("Interaction")]
    [Tooltip("Max distance the player can interact with something.")]
    [SerializeField] private float interactionRange = 3f;

    [Tooltip("Which layers count as interactable. Keep this on its own layer, separate from groundMask/grappleMask.")]
    [SerializeField] private LayerMask interactionMask = ~0;

    // ── Abilities ─────────────────────────────────────────────────────────────

    [Header("Abilities")]
    [Tooltip("Abilities the player has at the start of the scene. For a tutorial level, "
           + "set this to None and let AbilityCrystals unlock Jump/Grapple/Launch one at a "
           + "time. For any other level, leave this at All.")]
    [SerializeField] private PlayerAbility startingAbilities = PlayerAbility.All;

    // ── Public State (read by UI / VFX) ──────────────────────────────────────
    public float ChargeAmount => chargeAmount;
    public bool IsCharging => isCharging;
    public bool IsGrappling => grappleState == GrappleState.Attached;

    [Tooltip("Whatever the crosshair is currently over, if it implements IInteractable. Null otherwise.")]
    public IInteractable CurrentInteractable => currentInteractable;

    /// <summary>What the crosshair is currently reporting for grapple targeting. Drives crosshairImage internally; exposed for any other UI that wants it too.</summary>
    public GrappleTargetState CurrentGrappleTarget => grappleTargetState;

    /// <summary>Whether the player still has their bonus in-air launch charge available (always true while grounded and allowAirLaunch is used up mid-air).</summary>
    public bool AirLaunchAvailable => airLaunchAvailable;

    /// <summary>Currently unlocked abilities. Read-only from outside — grant abilities via UnlockAbility.</summary>
    public PlayerAbility UnlockedAbilities => unlockedAbilities;

    /// <summary>Fired once, right when an ability is newly granted (not on redundant re-grants). Useful for a "Jump Restored" banner, SFX, save data, etc.</summary>
    public event Action<PlayerAbility> AbilityUnlocked;

    /// <summary>True if the player currently has every flag in <paramref name="ability"/> (can pass a single flag or a combination).</summary>
    public bool HasAbility(PlayerAbility ability) => (unlockedAbilities & ~suppressedAbilities & ability) == ability;

    /// <summary>Grants an ability (or combination of abilities) to the player. Safe to call repeatedly — already-unlocked flags are ignored and won't re-fire the event.</summary>
    public void UnlockAbility(PlayerAbility ability)
    {
        PlayerAbility newlyGranted = ability & ~unlockedAbilities;
        if (newlyGranted == PlayerAbility.None) return;

        unlockedAbilities |= ability;
        AbilityUnlocked?.Invoke(newlyGranted);
    }

    // ── Temporary Suppression / Knockback (boss hits, curses, cutscenes) ──────

    /// <summary>Abilities that are unlocked but currently disabled by SuppressAbilities. HasAbility() already accounts for these.</summary>
    public PlayerAbility SuppressedAbilities => suppressedAbilities;

    /// <summary>Fired when SuppressAbilities disables something — good hook for a screen tint, sputtering-sand VFX, or a sting.</summary>
    public event Action<PlayerAbility> AbilitiesSuppressed;

    /// <summary>Fired when suppression ends (timer ran out, or Respawn cleared it).</summary>
    public event Action AbilitiesRestored;

    /// <summary>
    /// Temporarily disables abilities without revoking them. Suppressing Grapple drops any active
    /// rope; suppressing Launch cancels an in-progress charge. All suppressed flags share one timer,
    /// so a second hit extends the whole lockout to whichever duration is longer.
    /// </summary>
    public void SuppressAbilities(PlayerAbility abilities, float duration)
    {
        if (abilities == PlayerAbility.None || duration <= 0f) return;

        suppressedAbilities |= abilities;
        suppressTimer = Mathf.Max(suppressTimer, duration);
        AbilitiesSuppressed?.Invoke(abilities);
    }

    /// <summary>
    /// Hard interrupt: overwrites the player's velocity with a shove (horizontal direction + upward
    /// pop), drops the grapple, and cancels any launch charge. Vertical component of
    /// <paramref name="direction"/> is ignored — use <paramref name="upwardSpeed"/> instead.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float horizontalSpeed, float upwardSpeed)
    {
        if (grappleState == GrappleState.Attached)
            ReleaseGrapple();

        isCharging = false;
        chargeAmount = 0f;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) direction = -transform.forward;
        direction.Normalize();

        velocity = direction * horizontalSpeed;
        velocity.y = upwardSpeed;

        // Same trick as ExecuteLaunch: stops GroundCheck re-grounding us next frame,
        // which would clamp velocity.y to -2 and eat the knockback.
        jumpGraceTimer = 0.2f;
        isGrounded = false;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
    }

    /// <summary>Seconds since the grapple last detached (0 while attached). Lets things like dream bubbles pop on "swing, release, fly through" rather than only while the rope is live.</summary>
    public float SecondsSinceGrapple => IsGrappling ? 0f : Time.time - lastGrappleReleaseTime;

    /// <summary>Drops the grapple only if it's latched onto <paramref name="root"/> or one of its children. Called by things that stop existing (crumbling platforms, lullaby anchor) so the rope doesn't stay pinned to empty space.</summary>
    public void ReleaseGrappleFrom(Transform root)
    {
        if (grappleState == GrappleState.Attached && grappleAnchor != null && grappleAnchor.IsChildOf(root))
            ReleaseGrapple();
    }

    private void TickSuppression()
    {
        if (suppressedAbilities == PlayerAbility.None) return;

        suppressTimer -= Time.deltaTime;
        if (suppressTimer <= 0f)
        {
            ClearSuppression();
            return;
        }

        // Cut off anything that just got disabled mid-use
        if (grappleState == GrappleState.Attached && !HasAbility(PlayerAbility.Grapple))
            ReleaseGrapple();

        if (isCharging && !HasAbility(PlayerAbility.Launch))
        {
            isCharging = false;
            chargeAmount = 0f;
        }
    }

    private void ClearSuppression()
    {
        if (suppressedAbilities == PlayerAbility.None) return;

        suppressedAbilities = PlayerAbility.None;
        suppressTimer = 0f;
        AbilitiesRestored?.Invoke();
    }

    // ── Private: Components / Input ───────────────────────────────────────────

    private CharacterController cc;
    private PlayerInput pi;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction grappleAction;
    private InputAction chargeAction;
    private InputAction interactAction;
    private InputAction interactAltAction;
    private LineRenderer grappleLine;

    // ── Private: Interaction State ────────────────────────────────────────────

    private IInteractable currentInteractable;

    // ── Private: Ability State ────────────────────────────────────────────────

    private PlayerAbility unlockedAbilities;
    private PlayerAbility suppressedAbilities;
    private float suppressTimer;

    // ── Private: Core Movement State ──────────────────────────────────────────

    private Vector3 velocity;
    private float pitch;
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float jumpGraceTimer;

    // ── Private: Grapple State ────────────────────────────────────────────────

    private enum GrappleState { Idle, Attached }
    private GrappleState grappleState = GrappleState.Idle;
    private Vector3 grapplePoint;
    private float ropeLength;

    /// <summary>Reported by UpdateGrappleTargeting each frame for crosshair coloring.</summary>
    public enum GrappleTargetState { None, Highlighted, InRange }
    private GrappleTargetState grappleTargetState = GrappleTargetState.None;

    // Tracks a moving anchor: grapplePoint is recomputed from this each frame instead of
    // staying fixed at the world position it was hit at. Null means the anchor doesn't move
    // (or was hit by something with no transform worth tracking, which never happens in
    // practice — every collider has one — so this is really just a "still attached?" guard).
    private Transform grappleAnchor;
    private float lastGrappleReleaseTime = float.NegativeInfinity;
    private Vector3 grappleLocalOffset;

    // ── Private: Charge Launch State ─────────────────────────────────────────

    private float chargeAmount;
    private bool isCharging;

    // True until spent by starting a charge in midair; restored the moment the player lands.
    private bool airLaunchAvailable = true;

    // ── Private: Moving Platform State ────────────────────────────────────────

    // The platform currently under the player, tracked so its motion can be folded
    // into our own Move() call each frame instead of leaving the player behind.
    private Transform currentPlatform;
    private Vector3 platformPrevPosition;
    private Quaternion platformPrevRotation;

    // ═════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();

        moveAction = pi.actions["Move"];
        lookAction = pi.actions["Look"];
        jumpAction = pi.actions["Jump"];
        grappleAction = pi.actions["Grapple"];
        chargeAction = pi.actions["ChargeLaunch"];

        // InteractAlt is optional — FindAction (which the [] indexer wraps) returns null
        // rather than throwing, so InteractAlt-less interactables still work fine.
        interactAction = pi.actions["Interact"];
        interactAltAction = pi.actions.FindAction("InteractAlt");

        unlockedAbilities = startingAbilities;

        InitGrappleLine();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera) playerCamera.enabled = true;
    }

    private void Update()
    {
        TickSuppression();      // Must run before input handling so a freshly-suppressed ability is cut off this frame
        HandleLook();
        HandleInteraction();    // Reads look-updated camera forward, so must run after HandleLook
        GroundCheck();          // Sets isGrounded / coyoteTimer
        TickJumpBuffer();
        UpdateGrappleTargeting(); // Crosshair color feedback; uses look-updated camera forward
        HandleGrappleInput();   // Toggle grapple state
        HandleChargeLaunch();   // May call ExecuteLaunch() which overrides isGrounded — must run before HandleMovement
        HandleMovement();       // Reads all state set above, writes velocity

        if (viewmodelAnimator != null)
        {
            // Only play walk if grounded and moving horizontally
            bool isMoving = HorizontalSpeed() > 0.3f;
            viewmodelAnimator.SetBool("IsWalking", isGrounded && isMoving);

            // Pass ability holding states directly to the animator
            viewmodelAnimator.SetBool("IsGrappling", IsGrappling);
            viewmodelAnimator.SetBool("IsCharging", IsCharging);
        }
    }

    private void LateUpdate()
    {
        UpdateGrappleLine();

        // Platform delta — how far the platform we're standing on moved/rotated since
        // last frame. Computed here (after GroundCheck may have just re-anchored us
        // onto a new platform this frame) so the first frame on a platform always
        // yields zero delta instead of snapping the player.
        Vector3 platformDelta = Vector3.zero;
        if (currentPlatform != null && isGrounded)
        {
            Vector3 newPos = currentPlatform.position;
            Quaternion newRot = currentPlatform.rotation;

            platformDelta = newPos - platformPrevPosition;

            // Carry rotation too: spin the player's horizontal offset from the
            // platform's pivot by however much the platform turned this frame, and
            // apply that as extra positional delta plus a matching facing turn.
            Quaternion deltaRot = newRot * Quaternion.Inverse(platformPrevRotation);
            Vector3 offsetFromPivot = (transform.position + platformDelta) - newPos;
            Vector3 rotatedOffset = deltaRot * offsetFromPivot;
            platformDelta += rotatedOffset - offsetFromPivot;
            transform.Rotate(0f, deltaRot.eulerAngles.y, 0f);

            platformPrevPosition = newPos;
            platformPrevRotation = newRot;
        }

        Vector3 finalVelocity = velocity;

        // If the platform is actively pushing us upward, don't fight it with the -2f grounding velocity.
        // Doing so causes the player to sink into the collider and triggers depenetration jitter.
        if (currentPlatform != null && isGrounded && platformDelta.y > 0f)
        {
            finalVelocity.y = Mathf.Max(0f, finalVelocity.y);
        }

        // Pass finalVelocity instead of velocity
        CollisionFlags flags = cc.Move(platformDelta + finalVelocity * Time.deltaTime);

        // Kill upward velocity on ceiling hits so the player drops immediately
        if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0f)
            velocity.y = 0f;
    }

    // ── Look ──────────────────────────────────────────────────────────────────

    private void HandleLook()
    {
        Vector2 look = lookAction.ReadValue<Vector2>() * lookSensitivity;

        transform.Rotate(0f, look.x, 0f);

        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    // ── Interaction ───────────────────────────────────────────────────────────

    private void HandleInteraction()
    {
        currentInteractable = FindInteractable();

        if (currentInteractable == null) return;

        if (interactAction != null && interactAction.WasPressedThisFrame())
            currentInteractable.Interact(gameObject);
        else if (interactAltAction != null && interactAltAction.WasPressedThisFrame())
            currentInteractable.InteractAlt(gameObject);
    }

    private IInteractable FindInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange,
                            interactionMask, QueryTriggerInteraction.Ignore))
        {
            // GetComponentInParent so the collider can live on a child mesh while the
            // script lives on a pivot/root object — same pattern used elsewhere in this file.
            return hit.collider.GetComponentInParent<IInteractable>();
        }

        return null;
    }

    // ── Ground Detection ──────────────────────────────────────────────────────

    private void GroundCheck()
    {
        // Skip detection for a few frames after jumping/launching so the player
        // physically clears the surface before we can re-ground them
        if (jumpGraceTimer > 0f)
        {
            jumpGraceTimer -= Time.deltaTime;
            isGrounded = false;
            coyoteTimer = 0f;
            currentPlatform = null;
            return;
        }

        float castRadius = cc.radius * 0.9f;
        Vector3 bottom = transform.position + cc.center + Vector3.down * (cc.height * 0.5f - cc.radius);
        Vector3 origin = bottom + Vector3.up * 0.1f;

        // --- NEW FIX START ---
        float dynamicCastDist = groundCheckDistance;

        // If we are on a platform, check if it moved down since the last frame
        if (currentPlatform != null)
        {
            float verticalDelta = currentPlatform.position.y - platformPrevPosition.y;
            if (verticalDelta < 0f)
            {
                // Extend the raycast by exactly how far the platform fell, plus a tiny safety margin
                dynamicCastDist += Mathf.Abs(verticalDelta) + 0.05f;
            }
        }

        bool hit = Physics.SphereCast(
            origin, castRadius, Vector3.down, out RaycastHit groundHit,
            0.1f + dynamicCastDist, groundMask, QueryTriggerInteraction.Ignore);
        // --- NEW FIX END ---

        if (hit)
        {
            isGrounded = true;
            coyoteTimer = coyoteTime;
            airLaunchAvailable = true; // Landing restores the bonus in-air launch charge

            // Landing while grappling detaches the hook — prevents awkward sliding
            if (grappleState == GrappleState.Attached)
                ReleaseGrapple();

            // GetComponentInParent so the platform's collider can live on a child mesh
            // while MovingPlatform sits on the root — same pattern as FindInteractable.
            MovingPlatform platform = groundHit.collider.GetComponentInParent<MovingPlatform>();
            Transform platformRoot = platform != null ? platform.transform : null;

            // Only re-anchor when we land on a *different* platform (or the ground).
            // Re-grabbing the same transform every frame would reset platformPrevPosition
            // to the current position and the delta would always compute to zero.
            if (platformRoot != currentPlatform)
            {
                currentPlatform = platformRoot;
                if (currentPlatform != null)
                {
                    platformPrevPosition = currentPlatform.position;
                    platformPrevRotation = currentPlatform.rotation;
                }
            }
        }
        else
        {
            isGrounded = false;
            coyoteTimer -= Time.deltaTime;
            currentPlatform = null;
        }
    }

    // ── Jump Buffer ───────────────────────────────────────────────────────────

    private void TickJumpBuffer()
    {
        if (jumpAction.WasPressedThisFrame())
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;
    }

    private bool CanJump()
    {
        if (!HasAbility(PlayerAbility.Jump)) return false;

        // Charging (right click / ChargeLaunch held) locks out jumping. Previously a jump
        // fired mid-charge would knock isGrounded false for a couple of frames, and
        // HandleChargeLaunch's release check never ran while airborne — so releasing the
        // charge button mid-jump silently ate the charge instead of launching.
        if (isCharging) return false;

        bool hasInput = jumpBufferTimer > 0f || (allowBunnyHop && jumpAction.IsPressed());
        bool nearGround = coyoteTimer > 0f;
        return hasInput && nearGround;
    }

    // ── Grapple ───────────────────────────────────────────────────────────────

    private void HandleGrappleInput()
    {
        if (grappleAction.WasPressedThisFrame() && grappleState == GrappleState.Idle
            && HasAbility(PlayerAbility.Grapple))
            TryFireGrapple();

        if (grappleAction.WasReleasedThisFrame() && grappleState == GrappleState.Attached)
            ReleaseGrapple();
    }

    /// <summary>
    /// Separate from TryFireGrapple's raycast — this one runs every frame purely to drive
    /// crosshair feedback, using a longer range so the player gets an early "something's
    /// there" cue before they're actually close enough to fire at it.
    /// </summary>
    private void UpdateGrappleTargeting()
    {
        if (HasAbility(PlayerAbility.Grapple) && grappleState == GrappleState.Idle)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, grappleHighlightRange,
                                grappleMask, QueryTriggerInteraction.Ignore))
            {
                grappleTargetState = hit.distance <= maxGrappleDistance
                    ? GrappleTargetState.InRange
                    : GrappleTargetState.Highlighted;
            }
            else
            {
                grappleTargetState = GrappleTargetState.None;
            }
        }
        else if (grappleState == GrappleState.Attached)
        {
            // Latched on — hold the rotated "highlighted" look rather than resetting to
            // neutral or snapping to the full in-range tint, since the player isn't aiming
            // at anything anymore while swinging.
            grappleTargetState = GrappleTargetState.Highlighted;
        }
        else
        {
            grappleTargetState = GrappleTargetState.None;
        }

        if (crosshairImage == null) return;

        // Rotated whenever an anchor is in view at all (Highlighted or InRange); only
        // InRange swaps it to the full-opacity white "you can fire now" tint.
        float targetRotationZ = grappleTargetState == GrappleTargetState.None ? 0f : crosshairAnchorRotation;
        Color targetColor = grappleTargetState == GrappleTargetState.InRange
            ? crosshairTargetColor
            : crosshairDefaultColor;

        // Framerate-independent ease toward the target rotation/color rather than snapping,
        // so the crosshair doesn't pop jarringly as the player's look crosses an anchor edge.
        float t = 1f - Mathf.Exp(-crosshairTransitionSpeed * Time.deltaTime);

        float currentZ = crosshairImage.rectTransform.localEulerAngles.z;
        float newZ = Mathf.LerpAngle(currentZ, targetRotationZ, t);
        crosshairImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, newZ);

        crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, t);
    }

    private void TryFireGrapple()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance,
                            grappleMask, QueryTriggerInteraction.Ignore))
        {
            grapplePoint = hit.point;
            ropeLength = hit.distance;
            grappleState = GrappleState.Attached;

            // Remember the point relative to the collider's own transform, so if that
            // transform moves or rotates afterward, we can recompute where the hit point
            // now is instead of staying pinned to the world position it was fired at.
            grappleAnchor = hit.collider.transform;
            grappleLocalOffset = grappleAnchor.InverseTransformPoint(hit.point);
        }
    }

    private void ReleaseGrapple()
    {
        if (grappleState == GrappleState.Attached)
            lastGrappleReleaseTime = Time.time;

        grappleState = GrappleState.Idle;
        grappleAnchor = null;
    }

    private void ApplyGrapplePhysics()
    {
        // Follow the anchor if it's moved/rotated since last frame (or since it was hit,
        // for the very first frame). A static anchor just recomputes to the same point
        // it already had, so there's no need to special-case "moving vs. not."
        if (grappleAnchor != null)
            grapplePoint = grappleAnchor.TransformPoint(grappleLocalOffset);

        Vector3 toAnchor = grapplePoint - transform.position;
        float dist = toAnchor.magnitude;

        if (dist < 0.01f) { ReleaseGrapple(); return; }

        Vector3 dir = toAnchor / dist; // Toward anchor

        // Step 1 — Reel: always reel in while attached
        ropeLength = Mathf.Max(ropeLength - reelSpeed * Time.deltaTime, minRopeLength);

        // Step 2 — Constraint: cancel outward velocity when rope is taut
        if (dist > ropeLength)
        {
            float awaySpeed = Vector3.Dot(velocity, -dir); // positive = moving away
            if (awaySpeed > 0f)
                velocity += awaySpeed * dir;               // zero the outward component
        }

        // Step 3 — Constant pull toward anchor
        velocity += dir * grapplePullForce * Time.deltaTime;

        // Step 4 — Damp inward velocity to prevent oscillation past the anchor
        float inwardSpeed = Vector3.Dot(velocity, dir);
        if (inwardSpeed > 0f)
        {
            float damp = inwardSpeed * grappleDamping * Time.deltaTime;
            velocity -= dir * Mathf.Min(damp, inwardSpeed); // clamp so we never reverse
        }

        // Auto-detach when fully reeled in
        if (dist <= minRopeLength * 0.5f)
            ReleaseGrapple();
    }

    // ── Charge Launch ─────────────────────────────────────────────────────────

    private void HandleChargeLaunch()
    {
        if (grappleState == GrappleState.Attached)
        {
            SyncChargeBarUI();
            return; // Can't start or continue charging while grappling — prevents awkward edge cases
        }

        if (!isCharging)
        {
            // Grounded charging always works; airborne charging only works if the player
            // still has their one bonus air-launch charge (restored on landing).
            bool canStartCharging = isGrounded || (allowAirLaunch && airLaunchAvailable);

            if (chargeAction.WasPressedThisFrame() && HasAbility(PlayerAbility.Launch) && canStartCharging)
            {
                isCharging = true;
                chargeAmount = 0f;

                if (!isGrounded)
                    airLaunchAvailable = false; // Spend the air charge the moment charging starts
            }

            if (!isCharging)
            {
                SyncChargeBarUI();
                return;
            }
        }

        chargeAmount = Mathf.MoveTowards(chargeAmount, 1f, Time.deltaTime / chargeTime);

        if (chargeAction.WasReleasedThisFrame())
        {
            ExecuteLaunch();
            isCharging = false;
            chargeAmount = 0f;
        }

        SyncChargeBarUI();
    }

    private void SyncChargeBarUI()
    {
        if (chargeBarRoot != null) chargeBarRoot.SetActive(isCharging);
        if (chargeBarSlider != null) chargeBarSlider.value = chargeAmount;
    }

    private void ExecuteLaunch()
    {
        // Grapple and launch together feels chaotic — release first
        if (grappleState == GrappleState.Attached)
            ReleaseGrapple();

        Vector3 launchDir = playerCamera.transform.forward;
        float launchSpeed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, chargeAmount);

        // Preserve sideways/perpendicular momentum; only override the forward component.
        // This means strafing mid-charge still carries through into the launch.
        velocity = Vector3.ProjectOnPlane(velocity, launchDir) + launchDir * launchSpeed;

        // Clear grounded state so HandleMovement doesn't snap velocity.y = -2 this frame
        jumpGraceTimer = 0.2f;
        isGrounded = false;
        coyoteTimer = 0f;
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float moveScale = isCharging ? chargeMoveScale : 1f;

        Vector3 wishDir = transform.right * input.x + transform.forward * input.y;
        if (wishDir.sqrMagnitude > 1f) wishDir.Normalize();

        if (grappleState == GrappleState.Attached)
        {
            HandleGrappledMovement(wishDir);
        }
        else if (isGrounded)
        {
            HandleGroundedMovement(wishDir, moveScale);
        }
        else
        {
            HandleAirborneMovement(wishDir, moveScale);
        }
    }

    private void HandleGrappledMovement(Vector3 wishDir)
    {
        ApplyGravity();
        ApplyGrapplePhysics();

        // Light directional control so the player isn't helpless mid-swing
        Accelerate(wishDir, grappleAirControl, airAcceleration);

        // Slingshot jump — preserve swing speed and add vertical burst
        if (jumpBufferTimer > 0f && HasAbility(PlayerAbility.Jump))
        {
            ReleaseGrapple();

            float baseJump = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            velocity.y = Mathf.Max(velocity.y + slingshotBoost, baseJump);

            jumpBufferTimer = 0f;
            jumpGraceTimer = 0.15f;
        }
    }

    private void HandleGroundedMovement(Vector3 wishDir, float moveScale)
    {
        if (CanJump())
        {
            // Friction is intentionally skipped so horizontal speed carries through the jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            jumpGraceTimer = 0.15f;

            // Treat as airborne this frame so the jump velocity isn't overridden
            Accelerate(wishDir, maxAirSpeed, airAcceleration);
            ApplyGravity();
        }
        else
        {
            ApplyFriction();
            Accelerate(wishDir, maxGroundSpeed * moveScale, groundAcceleration);

            // Constant downward keeps the player seated on slopes and stops micro-bouncing
            velocity.y = -2f;
        }
    }

    private void HandleAirborneMovement(Vector3 wishDir, float moveScale)
    {
        ApplyGravity();
        Accelerate(wishDir, maxAirSpeed * moveScale, airAcceleration);
    }

    // ── Grapple Line Renderer ─────────────────────────────────────────────────

    private void InitGrappleLine()
    {
        grappleLine = GetComponent<LineRenderer>();
        if (grappleLine == null)
            grappleLine = gameObject.AddComponent<LineRenderer>();

        grappleLine.positionCount = 2;
        grappleLine.startWidth = 0.2f;
        grappleLine.endWidth = 0.2f;
        grappleLine.useWorldSpace = true;
        grappleLine.enabled = false;
        if (sandVFX != null) sandVFX.Stop();
        // Assign a material in the Inspector for the best look.
        // Without one Unity will use a pink/magenta default — hard to miss!
    }

    private void UpdateGrappleLine()
    {
        if (grappleState != GrappleState.Attached)
        {
            grappleLine.enabled = false;
            if (sandVFX != null && sandVFX.aliveParticleCount > 0) sandVFX.Stop();
            return;
        }

        Vector3 lineStart;

        // Convert the 2D UI position into a 3D world point in front of the camera
        if (uiGrappleEmitter != null)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, uiGrappleEmitter.position);
            lineStart = playerCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, emitterForwardOffset));
        }
        else if (grappleOrigin != null)
        {
            lineStart = grappleOrigin.position;
        }
        else
        {
            lineStart = playerCamera.transform.position;
        }

        grappleLine.enabled = true;
        grappleLine.SetPosition(0, lineStart);
        grappleLine.SetPosition(1, grapplePoint);

        if (sandVFX != null)
        {
            sandVFX.SetVector3("StartPoint", lineStart);
            sandVFX.SetVector3("EndPoint", grapplePoint);

            if (!sandVFX.HasAnySystemAwake())
                sandVFX.Play();
        }
    }

    // ── Respawn ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Teleports the player to a checkpoint and clears all momentum/mid-air state so they
    /// don't land carrying grapple swing speed, launch charge, or fall velocity from before
    /// they died. Called by PlayerRespawner — nothing else should move this transform
    /// directly while a CharacterController is attached.
    /// </summary>
    public void Respawn(Vector3 position, Quaternion rotation)
    {
        if (grappleState == GrappleState.Attached)
            ReleaseGrapple();

        isCharging = false;
        chargeAmount = 0f;
        airLaunchAvailable = true;
        ClearSuppression();
        velocity = Vector3.zero;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        jumpGraceTimer = 0f;

        // CharacterController fights direct transform edits while enabled — it needs to be
        // off for the teleport to actually stick instead of being resolved away next Move().
        cc.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        cc.enabled = true;
    }

    // ── Physics Helpers ───────────────────────────────────────────────────────

    private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
    {
        float currentSpeed = Vector3.Dot(new Vector3(velocity.x, 0f, velocity.z), wishDir);
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0f) return;

        float accelSpeed = Mathf.Min(accel * wishSpeed * Time.deltaTime, addSpeed);
        velocity.x += accelSpeed * wishDir.x;
        velocity.z += accelSpeed * wishDir.z;
    }

    private void ApplyFriction()
    {
        float speed = HorizontalSpeed();
        if (speed < 0.001f) return;

        float control = Mathf.Max(speed, stopSpeed);
        float newSpeed = Mathf.Max(speed - control * friction * Time.deltaTime, 0f);
        float scale = newSpeed / speed;

        velocity.x *= scale;
        velocity.z *= scale;
    }

    private void ApplyGravity() =>
        velocity.y += Physics.gravity.y * Time.deltaTime;

    private float HorizontalSpeed() =>
        new Vector3(velocity.x, 0f, velocity.z).magnitude;
}