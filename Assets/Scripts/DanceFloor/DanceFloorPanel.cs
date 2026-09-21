using UnityEngine;

/// <summary>
/// A single light-up dance floor tile. Stepping on it advances its material to the next
/// color in the shared DanceFloorPalette. A panel doesn't know or care about the rest of
/// the puzzle — it just reports state changes to its DanceFloorPuzzleManager, which decides
/// when the puzzle is solved. Requires a trigger Collider sized to the panel's walkable
/// footprint (CharacterController still sends OnTrigger callbacks like a normal collider).
/// </summary>
[RequireComponent(typeof(Collider))]
public class DanceFloorPanel : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Renderer whose material gets swapped. Defaults to the Renderer on this object if left empty.")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("Shared palette this panel cycles through. Every panel in a puzzle should point at the same asset.")]
    [SerializeField] private DanceFloorPalette palette;

    [Header("Colors")]
    [Tooltip("Index into the palette this panel starts on.")]
    [SerializeField] private int startingColorIndex = 0;

    [Tooltip("Index into the palette this panel must be showing for the puzzle to count it as solved. " +
             "Set this per-panel — panels don't all need the same target color.")]
    [SerializeField] private int targetColorIndex = 0;

    [Header("Trigger")]
    [Tooltip("Only colliders with this tag advance the panel. Should be the tag on the player's root/CharacterController object.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Minimum time between color changes, so a single footstep can't double-trigger if the " +
             "player's collider clips the trigger boundary or the player lingers on the edge.")]
    [SerializeField] private float retriggerCooldown = 0.35f;

    public int CurrentColorIndex { get; private set; }
    public bool IsSolved => CurrentColorIndex == targetColorIndex;

    private DanceFloorPuzzleManager manager;
    private float cooldownTimer;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        var col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning($"{name}: DanceFloorPanel's Collider should be set 'Is Trigger' — panels are walked over, not collided with.", this);

        CurrentColorIndex = startingColorIndex;
        ApplyMaterial();
    }

    /// <summary>Called once by the DanceFloorPuzzleManager that owns this panel.</summary>
    public void Initialize(DanceFloorPuzzleManager owningManager)
    {
        manager = owningManager;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cooldownTimer > 0f) return;
        if (!other.CompareTag(playerTag)) return;

        AdvanceColor();
    }

    private void AdvanceColor()
    {
        cooldownTimer = retriggerCooldown;
        CurrentColorIndex = (CurrentColorIndex + 1) % palette.ColorCount;
        ApplyMaterial();
        manager?.NotifyPanelChanged(this);
    }

    private void ApplyMaterial()
    {
        // sharedMaterial, not material — we're swapping to a premade material asset wholesale,
        // not tweaking per-instance properties, and this avoids Unity silently instantiating
        // a duplicate material per panel every time we touch .material.
        targetRenderer.sharedMaterial = palette.GetMaterial(CurrentColorIndex);
    }

    /// <summary>Resets the panel to its starting color, e.g. when the puzzle is reset or replayed.</summary>
    public void ResetPanel()
    {
        cooldownTimer = 0f;
        CurrentColorIndex = startingColorIndex;
        ApplyMaterial();
    }

    /// <summary>
    /// Enables/disables this panel's trigger collider so the player can no longer change its
    /// color. Used by DanceFloorPuzzleManager's lockOnSolve — disabling the collider (rather
    /// than just ignoring OnTriggerEnter) means the player can freely walk over a locked panel
    /// without even a wasted OnTriggerEnter call.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        GetComponent<Collider>().enabled = interactable;
    }
}