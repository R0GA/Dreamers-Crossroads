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
///
/// DIALOGUE:
///  - Intro: played by a DialogueTriggerZone (assign the SAME DialogueSequence asset to the
///    zone and to 'Intro Dialogue' here). BeginPuzzle() is ignored until it ends; with
///    autoStart on, the puzzle starts the moment it ends. This is the only thing that locks.
///  - Fail: plays on a wrong note once the player has failed N times in total (default 1),
///    then only that once unless 'Repeat Fail Dialogue' is on.
///  - Progress: plays once, after the player has completed N rounds IN A ROW. A mistake
///    resets that streak, so it only plays when they're genuinely close to solving it.
///  - Solved: plays on success. If the progress line is still running, it's cut off so the
///    success line is never dropped.
///  - A line is never cut off by a hint/fail line: if another sequence is already running
///    when one is due, it doesn't play that time and is re-checked on the next opportunity.
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

    [Header("Dialogue")]
    [Tooltip("Must be the same asset your DialogueTriggerZone plays. The puzzle can't be started " +
             "until it finishes. Leave empty to have the puzzle usable from the start.")]
    [SerializeField] private DialogueSequence introDialogue;

    [Tooltip("Plays when the player presses a wrong note, once they've reached the failure count below.")]
    [SerializeField] private DialogueSequence failDialogue;
    [Tooltip("Total mistakes (across the whole puzzle) before the fail dialogue can play. " +
             "1 = on the very first mistake, 3 = on the third mistake.")]
    [Min(1)]
    [SerializeField] private int failsBeforeFailDialogue = 1;
    [Tooltip("If off, the fail dialogue plays only once. If on, it plays on every mistake from " +
             "the failure count onward (still skipped if another sequence is already playing).")]
    [SerializeField] private bool repeatFailDialogue = false;

    [Tooltip("Plays once, after the player has completed this many rounds IN A ROW without a " +
             "mistake. Any mistake resets that count to zero.")]
    [SerializeField] private DialogueSequence progressDialogue;
    [Min(1)]
    [SerializeField] private int roundsBeforeProgressDialogue = 3;

    [Tooltip("Plays when the puzzle is solved.")]
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

    /// <summary>
    /// True once the intro dialogue has finished and the puzzle hasn't been solved yet.
    /// InteractableTrigger uses this to hide its prompt and ignore presses until then.
    /// </summary>
    public bool IsReadyToBegin => introFinished && state != State.Solved;

    private enum State { Idle, Playing, Solved }
    private State state = State.Idle;

    private int currentRound = 1;  // how many notes of songSequence are "live" this round
    private int expectedIndex;     // how many correct presses the player has made so far this round
    private Coroutine runningRoutine;

    private bool introFinished;
    private bool failPlayed;
    private bool progressPlayed;
    private int failCount;            // total mistakes, never reset
    private int correctRoundsStreak;  // rounds completed since the last mistake

    private void Start()
    {
        // Subscribe in Start so DialogueManager.Awake() has definitely run.
        var manager = DialogueManager.Instance;

        if (manager == null)
        {
            introFinished = true; // no dialogue system in the scene: don't lock the player out
        }
        else
        {
            // No intro assigned, or it already played earlier this session (e.g. scene reloaded).
            introFinished = introDialogue == null || manager.HasPlayed(introDialogue);
            manager.DialogueEnded += HandleDialogueEnded;
        }

        if (autoStart && introFinished) BeginPuzzle();
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.DialogueEnded -= HandleDialogueEnded;
    }

    private void HandleDialogueEnded(DialogueSequence sequence)
    {
        if (sequence != introDialogue || introFinished) return;

        introFinished = true;
        if (autoStart) BeginPuzzle();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Starts the puzzle from round 1. Safe to call from a lever's Interact(), a trigger volume, a cutscene, etc.</summary>
    public void BeginPuzzle()
    {
        if (state == State.Solved) return;
        if (!introFinished) return; // intro dialogue still hasn't finished

        if (randomizeSequence && (songSequence == null || songSequence.Length == 0))
            GenerateRandomSequence();

        if (runningRoutine != null) StopCoroutine(runningRoutine);

        currentRound = 1;
        correctRoundsStreak = 0;
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

            failCount++;
            correctRoundsStreak = 0;
            HandleFailDialogue();

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

            // The progress line may still be running. DialogueManager refuses new sequences
            // mid-playback, so cut it off to make sure the success line always plays.
            var manager = DialogueManager.Instance;
            if (manager != null && manager.IsPlaying && solvedDialogue != null)
                manager.Stop();

            TryPlay(solvedDialogue);
            onPuzzleSolved?.Invoke();
            return;
        }

        onRoundSucceeded?.Invoke();

        correctRoundsStreak++;
        HandleProgressDialogue(correctRoundsStreak);
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

    // ── Dialogue ──────────────────────────────────────────────────────────────

    private enum PlayResult
    {
        Started, // sequence is now playing
        Busy,    // another sequence is playing; try again later
        Skipped  // nothing to play (none assigned, empty, or a playOnce that already played)
    }

    private static PlayResult TryPlay(DialogueSequence sequence)
    {
        var manager = DialogueManager.Instance;

        if (sequence == null || manager == null
            || sequence.lines == null || sequence.lines.Length == 0
            || manager.HasPlayed(sequence))
            return PlayResult.Skipped;

        return manager.PlayDialogue(sequence) ? PlayResult.Started : PlayResult.Busy;
    }

    private void HandleFailDialogue()
    {
        if (failCount < failsBeforeFailDialogue) return;
        if (failPlayed && !repeatFailDialogue) return;

        // Only counts as played if it started (or there's nothing to play). If something else
        // was running, it's left pending and tried again on the next mistake.
        if (TryPlay(failDialogue) != PlayResult.Busy)
            failPlayed = true;
    }

    private void HandleProgressDialogue(int roundsInARow)
    {
        if (progressPlayed || roundsInARow < roundsBeforeProgressDialogue) return;

        // Same rule: if busy, try again after the next completed round.
        if (TryPlay(progressDialogue) != PlayResult.Busy)
            progressPlayed = true;
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

        if (!randomizeSequence && progressDialogue != null && songSequence.Length > 0
            && roundsBeforeProgressDialogue >= songSequence.Length)
        {
            Debug.LogWarning($"[{name}] roundsBeforeProgressDialogue ({roundsBeforeProgressDialogue}) is not " +
                              $"less than the song length ({songSequence.Length}), so the progress dialogue " +
                              "will never play (the success dialogue takes over on the final round).", this);
        }

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