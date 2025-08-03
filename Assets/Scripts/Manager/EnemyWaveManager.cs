using System.Collections;
using UnityEngine;

/// <summary>
/// Handles everything to do with the wave system...
/// </summary>
public class EnemyWaveManager : Singleton<EnemyWaveManager>
{
    [Header("Wave Settings")]
    [SerializeField] private int wavesPerStage = 10;
    [SerializeField] private int initialEnemyCount = 5;
    [SerializeField] private int extraEnemiesMin = 1;
    [SerializeField] private int extraEnemiesMax = 3;
    [SerializeField] private int shopEveryWave = 5;

    [Header("Spawn Settings")]
    [SerializeField] private string[] enemyEntityIDList;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private int minAsteroids = 1;
    [SerializeField] private int maxAsteroids = 3;
    [SerializeField] private float waveStartDelay = 3f;

    private int currentStage;
    private int currentWave;
    private int lastWaveCount;
    private LevelManager levelManager;
    private PlayerHUD playerHUD;
    private ShopManager shopManager; 
    private bool shopShownThisWave = false;
    private FadeUI fadeUI;
    private bool isGameOver = false;

    private void Awake()
    {
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        fadeUI = UIManager.GetUIComponent<FadeUI>();
        shopManager = FindFirstObjectByType<ShopManager>();
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    public void StopWaveLogic()
    {
        isGameOver = true;
        StopAllCoroutines();
    }

    public void Setup()
    {
        isGameOver = false;
        currentStage = 1;

        Player player = levelManager.GetEntity<Player>();

        if(player != null)
        {
            player.PlayerStatistics.RegisterStageComplete(currentStage);
        }

        SetupStage();
    }

    private void StartNextWave()
    {
        if(isGameOver)
        {
            return;
        }

        currentWave++;
        playerHUD.UpdateCurrentStage(currentStage);
        playerHUD.UpdateCurrentWave(currentWave, wavesPerStage);
        StartCoroutine(WaveCoroutine());
    }

    private void SetupStage()
    {
        StartCoroutine(SpawnAsteroids(minAsteroids));

        currentWave = 0;
        lastWaveCount = initialEnemyCount;

        ResetPlayerEntity();

        playerHUD.UpdateCurrentStage(currentStage);
        playerHUD.UpdateCurrentWave(currentWave, wavesPerStage);

        StartNextWave();
    }

    private void ResetPlayerEntity()
    {
        Player player = levelManager.GetEntity<Player>();

        if(player != null)
        {
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            player.Teleport(Vector2.zero);
        }
    }

    public void OnShopSelectionComplete()
    {
        if(currentWave >= wavesPerStage)
        {
            currentStage++;

            Player player = levelManager.GetEntity<Player>();

            if(player != null)
            {
                player.PlayerStatistics.RegisterStageComplete(currentStage);
            }

            SetupStage();
        }
        else
        {
            StartNextWave();
        }
    }

    private IEnumerator WaveCoroutine()
    {
        if(isGameOver)
        {
            yield break;
        }

        // If this is the first wave of the stage, show stage indication before anything else...
        if(currentWave == 1)
        {
            yield return StartCoroutine(playerHUD.ShowStageStartAnnouncement(currentStage));
        }

        yield return new WaitForSeconds(1f);

        bool shopAfterWave = (currentWave % shopEveryWave == 0);
        yield return StartCoroutine(playerHUD.ShowWaveIncomingWarning(shopAfterWave));

        yield return new WaitForSeconds(waveStartDelay);

        int spawnCount = (currentWave == 1) ? lastWaveCount : (lastWaveCount = lastWaveCount + Random.Range(extraEnemiesMin, extraEnemiesMax + 1));

        int asteroidCount = Random.Range(minAsteroids, maxAsteroids + 1);

        StartCoroutine(SpawnAsteroids(asteroidCount));
        yield return StartCoroutine(SpawnEnemies(spawnCount));

        // Wait until all enemies are dead and gone before starting next wave...
        // TODO: Might want to check if all are not alive instead, so currentHealth <= 0 or something...
        yield return new WaitUntil(() => levelManager.GetEntities<Enemy>().Count == 0);

        if(isGameOver)
        {
            yield break;
        }

        // Open shop only after completing every specific wave...
        if(currentWave % shopEveryWave == 0)
        {
            yield return fadeUI.FadeOut();
            shopManager.OpenShop();
        }
        else
        {
            StartNextWave();
        }

        Player player = levelManager.GetEntity<Player>();

        if(player != null)
        {
            player.PlayerStatistics.RegisterWaveComplete(currentWave);
        }
    }

    private IEnumerator SpawnEnemies(int count)
    {
        for(int i = 0; i < count; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        int index = Random.Range(0, enemyEntityIDList.Length);
        string enemyID = enemyEntityIDList[index];
        Vector3 spawnPosition = GetOffScreenSpawnPosition();
        levelManager.SpawnEntity(enemyID, spawnPosition);
    }

    private IEnumerator SpawnAsteroids(int count)
    {
        for(int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetOffScreenSpawnPosition();
            levelManager.SpawnEntity("asteroid", spawnPosition);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector3 GetOffScreenSpawnPosition()
    {
        Camera cam = Camera.main;
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        Vector3 center = cam.transform.position;
        float xPos = Random.Range(center.x - width / 2f, center.x + width / 2f);
        float yPos = Random.Range(center.y - height / 2f, center.y + height / 2f);
        int side = Random.Range(0, 4);

        if(side == 0)
        {
            return new Vector3(center.x - width / 2f - 1f, yPos, 0f);
        }
        else if(side == 1)
        {
            return new Vector3(center.x + width / 2f + 1f, yPos, 0f);
        }
        else if(side == 2)
        {
            return new Vector3(xPos, center.y - height / 2f - 1f, 0f);
        }
        else
        {
            return new Vector3(xPos, center.y + height / 2f + 1f, 0f);
        }
    }
}
