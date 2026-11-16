using UnityEngine;

public class ViewmodelAnimationRelay : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    public void OnInteractAnimationEnd()
    {
        playerController.OnInteractAnimationEnd();
    }
}