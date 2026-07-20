using UnityEngine;
using TMPro;

/// <summary>
/// Shows a small "[E] Rotate Ring" style prompt whenever the player is looking at an
/// IInteractable that has opted into a prompt (i.e. GetInteractionPrompt() returns
/// non-empty text). Interactables that return null/"" are simply never shown — no flag
/// or extra setup needed on their end.
///
/// Not using TextMeshPro? Swap the TMP_Text field/type below for UnityEngine.UI.Text.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;

    [Tooltip("The panel/GameObject to show or hide. Needs a CanvasGroup for the fade.")]
    [SerializeField] private CanvasGroup promptRoot;

    [SerializeField] private TMP_Text promptText;

    [Header("Key Label")]
    [Tooltip("Shown before the prompt text, e.g. \"[E] Rotate Ring\". Update this if you rebind Interact.")]
    [SerializeField] private string keyLabel = "E";

    [Header("Feel")]
    [SerializeField] private float fadeSpeed = 12f;

    IInteractable _lastInteractable;
    float _targetAlpha;

    void Reset()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
    }

    void Awake()
    {
        if (promptRoot != null)
        {
            promptRoot.alpha = 0f;
            _targetAlpha = 0f;
        }
    }

    void Update()
    {
        if (player == null || promptRoot == null) return;

        IInteractable interactable = player.CurrentInteractable;

        if (interactable != _lastInteractable)
        {
            _lastInteractable = interactable;

            string prompt = interactable?.GetInteractionPrompt();
            bool show = !string.IsNullOrEmpty(prompt);

            _targetAlpha = show ? 1f : 0f;
            if (show && promptText != null) promptText.text = $"[{keyLabel}] {prompt}";
        }

        promptRoot.alpha = Mathf.MoveTowards(promptRoot.alpha, _targetAlpha, fadeSpeed * Time.deltaTime);
    }
}
