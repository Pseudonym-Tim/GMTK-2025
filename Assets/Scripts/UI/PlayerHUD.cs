using TMPro;
using UnityEngine;

/// <summary>
/// Handles everything related to the player's HUD...
/// </summary>
public class PlayerHUD : UIComponent
{
    [SerializeField] private TextMeshProUGUI currentHPText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI currentWaveText;
    [SerializeField] private TextMeshProUGUI currentStageText;

    public override void SetupUI()
    {
        
    }

    public override void Show(bool showUI = true)
    {

    }

    public void CreatePopupText(Vector2 spawnPos, string message, float scale = 1.0f)
    {
        PopupTextUI.Create(spawnPos, message, UICanvas.transform, scale);
    }

    public void UpdateCurrentScore(int currentScore)
    {
        string scoreMessage = TextHandler.GetText("currentScoreText", "player_hud");
        scoreMessage = scoreMessage.Replace("%currentScore%", currentScore.ToString("N0"));
        currentScoreText.text = scoreMessage;
    }

    public void UpdatePlayerHP(int currentHealth)
    {
        string healthMessage = TextHandler.GetText("currentHealthText", "player_hud");
        healthMessage = healthMessage.Replace("%healthAmount%", currentHealth.ToString("N0"));
        currentHPText.text = healthMessage;
    }

    public void UpdateHighscoreText(int currentHighscore)
    {
        string highscoreMessage = TextHandler.GetText("highscoreText", "player_hud");
        highscoreMessage = highscoreMessage.Replace("%highscore%", currentHighscore.ToString("N0"));
        highscoreText.text = highscoreMessage;
    }

    public void UpdateCurrentWave(int currentWave, int maxWave)
    {
        string waveMessage = TextHandler.GetText("currentWaveText", "player_hud");
        waveMessage = waveMessage.Replace("%currentWave%", currentWave.ToString("N0"));
        waveMessage = waveMessage.Replace("%maxWave%", maxWave.ToString("N0"));
        currentWaveText.text = waveMessage;
    }

    public void UpdateCurrentStage(int currentStage)
    {
        string stageMessage = TextHandler.GetText("currentStageText", "player_hud");
        stageMessage = stageMessage.Replace("%currentStage%", currentStage.ToString("N0"));
        currentStageText.text = stageMessage;
    }
}
