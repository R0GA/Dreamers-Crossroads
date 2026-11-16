using System.Collections;
using UnityEngine;

/// <summary>
/// A single pressable "note" in a music-sequence puzzle, drawn as a world-space billboarded
/// sprite. Implements IInteractable so the player can ring it directly with the normal
/// Interact action; SongSequencePuzzle also calls Play() directly (bypassing Interact) to
/// perform its own sequence playback.
///
/// Hierarchy tip: keep this script (and the collider) on the ROOT, and put the
/// SpriteRenderer + your billboard script on a CHILD. Assign that child to `visual`.
/// The bob moves the child's local position, so it stays correct no matter how the
/// billboard rotates the sprite to face the camera.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class NoteButton : MonoBehaviour, IInteractable
{
    [Tooltip("The puzzle this note belongs to. Every press is reported here; the puzzle " +
             "decides whether it was correct, too early, or should be ignored.")]
    [SerializeField] private SongSequencePuzzle puzzle;

    [Header("Sound")]
    [Tooltip("The tone this note rings. Give every NoteButton in a puzzle a distinct clip " +
             "(or share one clip and vary Pitch below) so each note is distinguishable by ear.")]
    [SerializeField] private AudioClip noteSound;

    [Tooltip("Playback pitch for noteSound. Handy if several notes share one sample.")]
    [SerializeField] private float pitch = 1f;

    [Header("Visual Feedback")]
    [Tooltip("The child object holding the sprite (and billboard script). This is what bobs. " +
             "Leave empty to use this object's own transform.")]
    [SerializeField] private Transform visual;

    [Tooltip("SpriteRenderer that flashes when this note plays. Leave empty to skip the color flash.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Tint the sprite flashes to when played. Fades back to the sprite's original color.")]
    [SerializeField] private Color litColor = Color.cyan;

    [Tooltip("Seconds the flash takes to fade back to the resting color.")]
    [SerializeField] private float flashDuration = 0.35f;

    [Tooltip("How far (in local units) the note rises at the peak of its bob.")]
    [SerializeField] private float bobHeight = 0.15f;

    [Tooltip("Seconds for one full up-and-down bob.")]
    [SerializeField] private float bobDuration = 0.35f;

    private AudioSource audioSource;
    private Color baseColor = Color.white;
    private Vector3 baseLocalPosition;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (visual == null) visual = transform;
        baseLocalPosition = visual.localPosition;

        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    private void OnDisable()
    {
        // Don't leave a note stuck mid-bob or mid-flash if it's disabled during playback.
        ResetVisuals();
        feedbackRoutine = null;
    }

    // ── IInteractable ─────────────────────────────────────────────────────────

    public void Interact(GameObject interactor)
    {
        if (puzzle == null)
        {
            Play(); // No puzzle wired up — just ring the note. Handy for testing in isolation.
            return;
        }

        // Ignore presses while the puzzle is playing the sequence back or between rounds —
        // matches how a real Simon-style toy ignores input outside its "your turn" window.
        if (!puzzle.IsAcceptingInput) return;

        Play();
        puzzle.NotifyNotePressed(this);
    }

    public string GetInteractionPrompt() =>
        (puzzle == null || puzzle.IsAcceptingInput) ? "Play Note" : null;

    // ── Playback ──────────────────────────────────────────────────────────────

    /// <summary>Rings this note's tone, bobs it up, and flashes it. Called for both player presses and the puzzle's own sequence playback.</summary>
    public void Play()
    {
        if (noteSound != null)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(noteSound);
        }

        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        ResetVisuals();
        feedbackRoutine = StartCoroutine(FeedbackRoutine());
    }

    private IEnumerator FeedbackRoutine()
    {
        float duration = Mathf.Max(flashDuration, bobDuration);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            // Bob: smooth rise and fall using half a sine wave (0 -> 1 -> 0).
            if (bobDuration > 0f)
            {
                float bobT = Mathf.Clamp01(t / bobDuration);
                float offset = Mathf.Sin(bobT * Mathf.PI) * bobHeight;
                visual.localPosition = baseLocalPosition + Vector3.up * offset;
            }

            // Flash: start fully lit, fade back to the resting color.
            if (spriteRenderer != null && flashDuration > 0f)
            {
                float flashT = Mathf.Clamp01(t / flashDuration);
                spriteRenderer.color = Color.Lerp(litColor, baseColor, flashT);
            }

            yield return null;
        }

        ResetVisuals();
        feedbackRoutine = null;
    }

    private void ResetVisuals()
    {
        if (visual != null) visual.localPosition = baseLocalPosition;
        if (spriteRenderer != null) spriteRenderer.color = baseColor;
    }
}