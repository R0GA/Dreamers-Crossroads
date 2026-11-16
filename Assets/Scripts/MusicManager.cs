using System.Collections;
using UnityEngine;

public enum MusicTrack { Default, Puzzle, Boss }

/// <summary>
/// Level music with crossfading. Uses two AudioSources — whichever is currently audible
/// fades out while the other starts the new track and fades in, then they swap roles.
///
/// Tracks: Default (plays on awake) -> Puzzle (MusicTrigger partway through the level) ->
/// Boss (FightStartTrigger on arena entry) -> back to Puzzle when the boss fight's
/// onDefeated fires. The defeat hook is subscribed automatically; no inspector wiring.
///
/// Scene-local singleton: put one in each level that has music.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("The two sources that trade places during a crossfade. Leave empty to have "
           + "them created automatically.")]
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;

    [Header("Tracks")]
    [SerializeField] private AudioClip defaultTrack;
    [SerializeField] private AudioClip puzzleTrack;
    [SerializeField] private AudioClip bossTrack;
    [SerializeField] private bool playDefaultOnAwake = true;

    [Header("Fading")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;
    [Tooltip("Crossfade length in seconds when a trigger doesn't specify its own.")]
    [SerializeField] private float defaultFadeDuration = 3f;

    [Header("Boss")]
    [Tooltip("Music returns to the puzzle track when this fight's onDefeated fires. Auto-found "
           + "in the scene if left empty.")]
    [SerializeField] private SirNightmareFight bossFight;

    private AudioSource active;
    private Coroutine fadeRoutine;

    private AudioSource Inactive => active == sourceA ? sourceB : sourceA;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sourceA = Prepare(sourceA);
        sourceB = Prepare(sourceB);
        active = sourceA;

        if (playDefaultOnAwake && defaultTrack != null)
        {
            active.clip = defaultTrack;
            active.volume = musicVolume;
            active.Play();
        }
    }

    private void Start()
    {
        if (bossFight == null) bossFight = FindFirstObjectByType<SirNightmareFight>();
        if (bossFight != null) bossFight.onDefeated.AddListener(PlayPuzzleMusic);
    }

    private void OnDestroy()
    {
        if (bossFight != null) bossFight.onDefeated.RemoveListener(PlayPuzzleMusic);
        if (Instance == this) Instance = null;
    }

    private AudioSource Prepare(AudioSource source)
    {
        if (source == null) source = gameObject.AddComponent<AudioSource>();

        source.Stop();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        return source;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Crossfades to one of the level's named tracks. A negative fadeDuration uses
    /// the manager's default.</summary>
    public void PlayTrack(MusicTrack track, float fadeDuration = -1f)
    {
        AudioClip clip = track switch
        {
            MusicTrack.Default => defaultTrack,
            MusicTrack.Puzzle => puzzleTrack,
            MusicTrack.Boss => bossTrack,
            _ => null
        };

        CrossfadeTo(clip, fadeDuration);
    }

    // Parameterless versions so UnityEvents can call them from the inspector.
    public void PlayDefaultMusic() => PlayTrack(MusicTrack.Default);
    public void PlayPuzzleMusic() => PlayTrack(MusicTrack.Puzzle);
    public void PlayBossMusic() => PlayTrack(MusicTrack.Boss);

    /// <summary>Crossfades to any clip. Does nothing if that clip is already the one playing,
    /// so re-triggering never restarts a track.</summary>
    public void CrossfadeTo(AudioClip clip, float fadeDuration = -1f)
    {
        if (clip == null) return;
        if (active.clip == clip && active.isPlaying) return;

        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        // Interrupted mid-fade: the source that was fading out is reused for the new track,
        // so silence it now. The one that was fading in carries on as the outgoing track,
        // starting from whatever volume it had reached.
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
            Inactive.Stop();
            Inactive.volume = 0f;
        }

        AudioSource from = active;
        AudioSource to = Inactive;

        to.clip = clip;
        to.volume = 0f;
        to.Play();
        active = to;

        if (fadeDuration <= 0f)
        {
            to.volume = musicVolume;
            from.volume = 0f;
            from.Stop();
            return;
        }

        fadeRoutine = StartCoroutine(CrossfadeRoutine(from, to, fadeDuration));
    }

    private IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to, float duration)
    {
        float fromStartVolume = from.volume;
        float elapsed = 0f;

        // Unscaled time so the music still fades if the game is paused or slowed.
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            to.volume = Mathf.Lerp(0f, musicVolume, t);
            from.volume = Mathf.Lerp(fromStartVolume, 0f, t);
            yield return null;
        }

        to.volume = musicVolume;
        from.volume = 0f;
        from.Stop();
        fadeRoutine = null;
    }
}
