using System;
using UnityEngine;

/// <summary>
/// The "finisher". Appears on Sir Nightmare only while he's drowsy. The player grapples in to
/// it and presses Interact to sing him under — no attack button, just a traversal skill followed
/// by the interaction you already have.
///
/// Setup: this component sits on a parent object that SirNightmareFight enables/disables, with
/// two child colliders (see the setup notes). PlayerController finds it via GetComponentInParent.
/// </summary>
public class LullabyAnchor : MonoBehaviour, IInteractable
{
    /// <summary>Raised when the player interacts. SirNightmareFight listens to this.</summary>
    public event Action Sung;

    [SerializeField] private string prompt = "Sing Lullaby";

    public void Interact(GameObject interactor) => Sung?.Invoke();

    public string GetInteractionPrompt() => prompt;
}
