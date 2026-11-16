using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Owns a set of DanceFloorPanels and watches for the puzzle-solved condition: every panel
/// showing its own individually assigned target color — not just "all panels matching each
/// other". Panels report in via NotifyPanelChanged whenever they change color, so the win
/// condition is only re-checked on an actual change rather than polled every frame.
///
/// DIALOGUE:
///  - Intro: played by a DialogueTriggerZone (assign the SAME DialogueSequence asset to the
///    zone and to 'Intro Dialogue' here). Panels ignore footsteps until it ends.
///  - First correct: plays the first time the player turns a tile to its correct color.
///  - Unturned: plays (once) the first time the player turns a correct tile back to a wrong
///    color, but only after the intro AND the first-correct dialogue have finished.
///  - Final: plays once more than half the tiles are correct.
///  - Only the intro locks the floor. Otherwise the player can keep stepping while dialogue
///    plays. A dialogue is never cut off or overlapped: if another sequence is already
///    running when one of these is due, it simply doesn't play that time and is re-checked
///    on the next qualifying step (next correct tile / next unturned tile).
/// </summary>
public class DanceFloorPuzzleManager : MonoBehaviour
{
    [Tooltip("Panels belonging to this puzzle. Leave empty to auto-collect every " +
             "DanceFloorPanel found in this object's children on Awake.")]
    [SerializeField] private DanceFloorPanel[] panels;

    [Header("Dialogue")]
    [Tooltip("Must be the same asset your DialogueTriggerZone plays. Panels stay locked until " +
             "it finishes. Leave empty to have the puzzle usable from the start.")]
    [SerializeField] private DialogueSequence introDialogue;
    [Tooltip("Plays the first time the player turns a tile to the correct color.")]
    [SerializeField] private DialogueSequence firstCorrectDialogue;
    [Tooltip("Plays once, the first time a correct tile is turned back to a wrong color. " +
             "Can only play after the intro and first-correct dialogue have finished.")]
    [SerializeField] private DialogueSequence unturnedDialogue;
    [Tooltip("Plays once when more than half of the tiles are on their correct color.")]
    [SerializeField] private DialogueSequence finalDialogue;

    [Header("Events")]
    public UnityEvent onPuzzleSolved;

    [Tooltip("Fires if a previously-solved puzzle gets broken again by a stray step. " +
             "Optional — leave empty if you don't want the puzzle to be 'un-solvable'.")]
    public UnityEvent onPuzzleUnsolved;

    [Header("Lock")]
    [Tooltip("If true, panels stop responding to footsteps the moment the puzzle is solved, " +
             "freezing progress so the player can't accidentally mess it up afterward. " +
             "If false, the player can keep stepping on panels and unsolve it (onPuzzleUnsolved still fires).")]
    [SerializeField] private bool lockOnSolve = false;

    public bool IsSolved { get; private set; }

    /// <summary>
    /// False only until the intro has finished. Panels check this before reacting to a
    /// footstep. Hint dialogue does NOT lock the floor.
    /// </summary>
    public bool AcceptingInput => introFinished;

    private bool introFinished;
    private int solvedCount;
    private bool firstCorrectPlayed;
    private bool firstCorrectEnded;
    private bool unturnedPlayed;
    private bool finalPlayed;

    private void Awake()
    {
        if (panels == null || panels.Length == 0)
            panels = GetComponentsInChildren<DanceFloorPanel>();

        foreach (var panel in panels)
            panel.Initialize(this);
    }

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

        solvedCount = CountSolvedPanels();

        // Catch the (unlikely but possible) case where every panel already starts on its target color.
        CheckWinCondition();
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

        if (sequence == firstCorrectDialogue && firstCorrectPlayed)
            firstCorrectEnded = true;
    }

    /// <summary>Called by a DanceFloorPanel whenever it changes color.</summary>
    public void NotifyPanelChanged(DanceFloorPanel changedPanel)
    {
        // Only one panel changes per step, so the change in the solved count tells us whether
        // that tile just became correct (+1) or was turned away from correct (-1).
        int newCount = CountSolvedPanels();
        int delta = newCount - solvedCount;
        solvedCount = newCount;

        HandleDialogue(delta, newCount);
        CheckWinCondition();
    }

    private void HandleDialogue(int delta, int newCount)
    {
        // A beat is only marked as played if it actually started (or there's nothing to play).
        // If another sequence is running (Busy), it's left pending and re-checked next time.

        // Final: more than half the tiles correct, evaluated when a tile is turned correct.
        // Takes priority if it lands on the same step as the first correct tile (e.g. a very
        // small floor); the first-correct line is then skipped.
        bool majority = newCount * 2 > panels.Length;
        if (delta > 0 && majority && !finalPlayed)
        {
            PlayResult result = TryPlay(finalDialogue);

            if (result == PlayResult.Started)
            {
                finalPlayed = true;
                if (!firstCorrectPlayed)
                {
                    firstCorrectPlayed = true;
                    firstCorrectEnded = true;
                }
                return;
            }

            if (result == PlayResult.Busy) return; // try again on the next correct tile
            finalPlayed = true;                    // nothing assigned: don't keep retrying
        }

        // First hint: the first time any tile is turned to its correct color.
        if (delta > 0 && !firstCorrectPlayed)
        {
            PlayResult result = TryPlay(firstCorrectDialogue);

            if (result == PlayResult.Started)
            {
                firstCorrectPlayed = true; // firstCorrectEnded is set when it finishes
            }
            else if (result == PlayResult.Skipped)
            {
                // Nothing to play, so nothing to wait for before the unturned hint unlocks.
                firstCorrectPlayed = true;
                firstCorrectEnded = true;
            }
            return;
        }

        // Unturned hint: a correct tile turned wrong, once the earlier dialogue has played out.
        if (delta < 0 && firstCorrectEnded && !unturnedPlayed)
        {
            if (TryPlay(unturnedDialogue) != PlayResult.Busy)
                unturnedPlayed = true;
        }
    }

    private int CountSolvedPanels()
    {
        int count = 0;
        foreach (var panel in panels)
            if (panel.IsSolved) count++;
        return count;
    }

    private void CheckWinCondition()
    {
        bool allSolved = solvedCount == panels.Length;

        if (allSolved && !IsSolved)
        {
            IsSolved = true;
            if (lockOnSolve) SetPanelsInteractable(false);
            onPuzzleSolved?.Invoke();
        }
        else if (!allSolved && IsSolved)
        {
            IsSolved = false;
            onPuzzleUnsolved?.Invoke();
        }
    }

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

    private void SetPanelsInteractable(bool interactable)
    {
        foreach (var panel in panels)
            panel.SetInteractable(interactable);
    }

    /// <summary>
    /// Resets every panel to its starting color and unlocks them. Hook this up to a retry
    /// trigger/button if you want one. Dialogue beats that already played stay played.
    /// </summary>
    public void ResetPuzzle()
    {
        IsSolved = false;
        SetPanelsInteractable(true);
        foreach (var panel in panels)
            panel.ResetPanel();

        solvedCount = CountSolvedPanels();
    }
}