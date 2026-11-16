using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Minimal reusable trigger for anything that should fire an action when the player
/// interacts with it — a lever, a switch, a plaque. Wire onInteract to
/// SongSequencePuzzle.BeginPuzzle() (or literally anything else) in the Inspector; no
/// puzzle-specific code needed here or in PlayerController.
/// </summary>
public class InteractableTrigger : MonoBehaviour, IInteractable
{
    [Tooltip("Shown in the interaction prompt UI, e.g. \"Pull Lever\".")]
    [SerializeField] private string prompt = "Interact";

    [Tooltip("Once used, ignore further presses (good for a one-shot lever). Leave off for a repeatable switch.")]
    [SerializeField] private bool oneShot = false;

    [Tooltip("Optional. If assigned, this trigger is unavailable until that puzzle is ready to begin " +
             "(i.e. its intro dialogue has finished): no prompt is shown and presses are ignored " +
             "WITHOUT using up a one-shot. Leave empty for an ordinary, always-available trigger.")]
    [SerializeField] private SongSequencePuzzle gatedByPuzzle;

    public UnityEvent onInteract;

    private bool used = false;

    private bool Available =>
        !(oneShot && used) &&
        (gatedByPuzzle == null || gatedByPuzzle.IsReadyToBegin);

    public void Interact(GameObject interactor)
    {
        if (!Available) return;

        used = true;
        onInteract?.Invoke();
    }

    public string GetInteractionPrompt() => Available ? prompt : null;
}