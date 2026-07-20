using SmnStyleHardline.Demo;
using UnityEngine;

public class ExitScript : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }
    public void ExitGame()
    {
           Application.Quit();
    }
}
