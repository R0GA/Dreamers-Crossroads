using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Source-engine inspired first-person player controller for Unity 6.
///
/// Movement is fully velocity-based using the Quake/Source Accelerate() function.
/// This produces fluid, momentum-driven motion with proper air strafing.
///
/// Key concepts:
///   - Ground movement: high acceleration + friction = responsive but not instant
///   - Air movement:    low wishSpeed cap + very high acceleration = strafing works,
///                      raw speed gain is capped (classic Quake/Source feel)
///   - Bunny hop:       friction is skipped on the jump frame, preserving speed
///   - Coyote time:     lets players jump slightly after walking off a ledge
///   - Jump buffer:     queues a jump slightly before landing
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // ── References ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    // ── Look ──────────────────────────────────────────────────────────────────

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxPitch = 89f;

    // ── Ground Movement ───────────────────────────────────────────────────────

    [Header("Ground Movement")]
    [Tooltip("Top horizontal speed while grounded (m/s).")]
    [SerializeField] private float maxGroundSpeed = 7f;

    [Tooltip("How quickly horizontal speed builds up on the ground. Higher = snappier.")]
    [SerializeField] private float groundAcceleration = 80f;

    [Tooltip("How aggressively friction bleeds off horizontal speed. Higher = stops faster.")]
    [SerializeField] private float friction = 8f;

    [Tooltip("Below this speed, friction treats the player as moving at stopSpeed so they "
           + "fully stop instead of slowing to a crawl forever.")]
    [SerializeField] private float stopSpeed = 1.5f;

    // ── Air Movement ──────────────────────────────────────────────────────────

    [Header("Air Movement")]
    [Tooltip("Maximum wish-speed cap while airborne. Keep this LOW (Source uses ~0.85 m/s). "
           + "Combined with the high airAcceleration this is what makes air-strafing work: "
           + "you can nudge velocity sideways but cannot gain raw forward speed freely.")]
    [SerializeField] private float maxAirSpeed = 0.85f;

    [Tooltip("Raw acceleration multiplier in the air. Must be very high to counteract the "
           + "small maxAirSpeed cap and produce responsive strafing.")]
    [SerializeField] private float airAcceleration = 800f;

    // ── Jump ──────────────────────────────────────────────────────────────────

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;

    [Tooltip("When enabled, holding jump fires again the instant the player lands, "
           + "skipping the friction frame and preserving speed (bunny hopping).")]
    [SerializeField] private bool allowBunnyHop = false;

    [Tooltip("Seconds after leaving a ledge during which the player can still jump.")]
    [SerializeField][Range(0f, 0.3f)] private float coyoteTime = 0.12f;

    [Tooltip("Seconds before landing that a jump press is queued and fired automatically.")]
    [SerializeField][Range(0f, 0.3f)] private float jumpBufferTime = 0.15f;

    // ── Ground Detection ──────────────────────────────────────────────────────

    [Header("Ground Detection")]
    [Tooltip("Extra distance below the capsule bottom the SphereCast checks for ground.")]
    [SerializeField] private float groundCheckDistance = 0.08f;

    [SerializeField] private LayerMask groundMask = ~0;

    // ── Private State ─────────────────────────────────────────────────────────

    private CharacterController cc;
    private PlayerInput pi;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private Vector3 velocity;       // World-space velocity in m/s (the single source of truth)
    private float pitch;            // Current camera up/down angle
    private bool isGrounded;
    private float coyoteTimer;      // Counts down after leaving ground
    private float jumpBufferTimer;  // Counts down after pressing jump
    private float jumpGraceTimer;   // Suppresses ground detection immediately after jumping

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();

        moveAction = pi.actions["Move"];
        lookAction = pi.actions["Look"];
        jumpAction = pi.actions["Jump"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera) playerCamera.enabled = true;
    }

    private void Update()
    {
        HandleLook();
        GroundCheck();
        TickJumpBuffer();
        HandleMovement();

        // Single Move() call per frame — everything goes through velocity
        CollisionFlags flags = cc.Move(velocity * Time.deltaTime);

        // Kill upward velocity on ceiling hits so the player doesn't "float"
        if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0f)
            velocity.y = 0f;
    }

    // ── Look ──────────────────────────────────────────────────────────────────

    private void HandleLook()
    {
        Vector2 look = lookAction.ReadValue<Vector2>() * lookSensitivity;

        // Yaw: rotate the whole body left/right
        transform.Rotate(0f, look.x, 0f);

        // Pitch: tilt only the camera pivot up/down
        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    // ── Ground Detection ──────────────────────────────────────────────────────

    private void GroundCheck()
    {
        // Don't detect ground immediately after jumping — the player hasn't
        // cleared the ground yet and would be snapped back down instantly.
        if (jumpGraceTimer > 0f)
        {
            jumpGraceTimer -= Time.deltaTime;
            isGrounded = false;
            coyoteTimer = 0f;
            return;
        }

        // Cast from just above the bottom sphere of the capsule so we never
        // start inside geometry, then check a short distance below.
        float castRadius = cc.radius * 0.9f;
        float halfHeight = cc.height * 0.5f;
        Vector3 bottom = transform.position + cc.center + Vector3.down * (halfHeight - cc.radius);
        Vector3 origin = bottom + Vector3.up * 0.1f;
        float castDist = 0.1f + groundCheckDistance;

        bool hit = Physics.SphereCast(
            origin, castRadius, Vector3.down, out _,
            castDist, groundMask, QueryTriggerInteraction.Ignore);

        if (hit)
        {
            isGrounded = true;
            coyoteTimer = coyoteTime;
        }
        else
        {
            isGrounded = false;
            coyoteTimer -= Time.deltaTime;
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

    /// <summary>
    /// Returns true if the player has valid jump input AND is within the
    /// coyote-time window of having been grounded.
    /// </summary>
    private bool CanJump()
    {
        bool hasJumpInput = jumpBufferTimer > 0f ||
                           (allowBunnyHop && jumpAction.IsPressed());
        bool nearGround = coyoteTimer > 0f;
        return hasJumpInput && nearGround;
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 wishDir = transform.right * input.x + transform.forward * input.y;
        if (wishDir.sqrMagnitude > 1f) wishDir.Normalize();

        if (isGrounded)
        {
            if (CanJump())
            {
                // Skip friction this frame so horizontal speed carries into the jump.
                // This is the core mechanic that makes bunny hopping possible.
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                coyoteTimer = 0f;
                jumpBufferTimer = 0f;
                jumpGraceTimer = 0.15f; // Ignore ground for this long so we clear it cleanly

                // Treat this frame as airborne immediately
                AirAccelerate(wishDir);
                ApplyGravity();
            }
            else
            {
                ApplyFriction();
                GroundAccelerate(wishDir);

                // Small downward constant keeps the player seated on slopes and
                // prevents micro-bouncing. Not a full gravity tick.
                velocity.y = -2f;
            }
        }
        else
        {
            ApplyGravity();
            AirAccelerate(wishDir);
        }
    }

    // ── Physics ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Bleeds off horizontal speed. Only applied when grounded and not jumping,
    /// so momentum is preserved through jumps.
    /// </summary>
    private void ApplyFriction()
    {
        float speed = HorizontalSpeed();
        if (speed < 0.001f) return;

        // "control" ensures friction fully stops the player rather than
        // approaching zero asymptotically (matches Source engine behaviour).
        float control = Mathf.Max(speed, stopSpeed);
        float newSpeed = Mathf.Max(speed - control * friction * Time.deltaTime, 0f);

        float scale = newSpeed / speed;
        velocity.x *= scale;
        velocity.z *= scale;
    }

    private void GroundAccelerate(Vector3 wishDir) =>
        Accelerate(wishDir, maxGroundSpeed, groundAcceleration);

    private void AirAccelerate(Vector3 wishDir) =>
        Accelerate(wishDir, maxAirSpeed, airAcceleration);

    /// <summary>
    /// The Quake/Source Accelerate() function.
    ///
    /// Projects the current horizontal velocity onto wishDir to find how fast
    /// we're already moving in that direction. We only add speed up to the
    /// wishSpeed cap, which is what keeps this feel controlled while still
    /// allowing meaningful directional influence (air strafing).
    /// </summary>
    private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
    {
        // How fast we're already moving in the desired direction
        float currentSpeed = Vector3.Dot(new Vector3(velocity.x, 0f, velocity.z), wishDir);

        // How much headroom we have before hitting the wish-speed cap
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0f) return;

        // Clamp so we never overshoot the cap
        float accelSpeed = Mathf.Min(accel * wishSpeed * Time.deltaTime, addSpeed);

        velocity.x += accelSpeed * wishDir.x;
        velocity.z += accelSpeed * wishDir.z;
    }

    private void ApplyGravity()
    {
        velocity.y += Physics.gravity.y * Time.deltaTime;
    }

    private float HorizontalSpeed() =>
        new Vector3(velocity.x, 0f, velocity.z).magnitude;
}