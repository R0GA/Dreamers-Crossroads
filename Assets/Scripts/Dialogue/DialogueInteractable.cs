using UnityEngine;

/// <summary>
/// Drop this on anything the player should be able to talk to, or that should say a line
/// when interacted with — an NPC, a sign, a lever with something to say. Implements
/// IInteractable so PlayerController picks it up automatically through its existing
/// raycast/interact pipeline; no other wiring needed.
///
/// If a piece of interactable already has its own script (e.g. an ability pickup that
/// grants a PlayerAbility), you don't need this component at all — just call
/// DialogueManager.Instance.PlayDialogue(sequence) directly from that script's own
/// Interact() alongside whatever else it does.
/// </summary>
public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Played on the primary Interact press.")]
    [SerializeField] private DialogueSequence sequence;

    [Tooltip("Optional — played on InteractAlt instead of the primary sequence. Leave empty "
           + "if this object doesn't need a secondary line.")]
    [SerializeField] private DialogueSequence altSequence;

    [Tooltip("Shown in the interaction prompt UI, e.g. 'Talk' or 'Inspect'.")]
    [SerializeField] private string interactionPrompt = "Talk";

    public void Interact(GameObject interactor)
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.PlayDialogue(sequence);
    }

    public void InteractAlt(GameObject interactor)
    {
        if (altSequence != null && DialogueManager.Instance != null)
            DialogueManager.Instance.PlayDialogue(altSequence);
    }

    public string GetInteractionPrompt()
    {
        // Don't show a prompt for an empty sequence, or while dialogue is already playing
        if (sequence == null) return null;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return null;
        return interactionPrompt;
    }
}
