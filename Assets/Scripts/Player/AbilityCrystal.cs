using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A world object the player interacts with to permanently restore one traversal ability
/// (Jump, Grapple, or Launch). Drop one of these at each tutorial gate, set abilityToGrant
/// in the Inspector, and it wires itself into the existing IInteractable / prompt UI —
/// no other scripts need to know it exists.
///
/// Needs a Collider (trigger or solid, doesn't matter) on the interactionMask layer so
/// PlayerController's raycast can find it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AbilityCrystal : MonoBehaviour, IInteractable
{
    [Header("Ability")]
    [Tooltip("Which ability this crystal restores when the player interacts with it.")]
    [SerializeField] private PlayerAbility abilityToGrant = PlayerAbility.Jump;

    [Tooltip("Shown in the on-screen prompt, e.g. \"Restore Jump\". Purely cosmetic.")]
    [SerializeField] private string promptText = "Restore Jump";

    [Header("Behaviour")]
    [Tooltip("If true, the crystal can only be used once — the collider disables and it " +
             "stops offering a prompt after granting its ability. Turn off for a crystal " +
             "you want interactable repeatedly (e.g. a respawn/checkpoint).")]
    [SerializeField] private bool consumeOnUse = true;

    [Tooltip("Seconds to wait after use before destroying the GameObject, so VFX/SFX have " +
             "time to play out. Only relevant when consumeOnUse is true.")]
    [SerializeField] private float destroyDelay = 2f;

    [Header("Feedback (all optional)")]
    [SerializeField] private GameObject visualToHide;
    [SerializeField] private ParticleSystem activateVfx;
    [SerializeField] private AudioSource activateSfx;

    [Header("Events")]
    [Tooltip("Invoked once, the moment this crystal successfully grants its ability. Hook up VFX, quest updates, dialogue, etc.")]
    public UnityEvent onAbilityGranted;

    private bool used;

    public void Interact(GameObject interactor)
    {
        if (used) return;

        PlayerController player = interactor.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning($"{name}: interactor \"{interactor.name}\" has no PlayerController — ability not granted.", this);
            return;
        }

        used = true;
        player.UnlockAbility(abilityToGrant);

        if (activateVfx != null) activateVfx.Play();
        if (activateSfx != null) activateSfx.Play();
        onAbilityGranted?.Invoke();

        if (consumeOnUse)
        {
            if (visualToHide != null) visualToHide.SetActive(false);
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, destroyDelay);
        }
    }

    // No InteractAlt behaviour needed — default no-op from the interface is fine.

    public string GetInteractionPrompt() => used ? null : promptText;
}
