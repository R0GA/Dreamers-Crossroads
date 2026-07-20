using UnityEngine;

/// <summary>
/// Plays a DialogueSequence when the player enters a trigger volume — e.g. stepping into a
/// room and having the character think out loud. Put this on an empty GameObject with a
/// trigger Collider sized to the area you want to cover.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DialogueTriggerZone : MonoBehaviour
{
    [Tooltip("Dialogue to play when the player enters this zone.")]
    [SerializeField] private DialogueSequence sequence;

    [Tooltip("Tag used to identify the player. Change this if your player object uses a "
           + "different tag.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("If true, this zone can only ever fire once. If false, it fires again every "
           + "time the player re-enters (subject to reTriggerCooldown).")]
    [SerializeField] private bool triggerOnce = true;

    [Tooltip("Minimum seconds between fires when triggerOnce is false. Stops the same line "
           + "spamming if the player lingers on the trigger's edge.")]
    [SerializeField] private float reTriggerCooldown = 3f;

    private bool hasFired;
    private float cooldownTimer;

    private void Reset()
    {
        // Zones are almost always meant to be walk-through, not solid — default to trigger.
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (triggerOnce && hasFired) return;
        if (!triggerOnce && cooldownTimer > 0f) return;
        if (DialogueManager.Instance == null) return;

        if (DialogueManager.Instance.PlayDialogue(sequence))
        {
            hasFired = true;
            cooldownTimer = reTriggerCooldown;
        }
    }
}
