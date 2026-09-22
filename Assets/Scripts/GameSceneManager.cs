using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "Main_Menu";
    [SerializeField] private string controlsScene = "Controls_Scene";
    [SerializeField] private string tutorialScene = "TutorialLVL";
    [SerializeField] private string levelOneScene = "Amina_Level_1_ProgressionCheck";
    [SerializeField] private string tutorialEndScene = "EndSceneTutorial";


    public void LoadTutorial()
    {
        LoadScene(tutorialScene);
    }

    public void LoadLevelOne()
    {
        LoadScene(levelOneScene);
    }

    public void LoadControls()
    {
        LoadScene(controlsScene);
    }

    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }

    public void LoadTutorialEnd()
    {
        LoadScene(tutorialEndScene);
    }


    public void RestartCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void QuitGame()
    {
                Application.Quit();
    }

    private void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError(
                $"Scene '{sceneName}' could not be loaded " +
                "Check if it is added to the Scene List"
            );
        }
    }


}
