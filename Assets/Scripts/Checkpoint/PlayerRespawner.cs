using System;
using UnityEngine;

/// <summary>
/// Tracks the player's current checkpoint and respawns them there on request. Put this on
/// the same object as PlayerController (or a parent of it). Checkpoint and VoidRespawnZone
/// both talk to this — neither of them needs to know about PlayerController directly.
/// </summary>
public class PlayerRespawner : MonoBehaviour
{
    [Tooltip("Used if the player falls before touching any Checkpoint yet — typically the "
           + "level's starting position.")]
    [SerializeField] private Transform initialSpawnPoint;

    /// <summary>Fired right after a respawn happens — hook a screen fade, SFX, etc. off this.</summary>
    public event Action Respawned;

    private PlayerController playerController;
    private Transform currentCheckpoint;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        currentCheckpoint = initialSpawnPoint;
    }

    /// <summary>Called by a Checkpoint trigger when the player passes through it.</summary>
    public void SetCheckpoint(Transform checkpoint) => currentCheckpoint = checkpoint;

    /// <summary>Called by a VoidRespawnZone (or anything else — a bottomless-pit hazard, a
    /// "reset" debug key) to send the player back to their last checkpoint.</summary>
    public void Respawn()
    {
        Transform target = currentCheckpoint != null ? currentCheckpoint : initialSpawnPoint;
        if (target == null)
        {
            Debug.LogWarning("PlayerRespawner has no checkpoint and no initialSpawnPoint set.");
            return;
        }

        playerController.Respawn(target.position, target.rotation);
        Respawned?.Invoke();
    }
}
