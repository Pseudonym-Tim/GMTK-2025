using UnityEngine;

/// <summary>
/// Handles everything related to the scoring...
/// </summary>
public class ScoreManager : Singleton<ScoreManager>
{
    private const int MAX_SCORE = 99999;

    private PlayerHUD playerHUD;

    public int CurrentScore { get; private set; } = 0;
    public int CurrentHighscore { get; private set; } = 0;

    public void Setup()
    {
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        CurrentScore = 0;
        playerHUD.UpdateCurrentScore(CurrentScore);
        LoadHighscore();
    }

    public void AddScore(int scoreToAdd)
    {
        CurrentScore += scoreToAdd;

        if(CurrentScore > MAX_SCORE)
        {
            CurrentScore = MAX_SCORE;
        }

        playerHUD.UpdateCurrentScore(CurrentScore);
        CheckUpdateHighscore();
    }

    private void CheckUpdateHighscore()
    {
        if(CurrentScore > CurrentHighscore)
        {
            CurrentHighscore = CurrentScore;
            PlayerPrefs.SetInt("highscore", CurrentHighscore);
            PlayerPrefs.Save();
            playerHUD.UpdateHighscoreText(CurrentHighscore);
        }
    }

    private void LoadHighscore()
    {
        CurrentHighscore = PlayerPrefs.GetInt("highscore", 0);
        playerHUD.UpdateHighscoreText(CurrentHighscore);
    }
}
