using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelectController : MonoBehaviour
{
    [Header("Scene Names")]
    public string easyLevelSelectSceneName = "LevelSelectScene"; 
    public string normalLevelSelectSceneName = "NormalLevelSelectScene"; 
    public string hardLevelSelectSceneName = "HardLevelSelectScene";     
    public string titleSceneName = "TitleScene";

    public void SelectDifficulty(int difficulty)
    {
        GameSession.Difficulty = difficulty;
        Invoke("LoadLevelSelectScene", 0.2f);
    }

    private void LoadLevelSelectScene()
    {
        if (GameSession.Difficulty == 0)
        {
            SceneManager.LoadScene(easyLevelSelectSceneName);
        }
        else if (GameSession.Difficulty == 1)
        {
            SceneManager.LoadScene(normalLevelSelectSceneName);
        }
        else if (GameSession.Difficulty == 2)
        {
            SceneManager.LoadScene(hardLevelSelectSceneName);
        }
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
