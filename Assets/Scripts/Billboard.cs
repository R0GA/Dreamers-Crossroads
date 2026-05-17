using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("Billboard: Main Camera not found in the scene.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Finds the exact direction the camera is facing
            Vector3 cameraForward = mainCamera.transform.forward;

            // Prevents upward tilting. Makes it a bit like that one paper mario game
            cameraForward.y = 0;

            // Apply the flattened rotation to the character sprite
            // .normalized ensures the vector maintains a standard length of 1
            transform.forward = cameraForward.normalized;
        }
    }
}