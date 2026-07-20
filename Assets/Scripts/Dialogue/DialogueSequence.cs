using UnityEngine;

/// <summary>
/// An ordered set of DialogueLines that plays as one beat — a room-entry monologue, an NPC
/// conversation, a powerup pickup line, etc. Create these as assets via
/// Assets > Create > Dialogue > Dialogue Sequence, fill in the lines, then hand the asset
/// to a DialogueTriggerZone or DialogueInteractable (or call
/// DialogueManager.Instance.PlayDialogue(sequence) yourself from any other script).
/// </summary>
[CreateAssetMenu(fileName = "New Dialogue Sequence", menuName = "Dialogue/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Tooltip("Played in order, one after another.")]
    public DialogueLine[] lines;

    [Tooltip("If true, this sequence will only ever play once per game session, tracked by "
           + "the DialogueManager. Good for one-off story beats; leave off for repeatable "
           + "ambient lines (e.g. an NPC you can talk to again).")]
    public bool playOnce = false;
}
