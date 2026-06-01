using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    public string difficultySceneName = "DifficultySelectScene";
    public string gameSceneName = "MainScene";

    public void SelectLevel(int levelIndex)
    {
        GameSession.SelectedLevelIndex = levelIndex;
        Invoke("LoadGameScene", 0.2f);
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToTitle()
    {
        Invoke("LoadDifficultyScene", 0.2f);
    }

    private void LoadDifficultyScene()
    {
        SceneManager.LoadScene(difficultySceneName);
    }
}