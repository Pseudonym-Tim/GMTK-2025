using UnityEngine;

/// <summary>
/// Base enemy class...
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : Entity, IScreenWrappable
{
    public int maxHealth = 3;
    [SerializeField] private int pointsToAward = 300;

    [Header("Screenwrap")]
    public Vector2 wrapBoundsSize = new Vector2(1f, 1f);
    public Vector2 wrapBoundsOffset = Vector2.zero;

    protected int currentHealth = 0;
    protected Rigidbody2D enemyRigidbody2D;
    protected LevelManager levelManager;
    protected Player playerEntity;
    private BulletProjectile lastDamageSource;

    protected override void OnEntityAwake()
    {
        enemyRigidbody2D = GetComponent<Rigidbody2D>();
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    public override void OnEntitySpawn()
    {
        playerEntity = levelManager.GetEntity<Player>();
        currentHealth = maxHealth;
        ScreenwrapsUsed = 0;
        ScreenwrapManager.Register(this);
    }

    protected override void OnEntityUpdate()
    {
        if(playerEntity == null)
        {
            playerEntity = levelManager.GetEntity<Player>();
        }
    }

    protected override void OnEntityCollision2D(Collision2D collision2D)
    {
        Enemy otherEnemy = collision2D.gameObject.GetComponent<Enemy>();

        if(otherEnemy != null && otherEnemy != this)
        {
            ScreenwrapManager.Unregister(this);
            DestroyEntity();
        }
    }

    public void OnScreenwrap()
    {

    }

    public virtual void TakeDamage(int damageToTake = 1, BulletProjectile bulletProjectile = null)
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

    protected virtual void OnDeath()
    {
        CheckAwardPoints();
        ScreenwrapManager.Unregister(this);
        DestroyEntity();
    }

    private void CheckAwardPoints()
    {
        if(lastDamageSource == null) { return; }

        if(lastDamageSource != null && lastDamageSource.Owner != BulletProjectile.BulletOwner.PLAYER)
        {
            return;
        }

        int multiplier = lastDamageSource.ScreenwrapsUsed + 1;

        int totalPoints = pointsToAward * multiplier;
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        scoreManager?.AddScore(totalPoints);

        SpawnPointsTextPopup(totalPoints);
    }

    private void SpawnPointsTextPopup(int pointsToGive)
    {
        PlayerHUD playerHUD = UIManager.GetUIComponent<PlayerHUD>();

        string pointsMessage = TextHandler.GetText("pointsAwardText", "player_hud");
        pointsMessage = pointsMessage.Replace("%pointAmount%", pointsToGive.ToString("N0"));

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
    public int MaxScreenwraps { get => -1; }
    public int ScreenwrapsUsed { get; set; }
}
