using UnityEngine;

/// <summary>
/// Place at each checkpoint along the level — a floating platform, the start of an island,
/// etc. Needs a trigger Collider sized to catch the player as they pass through/over it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Tag used to identify the player.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Exact position/rotation the player respawns at. Leave empty to use this "
           + "object's own transform — set a separate child transform if you want the "
           + "trigger volume and the actual spawn point to differ (e.g. spawn a step back "
           + "from the ledge instead of exactly on the trigger).")]
    [SerializeField] private Transform respawnPoint;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerRespawner respawner = other.GetComponentInParent<PlayerRespawner>();
        if (respawner == null) return;

        respawner.SetCheckpoint(respawnPoint != null ? respawnPoint : transform);
    }
}
