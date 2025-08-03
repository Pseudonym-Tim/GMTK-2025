using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// Player spaceship entity...
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity, IScreenWrappable
{
    public int maxHealth = 3;

    [Header("Movement")]
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float moveAcceleration = 10f;
    [SerializeField] private float moveDeceleration = 5f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Shooting")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootCooldown = 0.25f;

    [Header("Screenwrap")]
    [SerializeField] private Vector2 wrapBoundsSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 wrapBoundsOffset = Vector2.zero;

    private Rigidbody2D playerRigidbody2D;
    private LevelManager levelManager;
    private PlayerHUD playerHUD;
    private float targetRotation;
    private float shootTimer = 0f;
    private int currentHealth = 0;
    private bool isInvulnerable = false;

    protected override void OnEntityAwake()
    {
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

    protected override void OnEntityUpdate()
    {
        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Tab) && Input.GetKeyDown(KeyCode.F12))
        {
            isInvulnerable = !isInvulnerable;
        }

        if(!PlayerInput.InputEnabled) { return; }

        // Turn ship...
        float rotationInput = 0f;
        if(PlayerInput.IsButtonHeld("TurnLeft")) { rotationInput = 1f; }
        if(PlayerInput.IsButtonHeld("TurnRight")) { rotationInput = -1f; }
        targetRotation = playerRigidbody2D.rotation + rotationInput * rotateSpeed;

        UpdateShooting();
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
        if(isInvulnerable) { return; }

        currentHealth -= damageToTake;
        if(currentHealth < 0) { currentHealth = 0; }

        playerHUD.UpdatePlayerHP(currentHealth);

        if(currentHealth <= 0)
        {
            OnDeath();
            return;
        }

        EntityHurtFlash.Setup(this);
    }

    private void UpdateShooting()
    {
        shootTimer -= Time.deltaTime;

        if(PlayerInput.IsButtonHeld("Shoot") && shootTimer <= 0f)
        {
            ShootBullet();
            shootTimer = shootCooldown;
        }
    }

    private void ShootBullet()
    {
        BulletProjectile bulletProjectile = (BulletProjectile)levelManager.SpawnEntity("bullet_projectile", shootPoint.position, shootPoint.rotation);
        bulletProjectile.Setup(BulletProjectile.BulletOwner.PLAYER, shootPoint.up);
    }

    public void OnDeath()
    {
        TriggerGameOver();
        ScreenwrapManager.Unregister(this);
        DestroyEntity();
    }

    public void OnScreenwrap()
    {

    }

    public void TriggerGameOver()
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

    public PlayerStatistics PlayerStatistics { get; set; } = null;
    public Vector2 ScreenwrapBoundsSize { get => wrapBoundsSize; }
    public Vector2 ScreenwrapBoundsOffset { get => wrapBoundsOffset; }
    public int MaxScreenwraps { get => -1; }
    public int ScreenwrapsUsed { get; set; }

    public PlayerCamera PlayerCamera { get; private set; } = null;
}
