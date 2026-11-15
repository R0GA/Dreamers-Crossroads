using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Listens to DialogueManager and renders whatever is currently playing: speaker name,
/// typewriter-revealed body text, and an optional portrait.
///
/// Player-paced lines: pressing the "advance" action first fast-forwards the typewriter,
/// then (once fully revealed) advances to the next line. A "Press X to continue" hint
/// appears once the line is fully revealed.
///
/// Auto-advance lines: the line moves on by itself after its delay, so no "continue" hint
/// is shown. Instead a "Press X to skip" hint is shown for as long as the line is running,
/// and pressing the advance action skips straight to the next line.
///
/// An optional separate "skip" action can also jump straight to the next line on any line.
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

    [Header("Optional Skip")]
    [Tooltip("Optional separate action that skips straight to the next line, with no "
           + "typewriter-finish step. Leave empty if you don't want a dedicated skip key.")]
    [SerializeField] private InputActionReference skipAction;

    [Header("Advance Hint")]
    [Tooltip("Small object (e.g. a text label + key icon) shown when the player can "
           + "continue or skip.")]
    [SerializeField] private GameObject advanceHintRoot;
    [Tooltip("Optional. Filled using the formats below and the action's actual binding.")]
    [SerializeField] private TMP_Text advanceHintText;
    [Tooltip("Shown on player-paced lines once fully revealed. {0} is replaced by the key.")]
    [SerializeField] private string continueHintFormat = "Press {0} to continue";
    [Tooltip("Shown on auto-advance lines while they run. {0} is replaced by the key.")]
    [SerializeField] private string skipHintFormat = "Press {0} to skip";

    [Header("Typewriter")]
    [SerializeField] private float charactersPerSecond = 40f;

    private Coroutine typeRoutine;
    private bool lineFullyRevealed;
    private bool currentLineAutoAdvances;
    private int dialogueStartFrame = -1;

    private void OnEnable()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (advanceAction != null) advanceAction.action.Enable();
        if (skipAction != null) skipAction.action.Enable();
        HideHint();
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
        if (skipAction != null) skipAction.action.Disable();
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

        // Ignore the press that opened the dialogue (e.g. Interact bound to the same key)
        if (Time.frameCount == dialogueStartFrame) return;

        // Hard skip: jump straight to the next line
        if (skipAction != null && skipAction.action.WasPressedThisFrame())
        {
            DialogueManager.Instance.Advance();
            return;
        }

        if (advanceAction == null || !advanceAction.action.WasPressedThisFrame()) return;

        // Auto-advance lines: the press always means "skip this line". There's no
        // reveal-first step, since the hint tells the player it will skip.
        if (currentLineAutoAdvances)
        {
            DialogueManager.Instance.Advance();
            return;
        }

        if (!lineFullyRevealed)
        {
            // First press just reveals the rest of the line instantly
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            bodyText.maxVisibleCharacters = bodyText.text.Length;
            lineFullyRevealed = true;
            ShowHint(continueHintFormat);
        }
        else
        {
            DialogueManager.Instance.Advance();
        }
    }

    // ── Event Handlers ───────────────────────────────────────────────────────

    private void HandleDialogueStarted(DialogueSequence sequence)
    {
        dialogueStartFrame = Time.frameCount;
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

        currentLineAutoAdvances = line.autoAdvanceDelay > 0f;

        // Auto-advance lines show the skip hint immediately and keep it for the whole line.
        // Player-paced lines hide the hint until the text is fully revealed.
        if (currentLineAutoAdvances) ShowHint(skipHintFormat);
        else HideHint();

        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypewriterRoutine(line.text));
    }

    private void HandleDialogueEnded(DialogueSequence sequence)
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        currentLineAutoAdvances = false;
        HideHint();
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

        // Auto-advance lines already show the skip hint; only paced lines switch to "continue".
        if (!currentLineAutoAdvances)
            ShowHint(continueHintFormat);
    }

    // ── Advance Hint ─────────────────────────────────────────────────────────

    private void ShowHint(string format)
    {
        if (advanceHintRoot != null) advanceHintRoot.SetActive(true);

        if (advanceHintText != null && advanceAction != null)
        {
            string key = advanceAction.action.GetBindingDisplayString();
            advanceHintText.text = string.Format(format, key);
        }
    }

    private void HideHint()
    {
        if (advanceHintRoot != null) advanceHintRoot.SetActive(false);
    }
}