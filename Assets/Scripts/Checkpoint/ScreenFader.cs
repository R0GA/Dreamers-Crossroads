using System.Collections;
using UnityEngine;

/// <summary>
/// Full-screen black fade. Put this on a UI object that stretches across your whole Canvas
/// (an Image set to black, anchored to stretch on all sides) and give it a high Canvas
/// sorting order so it draws over the HUD. It starts fully transparent and never blocks
/// clicks. PlayerRespawner drives it; anything else (death sequences, scene transitions)
/// can use it too.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    private CanvasGroup group;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    /// <summary>Fades to the given alpha (0 = clear, 1 = black). Yield this from a coroutine.
    /// Uses unscaled time so it still works if the game is paused or slowed.</summary>
    public IEnumerator Fade(float targetAlpha, float duration)
    {
        if (duration <= 0f)
        {
            group.alpha = targetAlpha;
            yield break;
        }

        float start = group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = targetAlpha;
    }
}
