using System.Collections;
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

    [SerializeField] private CanvasGroup warningBackgroundCanvasGroup;
    [SerializeField] private TextMeshProUGUI warningWaveText;
    [SerializeField] private TextMeshProUGUI traderText;

    [SerializeField] private CanvasGroup stageAnnouncementCanvasGroup;
    [SerializeField] private TextMeshProUGUI stageAnnouncementText;

    public override void SetupUI()
    {
        warningWaveText.enabled = false;
        traderText.enabled = false;
        stageAnnouncementCanvasGroup.alpha = 0f;
        warningBackgroundCanvasGroup.alpha = 0f;
        stageAnnouncementCanvasGroup.alpha = 0f;
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

    public IEnumerator ShowStageStartAnnouncement(int stage)
    {
        string stageStartMessage = TextHandler.GetText("stageStartText", "player_hud");
        stageAnnouncementText.text = stageStartMessage.Replace("%stage%", stage.ToString("N0"));
        yield return new WaitForSeconds(1.5f);
        stageAnnouncementCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(2f);
        stageAnnouncementCanvasGroup.alpha = 0f;
    }

    public IEnumerator ShowWaveIncomingWarning(bool isShopAvailable)
    {
        const int FLASHES = 6;
        float flashDuration = 0.5f;

        SFXManager sfxManager = FindFirstObjectByType<SFXManager>();

        for(int i = 0; i < FLASHES; i++)
        {
            if(isShopAvailable)
            {
                traderText.enabled = true;
            }

            sfxManager.Play2DSound("wave_alarm");

            warningWaveText.enabled = true;

            yield return StartCoroutine(LerpAlpha(warningBackgroundCanvasGroup, 0f, 0.1f, flashDuration / 2));

            warningWaveText.enabled = false; 
            
            if(isShopAvailable)
            {
                traderText.enabled = false;
            }

            yield return StartCoroutine(LerpAlpha(warningBackgroundCanvasGroup, 0.1f, 0f, flashDuration / 2));
        }
    }

    private IEnumerator LerpAlpha(CanvasGroup canvasGroup, float start, float end, float duration)
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
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
