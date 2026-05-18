using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string levelSelectSceneName = "LevelSelectScene";

    public void PlayGame()
    {
        SceneManager.LoadScene(levelSelectSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}