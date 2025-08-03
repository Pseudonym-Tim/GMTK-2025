using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// A dangerous meteroid type obstacle...
/// </summary>
public class Asteroid : Entity, IScreenWrappable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int pointsToAward = 300;
    [SerializeField] private float minDriftSpeed = 0.5f;
    [SerializeField] private float maxDriftSpeed = 1.5f;
    [SerializeField] private float minRotationSpeed = 20f;
    [SerializeField] private float maxRotationSpeed = 60f;
    [SerializeField] private SpriteRenderer asteroidGFX;
    [SerializeField] private Sprite[] spriteList;

    [Header("Screenwrap")]
    [SerializeField] private Vector2 wrapBoundsSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 wrapBoundsOffset = Vector2.zero;
    [SerializeField] private int minWrapCount = 1;
    [SerializeField] private int maxWrapCount = 3;

    private Rigidbody2D asteroidRigidbody2D;
    private int maximumWrapCount = 0;
    private int currentHealth;
    private BulletProjectile lastDamageSource;
    private SFXManager sfxManager;

    protected override void OnEntityAwake()
    {
        sfxManager = FindFirstObjectByType<SFXManager>();
        asteroidRigidbody2D = GetComponent<Rigidbody2D>();
    }

    public override void OnEntitySpawn()
    {
        // Set maximum wrap...
        currentHealth = maxHealth;
        ScreenwrapsUsed = 0;
        maximumWrapCount = Random.Range(minWrapCount, maxWrapCount);
        ScreenwrapManager.Register(this);

        // Set GFX...
        int spriteIndex = Random.Range(0, spriteList.Length);
        asteroidGFX.sprite = spriteList[spriteIndex];

        // Set drift...
        Vector2 driftDirection = Random.insideUnitCircle.normalized;
        float driftSpeed = Random.Range(minDriftSpeed, maxDriftSpeed);
        asteroidRigidbody2D.linearVelocity = driftDirection * driftSpeed;

        // Set rotation speed...
        float rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        asteroidRigidbody2D.angularVelocity = rotationSpeed;
    }

    protected override void OnEntityCollision2D(Collision2D collision2D)
    {
        Collider2D collider2D = collision2D.contacts[0].collider;

        Player player = collision2D.gameObject.GetComponent<Player>();

        if(player != null)
        {
            player.TakeDamage(1);
            TakeDamage(1);

            Vector2 knockbackDir = (player.CenterOfMass - CenterOfMass).normalized;

            Rigidbody2D playerRigidbody2D = player.GetComponent<Rigidbody2D>();

            float knockbackForcePlayer = 5f;

            playerRigidbody2D.linearVelocity = knockbackDir * knockbackForcePlayer;
        }

        Enemy enemy = collision2D.gameObject.GetComponent<Enemy>();

        if(enemy != null && enemy != this)
        {
            enemy.TakeDamage(1);
            TakeDamage(1);

            Vector2 knockbackDir = (enemy.CenterOfMass - CenterOfMass).normalized;

            Rigidbody2D enemyRigidbody2D = enemy.GetComponent<Rigidbody2D>();

            float knockbackForceOtherEnemy = 5f;

            enemyRigidbody2D.linearVelocity = knockbackDir * knockbackForceOtherEnemy;
        }

        TakeDamage();
    }

    public void TakeDamage(int damageToTake = 1, BulletProjectile bulletProjectile = null)
    {
        lastDamageSource = bulletProjectile;
        currentHealth -= damageToTake;
        if(currentHealth < 0) { currentHealth = 0; }

        if(currentHealth <= 0)
        {
            OnDeath();
            return;
        }

        EntityHurtFlash.Setup(this);
    }

    private void OnDeath()
    {
        SpawnExplosion();

        CheckAwardPoints();
        ScreenwrapManager.Unregister(this);
        DestroyEntity();
    }

    private void SpawnExplosion()
    {
        GameObject explosionPrefab = (GameObject)Resources.Load("Prefabs/Explosion");
        Quaternion randomRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        Instantiate(explosionPrefab, CenterOfMass, randomRot);
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        Player playerEntity = levelManager.GetEntity<Player>();
        playerEntity?.PlayerCamera.Shake(1f, 0.5f / 2);
        sfxManager.Play2DSound("explosion");
    }

    public void OnScreenwrap()
    {
        
    }

    private void CheckAwardPoints()
    {
        if(lastDamageSource == null) { return; }

        if(lastDamageSource != null && lastDamageSource.Owner != BulletProjectile.BulletOwner.PLAYER)
        {
            return;
        }

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        scoreManager?.AddScore(pointsToAward);

        SpawnPointsTextPopup();
    }

    private void SpawnPointsTextPopup()
    {
        PlayerHUD playerHUD = UIManager.GetUIComponent<PlayerHUD>();

        string pointsMessage = TextHandler.GetText("pointsAwardText", "player_hud");
        pointsMessage = pointsMessage.Replace("%pointAmount%", pointsToAward.ToString("N0"));

        playerHUD.CreatePopupText(CenterOfMass, pointsMessage);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 pos = EntityPosition + ScreenwrapBoundsOffset;
        Vector3 center3D = new Vector3(pos.x, pos.y, 0f);
        Vector3 size3D = new Vector3(ScreenwrapBoundsSize.x, ScreenwrapBoundsSize.y, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center3D, size3D);
    }

    public Vector2 ScreenwrapBoundsSize { get => wrapBoundsSize; }
    public Vector2 ScreenwrapBoundsOffset { get => wrapBoundsOffset; }
    public int MaxScreenwraps { get => maximumWrapCount; }
    public int ScreenwrapsUsed { get; set; }
}
