using UnityEngine;

/// <summary>
/// Implement this on anything the player should be able to look at and interact with:
/// puzzle rings, levers, switches, pickups, NPCs, doors, etc.
/// PlayerController only ever talks to this interface, so adding new interactables
/// never requires touching player code.
/// </summary>
public interface IInteractable
{
    /// <summary>Primary interaction — bound to the "Interact" action (E by default).</summary>
    void Interact(GameObject interactor);

    /// <summary>
    /// Secondary interaction — bound to the "InteractAlt" action. Useful for things like
    /// rotating a ring the other way. Optional: default implementation does nothing, so
    /// most interactables (levers, pickups, doors) don't need to implement it at all.
    /// </summary>
    void InteractAlt(GameObject interactor) { }

    /// <summary>
    /// Optional prompt text for a UI element, e.g. "Rotate Ring" or "Pick Up".
    /// Return null or an empty string to opt this interactable OUT of any prompt UI
    /// (the default) — only override this if you actually want a popup to appear.
    /// </summary>
    string GetInteractionPrompt() => null;
}
