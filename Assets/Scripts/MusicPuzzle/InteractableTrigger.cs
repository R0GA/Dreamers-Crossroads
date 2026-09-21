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

    public UnityEvent onInteract;

    private bool used = false;

    public void Interact(GameObject interactor)
    {
        if (oneShot && used) return;

        used = true;
        onInteract?.Invoke();
    }

    public string GetInteractionPrompt() => (oneShot && used) ? null : prompt;
}
