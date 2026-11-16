using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Simple pause menu: Resume, Back to Menu, Quit. Toggled with Escape (keyboard) or Start
/// (gamepad), or via the Resume button.
///
/// Pausing freezes time (Time.timeScale = 0), pauses all audio, switches off PlayerController
/// so look/jump/interact/grapple input is ignored, and frees the cursor. Resuming undoes all of
/// that exactly.
///
/// Setup: put this script on an object that is ALWAYS ACTIVE (e.g. the Canvas root or an empty
/// GameObject) — NOT on the pause panel itself, because a disabled object can't listen for the
/// key that unpauses. Assign the panel (which this script hides/shows) and the three buttons.
/// The scene needs an EventSystem using the Input System UI module for the buttons to work.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The panel shown while paused. Should be a CHILD of the object this script is on.")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button quitButton;

    [Header("Scenes")]
    [Tooltip("Name of the main menu scene. It must be added to the Build Profiles scene list.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Player")]
    [Tooltip("Switched off while paused. Found automatically if left empty.")]
    [SerializeField] private PlayerController player;

    public static bool IsPaused { get; private set; }

    private InputAction pauseAction;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        // Defined here (not in your Input Actions asset) so it keeps working while the player's
        // own action map is switched off, and needs no setup.
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        pauseAction.performed += OnPausePressed;

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (menuButton != null) menuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void OnEnable() => pauseAction.Enable();
    private void OnDisable() => pauseAction.Disable();

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (pausePanel != null) pausePanel.SetActive(false);
        IsPaused = false;
    }

    private void OnDestroy()
    {
        pauseAction.performed -= OnPausePressed;
        pauseAction.Dispose();

        // If this object is destroyed while paused (e.g. a scene reload), don't leave the
        // game frozen.
        if (IsPaused) Unfreeze();
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (IsPaused) Resume();
        else Pause();
    }

    // ── Public API (also usable from other scripts / UnityEvents) ─────────────

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (player != null) player.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pausePanel != null) pausePanel.SetActive(true);

        // Pre-select Resume so gamepad / keyboard navigation works straight away
        if (EventSystem.current != null && resumeButton != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Resume()
    {
        if (!IsPaused) return;

        Unfreeze();

        if (player != null) player.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Unfreeze();

        // DialogueManager persists across scenes, so wipe its state or a half-played sequence
        // and the "already played" history would carry into the next run.
        if (DialogueManager.Instance != null) DialogueManager.Instance.ResetAll();

        // The menu scene needs a usable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void Unfreeze()
    {
        IsPaused = false;
        Time.timeScale = previousTimeScale;
        AudioListener.pause = false;
    }
}
