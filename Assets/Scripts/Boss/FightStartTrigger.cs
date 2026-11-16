using UnityEngine;

/// <summary>Trigger volume at the arena entrance. Starts the fight the first time the player
/// walks in, and crossfades the level music to the boss track.</summary>
[RequireComponent(typeof(Collider))]
public class FightStartTrigger : MonoBehaviour
{
    [SerializeField] private SirNightmareFight fight;

    private bool used;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (used || fight == null) return;
        if (other.GetComponentInParent<PlayerController>() == null) return;

        used = true;
        fight.StartFight();

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayTrack(MusicTrack.Boss);
    }
}