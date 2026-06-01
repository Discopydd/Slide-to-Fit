using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string difficultySceneName = "DifficultySelectScene";

    public void PlayGame()
    {
        Invoke("LoadDifficultyScene", 0.2f);
    }

    private void LoadDifficultyScene()
    {
        SceneManager.LoadScene(difficultySceneName);
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