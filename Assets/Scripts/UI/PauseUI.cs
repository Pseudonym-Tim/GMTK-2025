using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;

/// <summary>
/// Handles everything related to the pause UI...
/// </summary>
public class PauseUI : UIComponent
{
    private const float BOB_FREQUENCY = 5.0f;
    private const float BOB_AMPLITUDE = 10.0f;

    private const float OPTION_DARKEN_FACTOR = 0.75f;

    public Image logoImage;
    [SerializeField] private MenuOption[] menuOptions;

    private int selectedIndex = 0;
    private Color[] originalPanelColors;
    private Color[] originalTextColors;
    private Vector3 initialLogoPosition;
    private FadeUI fadeUI;
    private GameOverUI gameOverUI;
    private ShopUI shopUI;

    [System.Serializable]
    public class MenuOption
    {
        public Image backgroundImage;
        public TextMeshProUGUI optionText;
    }

    public override void SetupUI()
    {
        gameOverUI = UIManager.GetUIComponent<GameOverUI>();
        shopUI = UIManager.GetUIComponent<ShopUI>();
        initialLogoPosition = logoImage.rectTransform.localPosition;
        CacheOriginalColors();
        HighlightOption(selectedIndex);
        Show(false);
    }

    public override void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;

        if(showUI)
        {
            GameManager.PauseGame();
            SetCanvasInteractivity(true);
            HighlightOption(selectedIndex);
        }
        else
        {
            GameManager.ResumeGame();
            SetCanvasInteractivity(false);
        }
    }

    private void Update()
    {
        // Prevent pause toggle if gameover or shop UI is active...
        if(gameOverUI.IsShown || shopUI.IsShown)
        {
            return;
        }

        if(InputManager.IsButtonPressed("Pause"))
        {
            SFXManager sfxManager = FindFirstObjectByType<SFXManager>();
            bool newState = !UICanvas.enabled;

            if(newState)
            {
                sfxManager.Play2DSound("menu_accept");
            }
            else
            {
                sfxManager.Play2DSound("menu_back");
            }

            Show(newState);
        }

        if(!IsInteractable) { return; }

        if(InputManager.IsButtonPressed("NavigateDown") || InputManager.IsButtonPressed("NavigateLeft"))
        {
            NavigateOption(1);
        }
        else if(InputManager.IsButtonPressed("NavigateUp") || InputManager.IsButtonPressed("NavigateRight"))
        {
            NavigateOption(-1);
        }

        if(InputManager.IsButtonPressed("SelectOption"))
        {
            SelectOption(selectedIndex);
            SFXManager sfxManager = FindFirstObjectByType<SFXManager>();
            sfxManager.Play2DSound("menu_accept");
        }
    }

    private void LateUpdate() => UpdateLogoBob();

    private void UpdateLogoBob()
    {
        Vector3 bobbingPosition = initialLogoPosition;
        bobbingPosition.y += Mathf.Sin(Time.time * BOB_FREQUENCY) * BOB_AMPLITUDE;
        logoImage.rectTransform.localPosition = bobbingPosition;
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
            Image panelImage = menuOptions[i].backgroundImage.GetComponent<Image>();
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

    public void SelectOption(int optionIndex)
    {
        switch(optionIndex)
        {
            case 0: // Resume...
                Show(false);
                break;
            case 1: // Quit to main menu...
                BeginFade();
                break;
        }
    }

    private void BeginFade()
    {
        FadeUI.OnFadeOutComplete += OnFadeOutComplete;
        fadeUI.FadeOut();
    }

    private void OnFadeOutComplete()
    {
        FadeUI.OnFadeOutComplete -= OnFadeOutComplete;

        switch(selectedIndex)
        {
            case 1: // Quit to main menu...
                SceneManager.LoadScene(0);
                break;
        }
    }
}
