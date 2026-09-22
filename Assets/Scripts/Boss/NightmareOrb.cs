using UnityEngine;

/// <summary>
/// Projectile thrown by Sir Nightmare. There's no health in this game, so a hit is a
/// knockback plus a short lockout of chosen abilities (grapple/launch by default) — the
/// punishment is losing your line and your tools for a moment, not losing progress.
///
/// Spawned and aimed by SirNightmareFight via Launch(). Needs a SphereCollider (trigger) and a
/// kinematic Rigidbody; both are configured automatically.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class NightmareOrb : MonoBehaviour
{
    [Header("Hit Effect")]
    [SerializeField] private float knockbackSpeed = 9f;

    [Tooltip("Upward pop on hit. Keep the airtime this gives (~2 x upward / gravity) longer than Suppress Duration "
           + "so a hit costs the player their tools for a moment but doesn't guarantee a fall into the void.")]
    [SerializeField] private float knockbackUpward = 6f;

    [SerializeField] private PlayerAbility suppressedAbilities = PlayerAbility.Grapple | PlayerAbility.Launch;
    [SerializeField] private float suppressDuration = 0.75f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 8f;

    [Tooltip("Layers that make the orb burst on contact (platforms, arena walls). Leave at Nothing to let orbs pass through scenery.")]
    [SerializeField] private LayerMask burstOnLayers = 0;

    [SerializeField] private GameObject burstVfxPrefab;

    private Vector3 direction = Vector3.forward;
    private float speed;
    private Collider homingTarget;
    private float turnRateRad;
    private float homingTimeLeft;
    private float age;
    private bool spent;

    private void Reset() => Configure();
    private void Awake() => Configure();

    private void Configure()
    {
        var sphere = GetComponent<SphereCollider>();
        sphere.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    /// <summary>Fires the orb. Pass a homingTarget to make it curve toward it (turnRate in degrees/sec) for homingDuration seconds.</summary>
    public void Launch(Vector3 dir, float orbSpeed, Collider target = null, float turnRateDegrees = 0f, float homingDuration = 0f)
    {
        direction = dir.normalized;
        speed = orbSpeed;
        homingTarget = target;
        turnRateRad = turnRateDegrees * Mathf.Deg2Rad;
        homingTimeLeft = homingDuration;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Update()
    {
        if (homingTarget != null && homingTimeLeft > 0f)
        {
            Vector3 toTarget = homingTarget.bounds.center - transform.position;
            direction = Vector3.RotateTowards(direction, toTarget.normalized, turnRateRad * Time.deltaTime, 0f);
            homingTimeLeft -= Time.deltaTime;
        }

        transform.position += direction * (speed * Time.deltaTime);

        age += Time.deltaTime;
        if (age >= lifetime) Burst();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spent) return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            // Shove along the orb's travel direction so the hit reads as "that orb pushed me"
            player.ApplyKnockback(direction, knockbackSpeed, knockbackUpward);
            player.SuppressAbilities(suppressedAbilities, suppressDuration);
            Burst();
            return;
        }

        if (!other.isTrigger && (burstOnLayers.value & (1 << other.gameObject.layer)) != 0)
            Burst();
    }

    /// <summary>Destroys the orb with its burst effect — used by the boss to clear the field at phase changes.</summary>
    public void Dismiss() => Burst();

    private void Burst()
    {
        if (spent) return;
        spent = true;

        if (burstVfxPrefab != null)
            Instantiate(burstVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
