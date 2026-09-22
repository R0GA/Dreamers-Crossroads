using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Owns a set of DanceFloorPanels and watches for the puzzle-solved condition: every panel
/// showing its own individually assigned target color — not just "all panels matching each
/// other". Panels report in via NotifyPanelChanged whenever they change color, so the win
/// condition is only re-checked on an actual change rather than polled every frame.
/// </summary>
public class DanceFloorPuzzleManager : MonoBehaviour
{
    [Tooltip("Panels belonging to this puzzle. Leave empty to auto-collect every " +
             "DanceFloorPanel found in this object's children on Awake.")]
    [SerializeField] private DanceFloorPanel[] panels;

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
    [SerializeField] private DialogueSequence solvedDialogue;

    public bool IsSolved { get; private set; }

    private void Awake()
    {
        if (panels == null || panels.Length == 0)
            panels = GetComponentsInChildren<DanceFloorPanel>();

        foreach (var panel in panels)
            panel.Initialize(this);
    }

    private void Start()
    {
        // Catch the (unlikely but possible) case where every panel already starts on its target color.
        CheckWinCondition();
    }

    /// <summary>Called by a DanceFloorPanel whenever it changes color.</summary>
    public void NotifyPanelChanged(DanceFloorPanel changedPanel)
    {
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        bool allSolved = true;
        foreach (var panel in panels)
        {
            if (!panel.IsSolved)
            {
                allSolved = false;
                break;
            }
        }

        if (allSolved && !IsSolved)
        {
            IsSolved = true;
            if (lockOnSolve) SetPanelsInteractable(false);
            if (DialogueManager.Instance != null && solvedDialogue != null)
                DialogueManager.Instance.PlayDialogue(solvedDialogue);
            onPuzzleSolved?.Invoke();
        }
        else if (!allSolved && IsSolved)
        {
            IsSolved = false;
            onPuzzleUnsolved?.Invoke();
        }
    }

    private void SetPanelsInteractable(bool interactable)
    {
        foreach (var panel in panels)
            panel.SetInteractable(interactable);
    }

    /// <summary>Resets every panel to its starting color and unlocks them. Hook this up to a retry trigger/button if you want one.</summary>
    public void ResetPuzzle()
    {
        IsSolved = false;
        SetPanelsInteractable(true);
        foreach (var panel in panels)
            panel.ResetPanel();
    }
}