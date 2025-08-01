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

    private void Awake()
    {
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    public void Setup()
    {
        currentStage = 1;
        SetupStage();
    }

    private void SetupStage()
    {
        currentWave = 0;
        lastWaveCount = initialEnemyCount;

        playerHUD.UpdateCurrentStage(currentStage);
        playerHUD.UpdateCurrentWave(currentWave, wavesPerStage);

        StartNextWave();
    }

    private void StartNextWave()
    {
        currentWave++;

        playerHUD.UpdateCurrentWave(currentWave, wavesPerStage);
        playerHUD.UpdateCurrentStage(currentStage);

        if(currentWave > wavesPerStage)
        {
            OpenShop();
            currentStage++;
            SetupStage();
            return;
        }

        // Every shop wave, open the shop and fuck off to wait for player choice...
        if(currentWave % shopEveryWave == 0)
        {
            OpenShop();
            return;
        }

        StartCoroutine(WaveCoroutine());
    }

    // TODO: Call after the player has bought upgrades and whatnot...
    public void OnShopSelectionComplete()
    {
        StartCoroutine(WaveCoroutine());
    }

    private IEnumerator WaveCoroutine()
    {
        // TODO: Wave incoming warning...
        Debug.Log("Wave " + currentWave + " incoming!");
        yield return new WaitForSeconds(waveStartDelay);

        int spawnCount = (currentWave == 1) ? lastWaveCount : (lastWaveCount = lastWaveCount + Random.Range(extraEnemiesMin, extraEnemiesMax + 1));

        int asteroidCount = Random.Range(minAsteroids, maxAsteroids + 1);

        StartCoroutine(SpawnAsteroids(asteroidCount));
        yield return StartCoroutine(SpawnEnemies(spawnCount));

        // Wait until all enemies are dead and gone before starting next wave...
        // TODO: Might want to check if all are not alive instead, so currentHealth <= 0 or something...
        yield return new WaitUntil(() => levelManager.GetEntities<Enemy>().Count == 0);

        StartNextWave();
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

    private void OpenShop()
    {
        // TODO: Open shop...
    }
}
