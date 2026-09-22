using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Classic "Simon Says" style music puzzle. Plays a melody one note at a time, then waits
/// for the player to ring the same notes back on the matching NoteButtons. Get a round
/// right and the melody grows by one more note; get it wrong and the round is repeated
/// (or the whole thing restarts — your choice); play the full song back correctly and
/// onPuzzleSolved fires.
///
/// Setup:
///  1. Place one NoteButton per distinct tone in the scene and point each one's `puzzle`
///     field at this component.
///  2. Drag those same NoteButtons into noteButtons below, then set songSequence to a
///     list of indices into that array — this is your melody. E.g. buttons
///     [Low, Mid, High] with songSequence [0, 0, 1, 2, 1] plays Low, Low, Mid, High, Mid.
///     (Or tick randomizeSequence to have one generated for you instead.)
///  3. Either tick autoStart, or have a lever/trigger elsewhere call BeginPuzzle() — any
///     IInteractable can do this from its own Interact(), no changes to player code needed.
///     (See InteractableTrigger for a ready-made generic lever.)
///  4. Wire onPuzzleSolved (and onMistake / onRoundSucceeded if you want extra feedback)
///     in the Inspector — open a door, unlock an ability, play a fanfare, etc.
/// </summary>
public class SongSequencePuzzle : MonoBehaviour
{
    [Header("Notes")]
    [Tooltip("Every playable note in this puzzle, in a stable order. songSequence indexes into this array.")]
    [SerializeField] private NoteButton[] noteButtons;

    [Tooltip("The melody, as indices into noteButtons. The puzzle reveals a growing prefix each round: " +
             "first songSequence[0], then [0..1], then [0..2], and so on until the whole array plays.")]
    [SerializeField] private int[] songSequence;

    [Header("Randomize (optional)")]
    [Tooltip("If set, songSequence above is ignored and a random melody is generated the first time BeginPuzzle() runs.")]
    [SerializeField] private bool randomizeSequence = false;

    [Tooltip("Length of the generated melody when randomizeSequence is on.")]
    [SerializeField] private int randomSequenceLength = 8;

    [Header("Timing")]
    [Tooltip("Seconds each note stays lit/ringing during playback.")]
    [SerializeField] private float noteDuration = 0.5f;

    [Tooltip("Silent gap between notes during playback.")]
    [SerializeField] private float noteGap = 0.25f;

    [Tooltip("Pause after the sequence finishes playing before the player's presses are accepted.")]
    [SerializeField] private float inputDelay = 0.3f;

    [Tooltip("Pause after a correct or incorrect round before the sequence plays again.")]
    [SerializeField] private float roundEndDelay = 1f;

    [Header("Behaviour")]
    [Tooltip("Play the first round automatically when the scene loads. Leave off if a lever/trigger should call BeginPuzzle() instead.")]
    [SerializeField] private bool autoStart = false;

    [Tooltip("On a wrong note: ON replays from round 1 (classic Simon Says); OFF just repeats the current round.")]
    [SerializeField] private bool resetProgressOnMistake = true;
    [SerializeField] private DialogueSequence solvedDialogue;

    [Header("Events")]
    public UnityEvent onRoundStarted;
    public UnityEvent onRoundSucceeded;
    public UnityEvent onMistake;
    public UnityEvent onPuzzleSolved;

    /// <summary>
    /// True only during the window between the sequence finishing playback and the next
    /// wrong/completing press. NoteButtons check this both to decide whether to forward a
    /// press at all, and to decide whether to show an interaction prompt.
    /// </summary>
    public bool IsAcceptingInput { get; private set; }

    private enum State { Idle, Playing, Solved }
    private State state = State.Idle;

    private int currentRound = 1;  // how many notes of songSequence are "live" this round
    private int expectedIndex;     // how many correct presses the player has made so far this round
    private Coroutine runningRoutine;

    private void Start()
    {
        if (autoStart) BeginPuzzle();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Starts the puzzle from round 1. Safe to call from a lever's Interact(), a trigger volume, a cutscene, etc.</summary>
    public void BeginPuzzle()
    {
        if (state == State.Solved) return;

        if (randomizeSequence && (songSequence == null || songSequence.Length == 0))
            GenerateRandomSequence();

        if (runningRoutine != null) StopCoroutine(runningRoutine);

        currentRound = 1;
        state = State.Playing;
        runningRoutine = StartCoroutine(PlayRound());
    }

    /// <summary>Called by a NoteButton when the player presses it. No-op outside the input window or once solved.</summary>
    public void NotifyNotePressed(NoteButton pressed)
    {
        if (state != State.Playing || !IsAcceptingInput) return;

        NoteButton expectedButton = noteButtons[songSequence[expectedIndex]];

        if (pressed != expectedButton)
        {
            IsAcceptingInput = false;
            onMistake?.Invoke();

            if (resetProgressOnMistake) currentRound = 1;
            runningRoutine = StartCoroutine(DelayThenPlayRound());
            return;
        }

        expectedIndex++;
        if (expectedIndex < currentRound) return; // Round not finished yet — keep listening

        IsAcceptingInput = false;

        if (currentRound >= songSequence.Length)
        {
            state = State.Solved;
            if (DialogueManager.Instance != null && solvedDialogue != null)
                DialogueManager.Instance.PlayDialogue(solvedDialogue);
            onPuzzleSolved?.Invoke();
            return;
        }

        onRoundSucceeded?.Invoke();
        currentRound++;
        runningRoutine = StartCoroutine(DelayThenPlayRound());
    }

    // ── Playback ──────────────────────────────────────────────────────────────

    private IEnumerator DelayThenPlayRound()
    {
        yield return new WaitForSeconds(roundEndDelay);
        runningRoutine = StartCoroutine(PlayRound());
    }

    private IEnumerator PlayRound()
    {
        expectedIndex = 0;
        IsAcceptingInput = false;
        onRoundStarted?.Invoke();

        for (int i = 0; i < currentRound; i++)
        {
            noteButtons[songSequence[i]].Play();
            yield return new WaitForSeconds(noteDuration);
            yield return new WaitForSeconds(noteGap);
        }

        yield return new WaitForSeconds(inputDelay);
        IsAcceptingInput = true;
    }

    private void GenerateRandomSequence()
    {
        songSequence = new int[randomSequenceLength];
        for (int i = 0; i < randomSequenceLength; i++)
            songSequence[i] = Random.Range(0, noteButtons.Length);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (noteButtons == null || noteButtons.Length == 0 || songSequence == null) return;

        foreach (int index in songSequence)
        {
            if (index < 0 || index >= noteButtons.Length)
            {
                Debug.LogWarning($"[{name}] songSequence contains index {index}, which is out " +
                                  $"of range for noteButtons (length {noteButtons.Length}).", this);
                break;
            }
        }
    }
#endif
}
