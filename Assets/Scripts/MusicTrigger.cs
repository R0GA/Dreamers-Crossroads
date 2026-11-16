using UnityEngine;

/// <summary>
/// Crossfades the level music to a chosen track when the player enters this trigger volume.
/// Replaces SongSwitch. Use it for the Default -> Puzzle change partway through the level.
/// (The boss track is started by FightStartTrigger, and the return to Puzzle after the
/// fight is handled by MusicManager itself.)
/// </summary>
[RequireComponent(typeof(Collider))]
public class MusicTrigger : MonoBehaviour
{
    [Tooltip("Track to crossfade to when the player enters.")]
    [SerializeField] private MusicTrack track = MusicTrack.Puzzle;

    [Tooltip("Crossfade length in seconds. Leave negative to use the MusicManager's default.")]
    [SerializeField] private float fadeDuration = -1f;

    [Tooltip("Keep this on. Otherwise walking back through the volume later (e.g. into the "
           + "boss arena) would pull the music away from the track that should be playing.")]
    [SerializeField] private bool triggerOnce = true;

    private bool used;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && used) return;
        if (other.GetComponentInParent<PlayerController>() == null) return;
        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("MusicTrigger: no MusicManager in the scene.");
            return;
        }

        used = true;
        MusicManager.Instance.PlayTrack(track, fadeDuration);
    }
}
