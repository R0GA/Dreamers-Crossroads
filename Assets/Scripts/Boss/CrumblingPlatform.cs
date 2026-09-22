using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A platform Sir Nightmare can attack. Strike() telegraphs (shake + optional warning effect),
/// then the platform gives way: colliders switch off instantly so the player simply drops, the
/// mesh falls into the void, and after a delay the platform rises back and turns solid again.
///
/// Setup: put this on the platform ROOT together with its colliders. Put the mesh(es) under a
/// child object and assign it as Visual Root. Only the child shakes and falls — the colliders
/// never move — so the CharacterController never gets jittered while the player stands on it.
/// </summary>
public class CrumblingPlatform : MonoBehaviour
{
    public enum State { Solid, Warning, Crumbled, Restoring }

    [Header("References")]
    [Tooltip("Direct child holding the mesh(es). This is what shakes and falls. Keep colliders OUT of it.")]
    [SerializeField] private Transform visualRoot;

    [Tooltip("Colliders switched off while crumbled. If empty, every collider outside Visual Root is used.")]
    [SerializeField] private Collider[] solidColliders;

    [Tooltip("Optional: shown during the warning (ink pooling, red glow, cracks). Keep it outside Visual Root.")]
    [SerializeField] private GameObject warningEffect;

    [Tooltip("Optional: played the instant the platform gives way. Keep it outside Visual Root.")]
    [SerializeField] private ParticleSystem crumbleParticles;

    [Header("Rules")]
    [Tooltip("Boss attacks never target this platform. Use it for the arena entry platform / checkpoint so the player always has somewhere safe.")]
    [SerializeField] private bool immune;

    [Header("Feel")]
    [SerializeField] private float shakeAmplitude = 0.07f;
    [SerializeField] private float fallDistance = 15f;
    [SerializeField] private float fallTime = 0.9f;
    [Tooltip("How far below its home position the platform starts when it rises back.")]
    [SerializeField] private float restoreRise = 3f;
    [SerializeField] private float restoreTime = 0.7f;

    [Header("Events (hook SFX / VFX here)")]
    public UnityEvent onWarn;
    public UnityEvent onCrumble;
    public UnityEvent onRestore;

    public State Current { get; private set; } = State.Solid;
    public bool IsSolid => Current == State.Solid;
    public bool Immune => immune;
    public bool IsPermanentlyCollapsed => permanent;

    /// <summary>True if a boss attack is allowed to target this platform right now.</summary>
    public bool CanBeStruck => !immune && !permanent && Current == State.Solid;

    private Vector3 visualHome;
    private Vector3 visualScale;
    private Renderer[] visualRenderers;
    private Coroutine routine;
    private bool permanent;
    private PlayerController player;

    private void Reset()
    {
        if (transform.childCount > 0) visualRoot = transform.GetChild(0);
    }

    private void Awake()
    {
        if (visualRoot == null)
        {
            Debug.LogError($"{name}: CrumblingPlatform needs a Visual Root child assigned.", this);
            enabled = false;
            return;
        }

        visualHome = visualRoot.localPosition;
        visualScale = visualRoot.localScale;

        // Particle renderers are excluded so a stray effect under the visual doesn't get hidden
        visualRenderers = Array.FindAll(
            visualRoot.GetComponentsInChildren<Renderer>(true),
            r => !(r is ParticleSystemRenderer));

        if (solidColliders == null || solidColliders.Length == 0)
        {
            solidColliders = Array.FindAll(
                GetComponentsInChildren<Collider>(),
                c => !c.transform.IsChildOf(visualRoot));
        }

        if (warningEffect != null) warningEffect.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Telegraphs for warnTime, crumbles, stays gone for downTime, then returns. False if the platform can't be struck right now.</summary>
    public bool Strike(float warnTime, float downTime)
    {
        if (!enabled || !CanBeStruck) return false;
        routine = StartCoroutine(CrumbleRoutine(warnTime, downTime));
        return true;
    }

    /// <summary>Crumbles and never comes back — use at phase changes to shrink the arena as the level collapses.</summary>
    public void Collapse(float warnTime = 0.8f)
    {
        if (!enabled || permanent) return;
        permanent = true;

        // Warning / Crumbled: the routine already in flight sees 'permanent' and stops after the fall.
        if (Current == State.Solid || Current == State.Restoring)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(CrumbleRoutine(warnTime, 0f));
        }
    }

    /// <summary>Instantly puts the platform back to a normal solid state (fight reset, debugging).</summary>
    public void RestoreImmediately()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        permanent = false;

        ResetVisual();
        SetVisible(true);
        SetSolid(true);
        if (warningEffect != null) warningEffect.SetActive(false);
        Current = State.Solid;
    }

    // ── Routine ───────────────────────────────────────────────────────────────

    private IEnumerator CrumbleRoutine(float warnTime, float downTime)
    {
        // ── Warning ──
        ResetVisual();
        SetVisible(true);
        Current = State.Warning;
        if (warningEffect != null) warningEffect.SetActive(true);
        onWarn.Invoke();

        for (float t = 0f; t < warnTime; t += Time.deltaTime)
        {
            float intensity = Mathf.Lerp(0.35f, 1f, t / warnTime); // shake builds up
            visualRoot.localPosition = visualHome + UnityEngine.Random.insideUnitSphere * (shakeAmplitude * intensity);
            yield return null;
        }
        visualRoot.localPosition = visualHome;

        // ── Give way ──
        Current = State.Crumbled;
        if (warningEffect != null) warningEffect.SetActive(false);
        SetSolid(false);
        ReleasePlayerGrapple();
        if (crumbleParticles != null) crumbleParticles.Play();
        onCrumble.Invoke();

        for (float t = 0f; t < fallTime; t += Time.deltaTime)
        {
            float k = t / fallTime;
            visualRoot.localPosition = visualHome + transform.InverseTransformVector(Vector3.down * (fallDistance * k * k));
            visualRoot.localScale = Vector3.Lerp(visualScale, visualScale * 0.5f, k);
            yield return null;
        }
        SetVisible(false);

        if (permanent) { routine = null; yield break; }

        yield return new WaitForSeconds(downTime);

        if (permanent) { routine = null; yield break; } // Collapse() was called while we were down

        // ── Restore ──
        Current = State.Restoring;
        SetVisible(true);
        onRestore.Invoke();

        Vector3 start = visualHome + transform.InverseTransformVector(Vector3.down * restoreRise);
        for (float t = 0f; t < restoreTime; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / restoreTime, 3f); // ease-out
            visualRoot.localPosition = Vector3.Lerp(start, visualHome, k);
            visualRoot.localScale = Vector3.Lerp(visualScale * 0.5f, visualScale, k);
            yield return null;
        }

        // Solid only once fully back, so it never pops into a player who's hovering on a grapple
        ResetVisual();
        SetSolid(true);
        Current = State.Solid;
        routine = null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetSolid(bool solid)
    {
        foreach (var c in solidColliders)
            if (c != null) c.enabled = solid;
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in visualRenderers)
            if (r != null) r.enabled = visible;
    }

    private void ResetVisual()
    {
        visualRoot.localPosition = visualHome;
        visualRoot.localScale = visualScale;
    }

    // If the player was swinging off this platform, drop the rope so it doesn't stay pinned to nothing
    private void ReleasePlayerGrapple()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (player != null) player.ReleaseGrappleFrom(transform);
    }
}
