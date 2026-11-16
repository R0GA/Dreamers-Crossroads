using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PortalController : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("The name of the scene to load when the player enters the portal.")]
    public string nextSceneName;

    [Tooltip("How long it takes for the portal to grow to full size.")]
    public float growDuration = 1.0f;

    [Tooltip("Controls the scaling animation. Try an Ease In Out curve.")]
    public AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Dialogue Gate (optional)")]
    [Tooltip("If ticked, the portal still reveals and grows as normal, but can't be entered "
           + "until Required Dialogue has finished. Leave unticked for the original behaviour: "
           + "enterable as soon as it has finished growing.")]
    public bool waitForDialogue = false;

    [Tooltip("The dialogue that must finish before the portal can be entered (e.g. the final "
           + "Pixie conversation). Should have playOnce ticked, so the portal can tell it "
           + "finished naturally rather than being cut off.")]
    public DialogueSequence requiredDialogue;

    [Header("Menu Return (optional)")]
    [Tooltip("Tick this on a portal that sends the player back to the main menu (e.g. after the final "
           + "level). Before loading, it resets the persistent DialogueManager, unfreezes time/audio and "
           + "frees the cursor, so the next run starts clean. Set Next Scene Name to your menu scene. "
           + "Leave unticked for ordinary level-to-level portals.")]
    public bool returnToMainMenu = false;

    [Header("Events")]
    [Tooltip("Fires once at the moment the portal becomes enterable — after it has grown and, "
           + "if waiting, after the dialogue has finished. Hook a glow, sound or particles here.")]
    public UnityEvent onPortalReady;

    private Vector3 originalScale;
    private Collider portalCollider;
    private bool isRevealed = false;
    private bool growFinished = false;
    private bool dialogueSatisfied = true;
    private bool isUsable = false;
    private bool isLoading = false;

    void Start()
    {
        // 1. Save the custom scale you set in the editor
        originalScale = transform.localScale;

        // 2. Set scale to zero so it is completely hidden
        transform.localScale = Vector3.zero;

        // 3. Disable the collider so the player can't trigger an invisible portal
        portalCollider = GetComponent<Collider>();
        if (portalCollider != null)
        {
            portalCollider.enabled = false;
            portalCollider.isTrigger = true; // Ensure it's treated as a trigger
        }

        // 4. Optionally start listening for the dialogue the portal is waiting on
        dialogueSatisfied = !waitForDialogue;
        if (waitForDialogue) SetUpDialogueWait();
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.DialogueEnded -= HandleDialogueEnded;
    }

    // Call this public method from your puzzle-solving script
    public void RevealPortal()
    {
        if (!isRevealed)
        {
            isRevealed = true;
            StartCoroutine(GrowPortalRoutine());
        }
    }

    private IEnumerator GrowPortalRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < growDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate how far along the animation is (0.0 to 1.0)
            float t = elapsedTime / growDuration;

            // Evaluate the animation curve for smoother scaling
            float curveValue = growCurve.Evaluate(t);

            // Use LerpUnclamped so the curve can "overshoot" for a bounce effect if desired
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, originalScale, curveValue);

            yield return null; // Wait for the next frame
        }

        // Snap exactly to the final scale to prevent any floating point inaccuracies
        transform.localScale = originalScale;

        // Fully grown. Becomes enterable now, or as soon as the dialogue finishes if waiting.
        growFinished = true;
        TryBecomeUsable();
    }

    // ── Dialogue Gate ────────────────────────────────────────────────────────

    private void SetUpDialogueWait()
    {
        // Misconfiguration should never trap the player in the level: if there's nothing
        // valid to wait for, behave like an ordinary portal.
        if (requiredDialogue == null)
        {
            Debug.LogWarning("PortalController: waitForDialogue is on but no requiredDialogue is assigned — not waiting.", this);
            dialogueSatisfied = true;
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("PortalController: waitForDialogue is on but there's no DialogueManager — not waiting.", this);
            dialogueSatisfied = true;
            return;
        }

        // Already completed earlier this session (playOnce sequences are remembered).
        if (DialogueManager.Instance.HasPlayed(requiredDialogue))
        {
            dialogueSatisfied = true;
            return;
        }

        DialogueManager.Instance.DialogueEnded += HandleDialogueEnded;
    }

    private void HandleDialogueEnded(DialogueSequence ended)
    {
        if (dialogueSatisfied || ended != requiredDialogue) return;

        // playOnce sequences are only recorded as played when they run to the end, so this
        // rejects one that was cut short by DialogueManager.Stop().
        if (ended.playOnce && !DialogueManager.Instance.HasPlayed(ended)) return;

        dialogueSatisfied = true;
        DialogueManager.Instance.DialogueEnded -= HandleDialogueEnded;
        TryBecomeUsable();
    }

    private void TryBecomeUsable()
    {
        if (isUsable || !growFinished || !dialogueSatisfied) return;
        isUsable = true;

        // Enable the trigger now that the portal is visible (and unlocked)
        if (portalCollider != null)
        {
            portalCollider.enabled = true;
        }

        onPortalReady.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the portal is the player
        if (other.CompareTag("Player") && isRevealed && isUsable && !isLoading)
        {
            isLoading = true; // the player may have several colliders; only load once

            if (returnToMainMenu) PrepareForMainMenu();
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // ── Main Menu Cleanup ────────────────────────────────────────────────────

    /// <summary>
    /// Mirrors PauseMenu's "Back to Menu" so finishing the game and quitting to the menu leave
    /// things in the same clean state.
    /// </summary>
    private void PrepareForMainMenu()
    {
        // DialogueManager persists across scenes: wipe any half-played sequence and the
        // "already played" history so a new run's dialogue isn't broken or skipped.
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ResetAll();

        Time.timeScale = 1f;
        AudioListener.pause = false;

        // The menu needs a usable cursor (PlayerController locks it in game scenes)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}