using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Drives a simple end-of-game scene: fades in from black, slowly scrolls a block of text
/// across the screen, holds for a moment, fades back to black, then loads the main menu.
///
/// Put this on an empty GameObject in the end scene. The scrolling text is any UI element
/// (usually a TextMeshPro - Text (UI) with a Content Size Fitter set to Preferred Size
/// vertically) that you assign to 'Content'. The fade is a full-screen black Image assigned
/// to 'Fade Overlay'. Both live under a Screen Space - Overlay Canvas.
///
/// Also performs the same clean-up as the pause menu's "Back to Menu" before loading the menu
/// (resets the persistent DialogueManager, unfreezes time/audio, frees the cursor), so players
/// can start a fresh run with working dialogue.
/// </summary>
public class EndGameScene : MonoBehaviour
{
    public enum ScrollDirection
    {
        [Tooltip("Classic credits roll: text starts below the screen and moves up, reading top to bottom.")]
        Up,
        [Tooltip("Text starts above the screen and drifts downward.")]
        Down
    }

    [Header("Scrolling Text")]
    [Tooltip("The UI element that scrolls. Its width is yours to set; its height should come from a "
           + "Content Size Fitter so it matches the amount of text.")]
    [SerializeField] private RectTransform content;

    [SerializeField] private ScrollDirection direction = ScrollDirection.Up;

    [Tooltip("Scroll speed in canvas units per second. Lower = slower. Around 50-80 is a gentle credits pace.")]
    [SerializeField, Min(1f)] private float scrollSpeed = 60f;

    [Tooltip("Seconds of stillness after the fade-in before the text starts moving.")]
    [SerializeField, Min(0f)] private float startDelay = 1.5f;

    [Tooltip("Seconds to wait after the text has fully scrolled away before fading to black.")]
    [SerializeField, Min(0f)] private float holdAfterScroll = 1.5f;

    [Header("Fade")]
    [Tooltip("Full-screen black Image covering everything else (including the text).")]
    [SerializeField] private Image fadeOverlay;
    [SerializeField, Min(0f)] private float fadeInDuration = 1.5f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 2.5f;

    [Tooltip("Optional. Its volume is faded out together with the screen.")]
    [SerializeField] private AudioSource music;

    [Header("After")]
    [Tooltip("Scene to load when finished. It must be in the Build Profiles scene list.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Skip")]
    [Tooltip("Lets the player press Escape / gamepad Start to jump straight to the fade-out.")]
    [SerializeField] private bool allowSkip = true;

    private InputAction skipAction;
    private bool skipRequested;
    private bool fadingOut;

    private void Awake()
    {
        skipAction = new InputAction("Skip", InputActionType.Button);
        skipAction.AddBinding("<Keyboard>/escape");
        skipAction.AddBinding("<Gamepad>/start");
        skipAction.performed += _ => { if (allowSkip && !fadingOut) skipRequested = true; };
    }

    private void OnEnable() => skipAction.Enable();
    private void OnDisable() => skipAction.Disable();
    private void OnDestroy() => skipAction.Dispose();

    private void Start()
    {
        // Arrive from a game scene that might have left these in an odd state
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Start fully black so nothing flashes before the first fade-in frame
        SetFade(1f);

        StartCoroutine(Sequence());
    }

    // ── Sequence ──────────────────────────────────────────────────────────────

    private IEnumerator Sequence()
    {
        // One frame so layout (Content Size Fitter / TMP) has produced the real text height
        yield return null;
        PrepareScroll(out float startY, out float endY);

        yield return FadeScreen(1f, 0f, fadeInDuration, fadeMusic: false);
        yield return WaitUnlessSkipped(startDelay);

        yield return Scroll(startY, endY);
        yield return WaitUnlessSkipped(holdAfterScroll);

        fadingOut = true;
        yield return FadeScreen(0f, 1f, fadeOutDuration, fadeMusic: true);

        LoadMainMenu();
    }

    /// <summary>Works out where the content starts and ends so it begins and finishes fully off-screen.</summary>
    private void PrepareScroll(out float startY, out float endY)
    {
        startY = endY = 0f;

        if (content == null)
        {
            Debug.LogError("EndGameScene: no Content assigned, so nothing will scroll.", this);
            return;
        }

        // Centre anchors + pivot keep the maths simple whatever the element was set up as.
        float width = content.rect.width;
        content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
        content.pivot = new Vector2(0.5f, 0.5f);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Canvas canvas = content.GetComponentInParent<Canvas>();
        RectTransform viewport = canvas != null ? canvas.rootCanvas.GetComponent<RectTransform>() : null;
        float viewHeight = viewport != null ? viewport.rect.height : Screen.height;

        // Centre-to-centre distance at which the content has just cleared the screen edge
        float offscreen = (viewHeight + content.rect.height) * 0.5f;

        startY = direction == ScrollDirection.Up ? -offscreen : offscreen;
        endY = -startY;

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, startY);
    }

    private IEnumerator Scroll(float startY, float endY)
    {
        if (content == null) yield break;

        float distance = Mathf.Abs(endY - startY);
        float sign = Mathf.Sign(endY - startY);
        float travelled = 0f;

        while (travelled < distance && !skipRequested)
        {
            travelled += scrollSpeed * Time.unscaledDeltaTime;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, startY + sign * Mathf.Min(travelled, distance));
            yield return null;
        }
    }

    private IEnumerator WaitUnlessSkipped(float seconds)
    {
        float waited = 0f;
        while (waited < seconds && !skipRequested)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeScreen(float from, float to, float duration, bool fadeMusic)
    {
        float startVolume = music != null ? music.volume : 0f;

        if (duration <= 0f)
        {
            SetFade(to);
            if (fadeMusic && music != null) music.volume = 0f;
            yield break;
        }

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            SetFade(Mathf.Lerp(from, to, k));
            if (fadeMusic && music != null) music.volume = Mathf.Lerp(startVolume, 0f, k);
            yield return null;
        }

        SetFade(to);
        if (fadeMusic && music != null) music.volume = 0f;
    }

    private void SetFade(float alpha)
    {
        if (fadeOverlay == null) return;

        Color c = fadeOverlay.color;
        c.a = alpha;
        fadeOverlay.color = c;
    }

    // ── Leaving ───────────────────────────────────────────────────────────────

    private void LoadMainMenu()
    {
        // DialogueManager persists across scenes: wipe any half-played sequence and the
        // "already played" history so the next run's dialogue isn't broken or skipped.
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ResetAll();

        Time.timeScale = 1f;
        AudioListener.pause = false;

        // The menu needs a usable cursor (PlayerController locked it during the game)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
