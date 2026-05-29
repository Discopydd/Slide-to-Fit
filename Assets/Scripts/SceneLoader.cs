using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string levelSelectSceneName = "LevelSelectScene";

    public void PlayGame()
    {
        Invoke("LoadLevelSelectScene", 0.2f);
    }

    private void LoadLevelSelectScene()
    {
        SceneManager.LoadScene(levelSelectSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Invoke("ActualQuit", 0.2f);
    }
    private void ActualQuit()
    {
        Application.Quit();
    }
}