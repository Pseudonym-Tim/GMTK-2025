using UnityEngine;

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

    private Rigidbody2D playerRigidbody2D;
    private int maximumWrapCount = 0;
    private int currentHealth;
    private BulletProjectile lastDamageSource;

    protected override void OnEntityAwake()
    {
        playerRigidbody2D = GetComponent<Rigidbody2D>();
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
        playerRigidbody2D.linearVelocity = driftDirection * driftSpeed;

        // Set rotation speed...
        float rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        playerRigidbody2D.angularVelocity = rotationSpeed;
    }

    protected override void OnEntityCollision2D(Collision2D collision2D)
    {
        Collider2D collider2D = collision2D.contacts[0].collider;

        Player playerEntity = collider2D.GetComponentInParent<Player>();
        playerEntity?.TakeDamage();

        Enemy enemyEntity = collider2D.GetComponentInParent<Enemy>();
        enemyEntity?.TakeDamage();

        ScreenwrapManager.Unregister(this);
        DestroyEntity();
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
        CheckAwardPoints();
        ScreenwrapManager.Unregister(this);
        DestroyEntity();
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
