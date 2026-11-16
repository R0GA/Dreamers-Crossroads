using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Birthday cake candle puzzle.
/// Candles must be lit in a hidden order. Lighting a wrong candle resets the whole puzzle.
/// The solution never changes between attempts.
///
/// SOLUTION ORDER:
///  - Default: the order of the 'candles' array IS the solution (Element 0 must be lit first, etc.).
///    The player can't see this order, so arrange them in the inspector however you like.
///  - Optional: tick 'Use Random Solution' to shuffle the order once, using a fixed seed,
///    so it's different per playthrough but constant within it.
///
/// DIALOGUE:
///  - Intro: played by a DialogueTriggerZone (assign the SAME DialogueSequence asset to both the
///    zone and this puzzle's 'Intro Dialogue'). The puzzle can't be interacted with until that
///    sequence ends.
///  - First hint: plays after the player's Nth failed attempt (default 2).
///  - Second hint: plays after a few more failed attempts counted from when the first hint
///    FINISHED (default 2), so rapid retries can't stack it up early.
///  - Solved: plays on success. Solving on the first try skips straight to this; hints never
///    block completion.
///  - The puzzle is also locked while ANY dialogue is playing, so the Interact key used to
///    advance dialogue can't accidentally light a candle.
/// </summary>
public class CandlePuzzle : MonoBehaviour
{
    [Header("Candles")]
    [Tooltip("Drag every candle here. Array order = solution order (unless Use Random Solution is on).")]
    [SerializeField] private Candle[] candles;

    [Header("Solution")]
    [SerializeField] private bool useRandomSolution = false;
    [Tooltip("Same seed = same order. Set this from your save data / run seed if you want a per-playthrough order.")]
    [SerializeField] private int seed = 12345;

    [Header("Reset Timing")]
    [Tooltip("Pause after a wrong candle is lit, so the player sees their mistake.")]
    [SerializeField] private float wrongPauseSeconds = 0.75f;
    [Tooltip("Delay between each candle blowing out during a reset (0 = all at once).")]
    [SerializeField] private float extinguishStagger = 0.1f;
    [Tooltip("Pause after all candles are out before the player can try again.")]
    [SerializeField] private float afterResetPauseSeconds = 0.5f;

    [Header("Dialogue")]
    [Tooltip("Must be the same asset your DialogueTriggerZone plays. The puzzle stays locked until "
           + "this sequence finishes. Leave empty to have the puzzle usable from the start.")]
    [SerializeField] private DialogueSequence introDialogue;
    [Tooltip("Hint played after the player has failed this many times in total (1 = after the first fail).")]
    [SerializeField] private DialogueSequence firstFailDialogue;
    [Min(1)]
    [SerializeField] private int failsBeforeFirstHint = 2;
    [Tooltip("Second hint, played after the first hint has been given and a few more fails have happened.")]
    [SerializeField] private DialogueSequence secondFailDialogue;
    [Tooltip("How many fails must happen AFTER the first hint has finished playing before the "
           + "second hint can play. Fails before or during the first hint don't count.")]
    [Min(1)]
    [SerializeField] private int failsBeforeSecondHint = 2;
    [Tooltip("Played when the puzzle is solved.")]
    [SerializeField] private DialogueSequence solvedDialogue;

    [Header("Events")]
    public UnityEvent onCorrectCandle;   // each right candle (e.g. play a little chime)
    public UnityEvent onWrongCandle;     // wrong candle (e.g. sad trombone, cake shake)
    public UnityEvent onPuzzleReset;     // once all candles are out again
    public UnityEvent onPuzzleSolved;    // open door, play birthday song, etc.

    private Candle[] solution;
    private int progress;
    private bool busy;

    private bool introFinished;
    private int failCount;
    private int failsSinceFirstHintEnded;
    private bool firstHintPlayed;
    private bool firstHintEnded;
    private bool secondHintPlayed;

    public bool Solved { get; private set; }

    /// <summary>
    /// False while the intro hasn't finished, during a fail/reset, after solving, or while
    /// any dialogue is on screen. Candle uses this for both interaction and its prompt.
    /// </summary>
    public bool AcceptingInput =>
        introFinished && !busy && !Solved && !IsDialoguePlaying;

    private static bool IsDialoguePlaying =>
        DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying;

    private void Awake()
    {
        BuildSolution();
        foreach (var candle in candles)
            candle.Initialise(this);
    }

    private void Start()
    {
        // Subscribe in Start so DialogueManager.Awake() has definitely run.
        var manager = DialogueManager.Instance;

        // No manager in the scene: don't lock the player out forever.
        if (manager == null)
        {
            introFinished = true;
            return;
        }

        // No intro assigned, or it already played earlier this session (e.g. scene reloaded).
        introFinished = introDialogue == null || manager.HasPlayed(introDialogue);

        // Stays subscribed for the puzzle's lifetime: needed for the intro AND the first hint.
        manager.DialogueEnded += HandleDialogueEnded;
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.DialogueEnded -= HandleDialogueEnded;
    }

    private void HandleDialogueEnded(DialogueSequence sequence)
    {
        if (sequence == introDialogue)
            introFinished = true;

        // Only fails made AFTER the first hint has finished count toward the second hint.
        if (sequence == firstFailDialogue && firstHintPlayed)
            firstHintEnded = true;
    }

    private void BuildSolution()
    {
        solution = (Candle[])candles.Clone();

        if (!useRandomSolution) return;

        // Fisher-Yates shuffle with a seeded RNG so the order stays fixed for a given seed.
        var rng = new System.Random(seed);
        for (int i = solution.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (solution[i], solution[j]) = (solution[j], solution[i]);
        }
    }

    /// <summary>Called by a Candle when the player lights it.</summary>
    public void TryLight(Candle candle)
    {
        if (!AcceptingInput || candle.IsLit) return;

        candle.Light();

        if (candle == solution[progress])
        {
            progress++;
            onCorrectCandle?.Invoke();

            if (progress >= solution.Length)
                Solve();
        }
        else
        {
            StartCoroutine(FailRoutine());
        }
    }

    private void Solve()
    {
        Solved = true;
        PlayDialogue(solvedDialogue);
        onPuzzleSolved?.Invoke();
    }

    private IEnumerator FailRoutine()
    {
        busy = true;
        failCount++;
        if (firstHintEnded && !secondHintPlayed)
            failsSinceFirstHintEnded++;
        onWrongCandle?.Invoke();

        yield return new WaitForSeconds(wrongPauseSeconds);

        // Blow out candles in the order they were lit, or all at once if stagger is 0.
        foreach (var candle in candles)
        {
            candle.Extinguish();
            if (extinguishStagger > 0f)
                yield return new WaitForSeconds(extinguishStagger);
        }

        yield return new WaitForSeconds(afterResetPauseSeconds);

        progress = 0;
        busy = false;
        onPuzzleReset?.Invoke();

        // Hints play once the puzzle is fully reset. While they're on screen,
        // AcceptingInput stays false (IsDialoguePlaying).
        TryPlayFailDialogue();
    }

    private void TryPlayFailDialogue()
    {
        if (!firstHintPlayed)
        {
            if (failCount >= failsBeforeFirstHint)
            {
                firstHintPlayed = true;

                // If the hint didn't actually start (no sequence assigned, or it's a playOnce
                // that already played), no DialogueEnded will ever fire, so count it as ended.
                if (!PlayDialogue(firstFailDialogue))
                    firstHintEnded = true;
            }
        }
        else if (firstHintEnded && !secondHintPlayed)
        {
            if (failsSinceFirstHintEnded >= failsBeforeSecondHint)
            {
                secondHintPlayed = true;
                PlayDialogue(secondFailDialogue);
            }
        }
    }

    private static bool PlayDialogue(DialogueSequence sequence)
    {
        return sequence != null
            && DialogueManager.Instance != null
            && DialogueManager.Instance.PlayDialogue(sequence);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (candles == null) return;

        var seen = new HashSet<Candle>();
        foreach (var c in candles)
        {
            if (c == null) { Debug.LogWarning("CandlePuzzle: a candle slot is empty.", this); continue; }
            if (!seen.Add(c)) Debug.LogWarning($"CandlePuzzle: '{c.name}' is in the list twice.", this);
        }
    }
#endif
}