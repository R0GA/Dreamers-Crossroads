using UnityEngine;

/// <summary>
/// Plays a DialogueSequence when the player enters a trigger volume — e.g. stepping into a
/// room and having the character think out loud. Put this on an empty GameObject with a
/// trigger Collider sized to the area you want to cover.
///
/// The zone keeps trying for as long as the player stands inside it, not just on the frame
/// they enter. So if another dialogue is still playing when the player walks in, this one
/// starts as soon as that one ends — no need to leave and re-enter the zone.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DialogueTriggerZone : MonoBehaviour
{
    [Tooltip("Dialogue to play when the player enters this zone.")]
    [SerializeField] private DialogueSequence sequence;

    [Tooltip("Tag used to identify the player. Change this if your player object uses a "
           + "different tag.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("If true, this zone can only ever fire once. If false, it fires again on each "
           + "separate visit (leave the zone and come back), subject to reTriggerCooldown.")]
    [SerializeField] private bool triggerOnce = true;

    [Tooltip("Minimum seconds between fires when triggerOnce is false. Stops the same line "
           + "spamming if the player lingers on the trigger's edge.")]
    [SerializeField] private float reTriggerCooldown = 3f;

    private bool hasFired;       // fired at least once, ever (used by triggerOnce)
    private bool firedThisVisit; // fired since the player last entered (resets on exit)
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

    private void OnTriggerEnter(Collider other) => TryFire(other);

    // Keep trying while the player stays inside, so a dialogue that was blocked because
    // another one was playing starts as soon as the blocker finishes.
    private void OnTriggerStay(Collider other) => TryFire(other);

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) firedThisVisit = false;
    }

    private void TryFire(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (triggerOnce && hasFired) return;
        if (firedThisVisit) return; // already played for this visit; don't repeat while they linger
        if (!triggerOnce && cooldownTimer > 0f) return;

        var manager = DialogueManager.Instance;
        if (manager == null || sequence == null) return;

        // A playOnce sequence that already played can never start again; stop retrying.
        if (manager.HasPlayed(sequence)) return;

        // Returns false if another dialogue is currently playing. Nothing is marked as fired in
        // that case, so this is simply tried again on the next physics step while the player
        // is still inside.
        if (manager.PlayDialogue(sequence))
        {
            hasFired = true;
            firedThisVisit = true;
            cooldownTimer = reTriggerCooldown;
        }
    }
}