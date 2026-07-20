using UnityEngine;

/// <summary>
/// A big trigger volume placed well below your lowest floating platform/island. Catches
/// missed jumps and sends the player back to their last checkpoint. Since it's a trigger
/// volume rather than a fixed Y check, it doesn't care about the level's shape — just make
/// it wide/deep enough that nothing can fall around it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class VoidRespawnZone : MonoBehaviour
{
    [Tooltip("Tag used to identify the player.")]
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerRespawner respawner = other.GetComponentInParent<PlayerRespawner>();
        if (respawner != null) respawner.Respawn();
    }
}
