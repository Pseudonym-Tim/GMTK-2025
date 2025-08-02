using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : UIComponent
{
    [SerializeField]
    private Image destroyedHeaderImage;

    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    [SerializeField]
    private TextMeshProUGUI newHighscoreText;

    [System.Serializable]
    private class StatReport
    {
        public string textKey;
        public string placeholder;
        public TextMeshProUGUI statText;
        [HideInInspector]
        public int value;
        [HideInInspector]
        public string template;
    }

    [SerializeField]
    private StatReport[] statReports;

    [SerializeField]
    private float scaleDuration = 0.5f;

    [SerializeField]
    private float shakeDuration = 0.5f;

    [SerializeField]
    private float shakeMagnitude = 10f;

    [SerializeField]
    private float reportCountDuration = 1f;

    [SerializeField]
    private float scoreAddDuration = 0.5f;

    private const float OPTION_DARKEN_FACTOR = 0.75f;

    [System.Serializable]
    public class MenuOption
    {
        public Image backgroundImage;
        public TextMeshProUGUI optionText;
    }

    [SerializeField]
    private MenuOption[] menuOptions;

    private int selectedIndex;

    private Color[] originalPanelColors;

    private Color[] originalTextColors;

    private bool menuActive;

    private RectTransform headerRectTransform;
    private Vector3 headerInitialPosition;
    private int initialHighscore;
    private ScoreManager scoreManager;

    public void InitStats(int totalKills, int recursionCount, int nearMissCount, int wavesComplete, int stagesComplete)
    {
        statReports[0].value = totalKills;
        statReports[1].value = recursionCount;
        statReports[2].value = nearMissCount;
        statReports[3].value = wavesComplete;
        statReports[4].value = stagesComplete;
    }

    public override void SetupUI()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        headerRectTransform = destroyedHeaderImage.rectTransform;
        headerInitialPosition = headerRectTransform.localPosition;
        newHighscoreText.gameObject.SetActive(false);

        for(int i = 0; i < statReports.Length; i++)
        {
            StatReport report = statReports[i];
            report.template = TextHandler.GetText(report.textKey, "gameover_ui");
            report.statText.gameObject.SetActive(false);
        }

        CacheOriginalColors();

        selectedIndex = 0;

        for(int i = 0; i < menuOptions.Length; i++)
        {
            menuOptions[i].backgroundImage.gameObject.SetActive(false);
            menuOptions[i].optionText.gameObject.SetActive(false);
        }

        Show(false);
    }

    public override void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;
        initialHighscore = scoreManager.CurrentHighscore;
        SetCanvasInteractivity(showUI);

        menuActive = false;

        if(showUI)
        {
            StartCoroutine(PlayGameOverSequence());
        }
    }

    private IEnumerator PlayGameOverSequence()
    {
        headerRectTransform.localScale = Vector3.zero;
        yield return ScaleHeader();
        yield return ShakeHeader();

        int displayedFinalScore = scoreManager.CurrentScore;
        finalScoreText.text = displayedFinalScore.ToString();

        for(int i = 0; i < statReports.Length; i++)
        {
            StatReport report = statReports[i];
            report.statText.enabled = true;
            yield return CountUpStat(report, reportCountDuration);
            yield return CountUpwards(finalScoreText, displayedFinalScore, displayedFinalScore + report.value, scoreAddDuration);
            displayedFinalScore += report.value;
        }

        if(displayedFinalScore > initialHighscore)
        {
            newHighscoreText.enabled = true;
            StartCoroutine(FlashNewHighscore());
        }

        ActivateMenuOptions();
    }

    private void ActivateMenuOptions()
    {
        selectedIndex = 0;

        for(int i = 0; i < menuOptions.Length; i++)
        {
            menuOptions[i].backgroundImage.gameObject.SetActive(true);
            menuOptions[i].optionText.gameObject.SetActive(true);
        }

        HighlightOption(selectedIndex);
        menuActive = true;
    }

    private void Update()
    {
        if(!IsInteractable || !menuActive)
        {
            return;
        }

        if(InputManager.IsButtonPressed("NavigateDown") || InputManager.IsButtonPressed("NavigateUp"))
        {
            NavigateOption(-1);
        }
        else if(InputManager.IsButtonPressed("NavigateLeft") || InputManager.IsButtonPressed("NavigateRight"))
        {
            NavigateOption(1);
        }

        if(InputManager.IsButtonPressed("SelectOption"))
        {
            SelectOption(selectedIndex);
        }
    }

    private void CacheOriginalColors()
    {
        originalPanelColors = new Color[menuOptions.Length];
        originalTextColors = new Color[menuOptions.Length];

        for(int i = 0; i < menuOptions.Length; i++)
        {
            originalPanelColors[i] = menuOptions[i].backgroundImage.color;
            originalTextColors[i] = menuOptions[i].optionText.color;
        }
    }

    private void NavigateOption(int direction)
    {
        selectedIndex = (selectedIndex + direction + menuOptions.Length) % menuOptions.Length;
        HighlightOption(selectedIndex);
    }

    private void HighlightOption(int index)
    {
        for(int i = 0; i < menuOptions.Length; i++)
        {
            Image panelImage = menuOptions[i].backgroundImage;
            TextMeshProUGUI textMesh = menuOptions[i].optionText;

            if(i == index)
            {
                panelImage.color = originalPanelColors[i];
                textMesh.color = originalTextColors[i];
            }
            else
            {
                Color dimmedPanel = originalPanelColors[i];
                dimmedPanel.r *= OPTION_DARKEN_FACTOR;
                dimmedPanel.g *= OPTION_DARKEN_FACTOR;
                dimmedPanel.b *= OPTION_DARKEN_FACTOR;

                Color dimmedText = originalTextColors[i];
                dimmedText.r *= OPTION_DARKEN_FACTOR;
                dimmedText.g *= OPTION_DARKEN_FACTOR;
                dimmedText.b *= OPTION_DARKEN_FACTOR;

                panelImage.color = dimmedPanel;
                textMesh.color = dimmedText;
            }
        }
    }

    private void SelectOption(int optionIndex)
    {
        switch(optionIndex)
        {
            case 0:
                GameManager.StartGame();
                break;
            case 1:
                SceneManager.LoadScene(0);
                break;
        }
    }

    private IEnumerator ScaleHeader()
    {
        float elapsed = 0f;

        while(elapsed < scaleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            headerRectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        headerRectTransform.localScale = Vector3.one;
    }

    private IEnumerator ShakeHeader()
    {
        float elapsed = 0f;

        while(elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float offsetX = (Random.value * 2f - 1f) * shakeMagnitude;
            float offsetY = (Random.value * 2f - 1f) * shakeMagnitude;
            headerRectTransform.localPosition = headerInitialPosition + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        headerRectTransform.localPosition = headerInitialPosition;
    }

    private IEnumerator CountUpStat(StatReport report, float duration)
    {
        int startValue = 0;
        int endValue = report.value;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            string filledIn = report.template.Replace($"%{report.placeholder}%", currentValue.ToString());
            report.statText.text = filledIn;
            yield return null;
        }

        report.statText.text = report.template.Replace($"%{report.placeholder}%", endValue.ToString());
    }

    private IEnumerator CountUpwards(TextMeshProUGUI textField, int startValue, int endValue, float duration)
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            textField.text = currentValue.ToString();
            yield return null;
        }

        textField.text = endValue.ToString();
    }

    private IEnumerator FlashNewHighscore()
    {
        while(true)
        {
            newHighscoreText.enabled = !newHighscoreText.enabled;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}