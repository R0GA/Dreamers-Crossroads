using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Listens to DialogueManager and renders whatever is currently playing: speaker name,
/// typewriter-revealed body text, and an optional portrait. Pressing the bound "advance"
/// action either fast-forwards the typewriter or, if the line is already fully revealed,
/// advances to the next line.
///
/// Drop this on your dialogue Canvas, wire up the fields, and it works with any
/// DialogueSequence with no further per-conversation setup.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root object toggled on/off to show/hide the whole dialogue panel.")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image portraitImage;

    [Header("Input")]
    [Tooltip("Action used to skip the typewriter / advance to the next line. Bind this to "
           + "the same key as Interact, or a dedicated 'Continue' action — your choice.")]
    [SerializeField] private InputActionReference advanceAction;

    [Header("Typewriter")]
    [SerializeField] private float charactersPerSecond = 40f;

    private Coroutine typeRoutine;
    private bool lineFullyRevealed;

    private void OnEnable()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (advanceAction != null) advanceAction.action.Enable();
    }

    private void Start()
    {
        // By the time Start runs, DialogueManager.Awake() is guaranteed to have finished.
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueUI could not find DialogueManager.Instance!");
            return;
        }

        DialogueManager.Instance.DialogueStarted += HandleDialogueStarted;
        DialogueManager.Instance.LineStarted += HandleLineStarted;
        DialogueManager.Instance.DialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        if (advanceAction != null) advanceAction.action.Disable();
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions here instead of OnDisable
        if (DialogueManager.Instance == null) return;

        DialogueManager.Instance.DialogueStarted -= HandleDialogueStarted;
        DialogueManager.Instance.LineStarted -= HandleLineStarted;
        DialogueManager.Instance.DialogueEnded -= HandleDialogueEnded;
    }

    private void Update()
    {
        if (panelRoot != null && !panelRoot.activeSelf) return;
        if (advanceAction == null || !advanceAction.action.WasPressedThisFrame()) return;

        if (!lineFullyRevealed)
        {
            // First press just reveals the rest of the line instantly
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            bodyText.maxVisibleCharacters = bodyText.text.Length;
            lineFullyRevealed = true;
        }
        else
        {
            DialogueManager.Instance.Advance();
        }
    }

    // ── Event Handlers ───────────────────────────────────────────────────────

    private void HandleDialogueStarted(DialogueSequence sequence)
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        Debug.Log($"Dialogue started: {sequence.name}");
    }

    private void HandleLineStarted(DialogueLine line)
    {
        if (speakerText != null) speakerText.text = line.speakerName;

        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = line.portrait != null;
        }

        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypewriterRoutine(line.text));
    }

    private void HandleDialogueEnded(DialogueSequence sequence)
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // ── Typewriter ───────────────────────────────────────────────────────────

    private IEnumerator TypewriterRoutine(string fullText)
    {
        lineFullyRevealed = false;
        bodyText.text = fullText;
        bodyText.maxVisibleCharacters = 0;

        float secondsPerChar = 1f / Mathf.Max(charactersPerSecond, 1f);
        int shown = 0;

        while (shown < fullText.Length)
        {
            shown++;
            bodyText.maxVisibleCharacters = shown;
            yield return new WaitForSeconds(secondsPerChar);
        }

        lineFullyRevealed = true;
    }
}
