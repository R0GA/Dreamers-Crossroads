using UnityEngine;

/// <summary>
/// A single candle on the cake. Needs a Collider on this object or a child, on a layer
/// included in PlayerController's Interaction Mask.
/// It doesn't decide whether it was lit "correctly" - it just reports to the CandlePuzzle.
/// </summary>
public class Candle : MonoBehaviour, IInteractable
{
    [Header("Visuals")]
    [Tooltip("Child object holding the flame particle system and/or point light. Disabled while unlit.")]
    [SerializeField] private GameObject flame;

    [Header("Interaction Prompt")]
    [Tooltip("Prompt text shown while the candle can be lit. Leave empty to show no prompt.")]
    [SerializeField] private string promptText = "Light Candle";

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lightSound;
    [SerializeField] private AudioClip extinguishSound;

    private CandlePuzzle puzzle;

    public bool IsLit { get; private set; }

    private bool CanBeLit => !IsLit && puzzle != null && puzzle.AcceptingInput;

    /// <summary>Called by CandlePuzzle at startup.</summary>
    public void Initialise(CandlePuzzle owner)
    {
        puzzle = owner;
        SetLit(false, playSound: false);
    }

    // ── IInteractable ─────────────────────────────────────────────────────────

    public void Interact(GameObject interactor)
    {
        if (!CanBeLit) return;
        puzzle.TryLight(this);
    }

    // Only show a prompt while the candle can actually be lit (hides it when lit,
    // during a reset, or after the puzzle is solved).
    public string GetInteractionPrompt() => CanBeLit ? promptText : null;

    // ── Called by CandlePuzzle ────────────────────────────────────────────────

    public void Light() => SetLit(true, playSound: true);

    public void Extinguish(bool playSound = true)
    {
        if (!IsLit) return;
        SetLit(false, playSound);
    }

    private void SetLit(bool lit, bool playSound)
    {
        IsLit = lit;
        if (flame != null) flame.SetActive(lit);

        if (playSound && audioSource != null)
        {
            var clip = lit ? lightSound : extinguishSound;
            if (clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
