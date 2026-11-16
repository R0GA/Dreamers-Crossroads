using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Tracks the player's current checkpoint and respawns them there on request. Put this on
/// the same object as PlayerController (or a parent of it). Checkpoint and VoidRespawnZone
/// both talk to this — neither of them needs to know about PlayerController directly.
///
/// If a ScreenFader is assigned, a respawn plays as: fade to black -> teleport -> brief hold
/// -> fade back in. Without one it teleports instantly, as before.
/// </summary>
public class PlayerRespawner : MonoBehaviour
{
    [Tooltip("Used if the player falls before touching any Checkpoint yet — typically the "
           + "level's starting position.")]
    [SerializeField] private Transform initialSpawnPoint;

    [Header("Screen Fade")]
    [Tooltip("Optional. Leave empty for an instant, fade-less respawn.")]
    [SerializeField] private ScreenFader fader;

    [Tooltip("Seconds to fade to black before teleporting. Keep short so a fall feels like a "
           + "quick cut, not a long wait.")]
    [SerializeField] private float fadeOutTime = 0.2f;

    [Tooltip("Seconds held on black after the teleport, before fading back in.")]
    [SerializeField] private float holdTime = 0.1f;

    [Tooltip("Seconds to fade back in once at the checkpoint.")]
    [SerializeField] private float fadeInTime = 0.3f;

    /// <summary>Fired right after the teleport happens (screen is black if a fader is used) —
    /// hook SFX, quips, etc. off this.</summary>
    public event Action Respawned;

    /// <summary>True from the moment a respawn starts until the fade-in finishes. Respawn
    /// requests made during this window are ignored, so overlapping void boxes can't stack
    /// two respawns.</summary>
    public bool IsRespawning { get; private set; }

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
        if (IsRespawning) return;

        Transform target = currentCheckpoint != null ? currentCheckpoint : initialSpawnPoint;
        if (target == null)
        {
            Debug.LogWarning("PlayerRespawner has no checkpoint and no initialSpawnPoint set.");
            return;
        }

        StartCoroutine(RespawnRoutine(target));
    }

    private IEnumerator RespawnRoutine(Transform target)
    {
        IsRespawning = true;

        if (fader != null)
            yield return fader.Fade(1f, fadeOutTime);

        playerController.Respawn(target.position, target.rotation);
        Respawned?.Invoke();

        if (fader != null)
        {
            if (holdTime > 0f) yield return new WaitForSeconds(holdTime);
            yield return fader.Fade(0f, fadeInTime);
        }

        IsRespawning = false;
    }
}
