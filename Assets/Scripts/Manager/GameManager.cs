using UnityEngine;

/// <summary>
/// Handles core game related stuff and setup...
/// </summary>
public class GameManager : Singleton<GameManager>
{
    private const float GAME_TIMESCALE = 1.0f;
    public const string GAME_NAME = "GMTK 2025";

    public static GameState CurrentGameState { get; set; } = GameState.INACTIVE;
    public static float InGameTimer { get; private set; } = 0.0f;
    private static float previousTimeScale = 0;
    private static LevelManager levelManager;

    public enum GameState
    {
        INACTIVE,
        PLAYING,
        PAUSED,
        GAME_OVER
    }

    private void Awake()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        StartGame();
    }

    private void Update()
    {
        if(CurrentGameState == GameState.PLAYING)
        {
            InGameTimer += Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.F2))
        {
            ScreenshotHandler.TakeScreenshot();
        }
    }

    public static void StartGame()
    {
        InGameTimer = 0.0f;
        Time.timeScale = GAME_TIMESCALE;

        UIManager.SetupUI();

        levelManager.Setup();

        BeginPlaying();
    }

    public static void BeginPlaying()
    {
        CurrentGameState = GameState.PLAYING;
    }

    public static void PauseGame()
    {
        if(Time.timeScale != 0)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0;
            CurrentGameState = GameState.PAUSED;
        }
    }

    public static void ResumeGame()
    {
        if(previousTimeScale != 0 && Time.timeScale == 0)
        {
            Time.timeScale = previousTimeScale;
            previousTimeScale = 0;
            CurrentGameState = GameState.PLAYING;
        }
    }

    public static void GameOver()
    {
        CurrentGameState = GameState.GAME_OVER;
        Debug.Log("Game over!");
        // TODO: Gameover stuff goes here...
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

    public static void QuitToMainMenu()
    {
        Debug.Log("Quit to main menu!");
        // TODO: Load main menu scene...
    }

    public static string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(InGameTimer / 60F);
        int seconds = Mathf.FloorToInt(InGameTimer % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public static bool IsGameOver
    {
        get { return CurrentGameState == GameState.GAME_OVER; }
    }
}