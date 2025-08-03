using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Player spaceship entity...
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity, IScreenWrappable
{
    public int maxHealth = 3; 
    [SerializeField] private float invulnerabilityDuration = 1f;

    [Header("Movement")]
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float moveAcceleration = 10f;
    [SerializeField] private float moveDeceleration = 5f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Shooting")]
    [SerializeField] private Transform shotPointLeft;
    [SerializeField] private Transform shotPointRight;
    [SerializeField] private Transform shootPointMain;
    [SerializeField] private float shootCooldown = 0.25f; 
    [SerializeField] private int bulletWrapUpgradeCount = 0;

    [Header("Screenwrap")]
    [SerializeField] private Vector2 wrapBoundsSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 wrapBoundsOffset = Vector2.zero;

    private Rigidbody2D playerRigidbody2D;
    private LevelManager levelManager;
    private PlayerHUD playerHUD;
    private WeaponUpgrade currentWeaponUpgrade = null;
    private float targetRotation;
    private float shootTimer = 0f;
    private int currentHealth = 0;
    private bool isInvulnerable = false;
    private SFXManager sfxManager;

    protected override void OnEntityAwake()
    {
        sfxManager = FindFirstObjectByType<SFXManager>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        levelManager = FindFirstObjectByType<LevelManager>();
        playerRigidbody2D = GetComponent<Rigidbody2D>();
        PlayerCamera = GetComponentInChildren<PlayerCamera>();
        PlayerStatistics = GetComponent<PlayerStatistics>();
    }

    public override void OnEntitySpawn()
    {
        currentHealth = maxHealth;
        playerHUD.UpdatePlayerHP(currentHealth);
        ScreenwrapsUsed = 0;
        ScreenwrapManager.Register(this);
        PlayerCamera?.Setup();
        PlayerInput.InputEnabled = true;
    }

    public void RestoreHealthFull()
    {
        currentHealth = maxHealth;
        playerHUD.UpdatePlayerHP(currentHealth);
    }

    protected override void OnEntityUpdate()
    {
        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Tab) && Input.GetKeyDown(KeyCode.F12))
        {
            isInvulnerable = !isInvulnerable;
        }

        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Tab) && Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(9999);
        }

        if(!PlayerInput.InputEnabled)
        {
            return;
        }

        UpdateShipRotation();
        UpdateShooting();
    }

    private void UpdateShipRotation()
    {
        float newTarget = 0f;

        if(currentWeaponUpgrade != null && currentWeaponUpgrade.TryGetRotationLock(this, out float lockedRot))
        {
            newTarget = lockedRot;
        }
        else
        {
            float rotInput = 0f;

            if(PlayerInput.IsButtonHeld("TurnLeft"))
            {
                rotInput = 1f;
            }

            if(PlayerInput.IsButtonHeld("TurnRight"))
            {
                rotInput = -1f;
            }

            newTarget = playerRigidbody2D.rotation + rotInput * rotateSpeed;
        }

        targetRotation = newTarget;
    }

    private void FixedUpdate()
    {
        // Smoothly rotate ship...
        playerRigidbody2D.MoveRotation(targetRotation);

        // Apply velocity...
        Vector2 currentVelocity = playerRigidbody2D.linearVelocity;
        bool isThrusting = PlayerInput.IsButtonHeld("Thrust");
        Vector2 desiredVelocity = isThrusting ? transform.up * moveSpeed : Vector2.zero;
        float rate = isThrusting ? moveAcceleration : moveDeceleration;
        Vector2 newVelocity = Vector2.MoveTowards(currentVelocity, desiredVelocity, rate * Time.fixedDeltaTime);
        playerRigidbody2D.linearVelocity = newVelocity;
    }

    public void TakeDamage(int damageToTake = 1)
    {
        if(isInvulnerable)
        {
            return;
        }

        currentHealth -= damageToTake;

        if(currentHealth < 0)
        {
            currentHealth = 0;
        }

        PlayerCamera.Shake(0.25f / 2, 0.025f);
        playerHUD.UpdatePlayerHP(currentHealth);

        if(currentHealth <= 0)
        {
            OnDeath();
            return;
        }

        EntityHurtFlash.Setup(this);
        StartCoroutine(InvulnerabilityCoroutine());
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    private void UpdateShooting()
    {
        shootTimer -= Time.deltaTime;

        if(PlayerInput.IsButtonHeld("Shoot") && shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
            PlayerCamera.Shake(0.25f / 2, 0.025f);
        }
    }

    private void Shoot()
    {
        if(currentWeaponUpgrade != null)
        {
            currentWeaponUpgrade.Shoot(this);
            return;
        }

        ShootStandard();
    }

    private void ShootStandard()
    {
        BulletProjectile bulletProjectile = (BulletProjectile)levelManager.SpawnEntity("bullet_projectile", shootPointMain.position, shootPointMain.rotation);
        bulletProjectile.Setup(BulletProjectile.BulletOwner.PLAYER, shootPointMain.up);
        sfxManager.Play2DSound("basic_shoot");
    }

    public Transform ShootPoint
    {
        get
        {
            return shootPointMain;
        }
    }

    public Transform ShotPointLeft
    {
        get
        {
            return shotPointLeft;
        }
    }

    public Transform ShotPointRight
    {
        get
        {
            return shotPointRight;
        }
    }

    public void OnDeath()
    {
        EnemyWaveManager enemyWaveManager = FindFirstObjectByType<EnemyWaveManager>();
        enemyWaveManager.StopWaveLogic();
        OnTriggerGameOver();

        PlayerCamera.Shake(1f, 0.25f);

        SpawnExplosion();

        ScreenwrapManager.Unregister(this);
        DestroyEntity();
    }

    private void SpawnExplosion()
    {
        GameObject explosionPrefab = (GameObject)Resources.Load("Prefabs/Explosion");
        Quaternion randomRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        Instantiate(explosionPrefab, CenterOfMass, randomRot);
        PlayerCamera.Shake(1f, 0.5f / 2);
        sfxManager.Play2DSound("explosion");
    }

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    public void EquipWeaponUpgrade(WeaponUpgrade upgrade)
    {
        currentWeaponUpgrade = upgrade;
    }

    public void OnScreenwrap()
    {

    }

    public void OnTriggerGameOver()
    {
        GameOverUI gameOverUI = UIManager.GetUIComponent<GameOverUI>();

        int totalKills = PlayerStatistics.TotalKills;
        int recursionCount = PlayerStatistics.RecursionCount;
        int nearMissCount = PlayerStatistics.NearMissCount;
        int wavesComplete = PlayerStatistics.WavesComplete;
        int stagesComplete = PlayerStatistics.StagesComplete;

        gameOverUI.InitStats(totalKills, recursionCount, nearMissCount, wavesComplete, stagesComplete);

        gameOverUI.Show(true);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 pos = EntityPosition + ScreenwrapBoundsOffset;
        Vector3 center3D = new Vector3(pos.x, pos.y, 0f);
        Vector3 size3D = new Vector3(ScreenwrapBoundsSize.x, ScreenwrapBoundsSize.y, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center3D, size3D);
    }

    public int BulletWrapUpgradeCount
    {
        get { return bulletWrapUpgradeCount; }
        set { bulletWrapUpgradeCount = value; }
    }

    public LevelManager LevelManager => levelManager;

    public PlayerStatistics PlayerStatistics { get; set; } = null;
    public Vector2 ScreenwrapBoundsSize { get => wrapBoundsSize; }
    public Vector2 ScreenwrapBoundsOffset { get => wrapBoundsOffset; }
    public int MaxScreenwraps { get => -1; }
    public int ScreenwrapsUsed { get; set; }

    public PlayerCamera PlayerCamera { get; private set; } = null;

    public bool HasHomingBullets { get; set; } = false;
    public float HomingStrength { get; set; } = 0f;
}
