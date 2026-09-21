using System.Collections;
using UnityEngine;

/// <summary>
/// A single pressable "note" in a music-sequence puzzle — a bell, a glowing floor pad, a
/// chime, a puzzle-ring segment, whatever fits the room. Implements IInteractable so the
/// player can ring it directly with the normal Interact action; SongSequencePuzzle also
/// calls Play() directly (bypassing Interact) to perform its own sequence playback.
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
    [Tooltip("Renderer that flashes when this note plays. Leave empty to skip visual feedback.")]
    [SerializeField] private Renderer noteRenderer;

    [Tooltip("Emission color the note flashes to when played.")]
    [SerializeField] private Color litColor = Color.cyan;

    [Tooltip("Seconds the flash takes to fade back down to its resting color.")]
    [SerializeField] private float flashDuration = 0.35f;

    private AudioSource audioSource;
    private Color baseEmission;
    private Coroutine flashRoutine;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (noteRenderer != null)
        {
            // Instance the material so flashing this note doesn't flash every other note
            // that happens to share the same material asset.
            noteRenderer.material = new Material(noteRenderer.material);

            if (noteRenderer.material.HasProperty(EmissionColorId))
            {
                noteRenderer.material.EnableKeyword("_EMISSION");
                baseEmission = noteRenderer.material.GetColor(EmissionColorId);
            }
        }
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

    /// <summary>Rings this note's tone and flash. Called for both player presses and the puzzle's own sequence playback.</summary>
    public void Play()
    {
        if (noteSound != null)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(noteSound);
        }

        if (noteRenderer != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        noteRenderer.material.SetColor(EmissionColorId, litColor);

        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            noteRenderer.material.SetColor(EmissionColorId, Color.Lerp(litColor, baseEmission, t / flashDuration));
            yield return null;
        }

        noteRenderer.material.SetColor(EmissionColorId, baseEmission);
        flashRoutine = null;
    }
}
