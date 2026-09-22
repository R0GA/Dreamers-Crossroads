using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Cache the camera reference to avoid expensive Camera.main calls every frame
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 lookPos = mainCamera.transform.position - transform.position;
        lookPos.y = 0; // Keep the billboard upright

        transform.rotation = Quaternion.LookRotation(lookPos);
    }
}