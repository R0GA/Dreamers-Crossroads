using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalController : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("The name of the scene to load when the player enters the portal.")]
    public string nextSceneName;

    [Tooltip("How long it takes for the portal to grow to full size.")]
    public float growDuration = 1.0f;

    [Tooltip("Controls the scaling animation. Try an Ease In Out curve.")]
    public AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalScale;
    private Collider portalCollider;
    private bool isRevealed = false;

    void Start()
    {
        // 1. Save the custom scale you set in the editor
        originalScale = transform.localScale;

        // 2. Set scale to zero so it is completely hidden
        transform.localScale = Vector3.zero;

        // 3. Disable the collider so the player can't trigger an invisible portal
        portalCollider = GetComponent<Collider>();
        if (portalCollider != null)
        {
            portalCollider.enabled = false;
            portalCollider.isTrigger = true; // Ensure it's treated as a trigger
        }
    }

    // Call this public method from your puzzle-solving script
    public void RevealPortal()
    {
        if (!isRevealed)
        {
            isRevealed = true;
            StartCoroutine(GrowPortalRoutine());
        }
    }

    private IEnumerator GrowPortalRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < growDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate how far along the animation is (0.0 to 1.0)
            float t = elapsedTime / growDuration;

            // Evaluate the animation curve for smoother scaling
            float curveValue = growCurve.Evaluate(t);

            // Use LerpUnclamped so the curve can "overshoot" for a bounce effect if desired
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, originalScale, curveValue);

            yield return null; // Wait for the next frame
        }

        // Snap exactly to the final scale to prevent any floating point inaccuracies
        transform.localScale = originalScale;

        // Enable the trigger now that the portal is visible
        if (portalCollider != null)
        {
            portalCollider.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the portal is the player
        if (other.CompareTag("Player") && isRevealed)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}