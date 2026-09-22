using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A stolen dream floating in the arena. Swing through it to pop it and release the dream —
/// each pop makes Sir Nightmare drowsier (tracked by SirNightmareFight).
///
/// The bubble uses a trigger collider, and your grapple raycast ignores triggers, so the rope
/// passes straight through bubbles instead of latching onto them.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class DreamBubble : MonoBehaviour
{
    /// <summary>Raised once when the player pops the bubble.</summary>
    public event Action<DreamBubble> Popped;

    /// <summary>Raised once if the bubble drifts away unpopped.</summary>
    public event Action<DreamBubble> Expired;

    [Header("Popping")]
    [Tooltip("If on, the bubble only pops when the player is grappling, or was within Grapple Grace Seconds of releasing — "
           + "so swinging through it counts, but walking or plain jumping into it doesn't.")]
    [SerializeField] private bool requireGrapple = true;

    [Tooltip("Lets 'swing, let go, fly through the bubble' count as a swing.")]
    [SerializeField] private float grappleGraceSeconds = 0.75f;

    [Header("Life")]
    [Tooltip("Seconds before the bubble drifts away unpopped.")]
    [SerializeField] private float lifetime = 14f;
    [Tooltip("The bubble pulses during this final stretch to warn it's about to go.")]
    [SerializeField] private float warnTime = 3f;

    [Header("Motion")]
    [SerializeField] private float bobAmplitude = 0.6f;
    [SerializeField] private float bobSpeed = 1.2f;

    [Header("Feedback")]
    [SerializeField] private GameObject popVfxPrefab;
    public UnityEvent onPop;

    private Vector3 basePosition;
    private Vector3 baseScale;
    private float phaseOffset;
    private float age;
    private bool finished;

    private void Reset() => Configure();
    private void Awake() => Configure();

    private void Configure()
    {
        GetComponent<SphereCollider>().isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Start()
    {
        basePosition = transform.position;
        baseScale = transform.localScale;
        phaseOffset = UnityEngine.Random.value * 10f;
    }

    private void Update()
    {
        age += Time.deltaTime;

        float bob = Mathf.Sin(age * bobSpeed + phaseOffset) * bobAmplitude;
        transform.position = basePosition + Vector3.up * bob;

        float grow = Mathf.SmoothStep(0f, 1f, age / 0.4f); // pops into existence instead of appearing instantly
        float remaining = lifetime - age;
        float pulse = remaining < warnTime ? 1f + 0.12f * Mathf.Sin(age * 16f) : 1f;
        transform.localScale = baseScale * (grow * pulse);

        if (remaining <= 0f) Expire();
    }

    // Enter AND Stay: the player may already be overlapping when they start a swing
    private void OnTriggerEnter(Collider other) => TryPop(other);
    private void OnTriggerStay(Collider other) => TryPop(other);

    private void TryPop(Collider other)
    {
        if (finished) return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;
        if (requireGrapple && player.SecondsSinceGrapple > grappleGraceSeconds) return;

        finished = true;
        Popped?.Invoke(this);
        onPop.Invoke();

        if (popVfxPrefab != null)
            Instantiate(popVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void Expire()
    {
        if (finished) return;
        finished = true;
        Expired?.Invoke(this);
        Destroy(gameObject);
    }
}
