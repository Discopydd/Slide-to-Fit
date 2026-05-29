using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    public string titleSceneName = "TitleScene";
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
        Invoke("LoadTitleScene", 0.2f);
    }

    private void LoadTitleScene()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}