using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    public string titleSceneName = "TitleScene";
    public string gameSceneName = "MainScene";

    public void SelectLevel(int levelIndex)
    {
        GameSession.SelectedLevelIndex = levelIndex;
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}