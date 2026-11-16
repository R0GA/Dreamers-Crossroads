using UnityEngine;
using System.Collections;

public class MenuCameraController : MonoBehaviour
{
    [Header("Camera Views")]
    [SerializeField] private Transform mainMenuView;
    [SerializeField] private Transform controlsView;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 1f;

    private Coroutine moveRoutine;

    public void ShowControls()
    {
        MoveToView(controlsView);
    }

    public void ShowMainMenu()
    {
        MoveToView(mainMenuView);
    }

    private void MoveToView(Transform target)
    {
        if (target == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveCamera(target));
    }

    private IEnumerator MoveCamera(Transform target)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / moveDuration);

            // Makes the movement ease in and out instead of moving mechanically.
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(startPosition, targetPosition, t);

            transform.rotation =
                Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        moveRoutine = null;
    }
}
