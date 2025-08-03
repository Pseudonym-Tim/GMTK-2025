using UnityEngine;

/// <summary>
/// A bullet projectile entity...
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BulletProjectile : Entity, IScreenWrappable
{
    private const float IGNORE_TIME = 0.25f;

    [Header("Projectile Settings")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private Vector2 hitBoxSize = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 hitBoxOffset = Vector2.zero;
    [SerializeField] private SpriteRenderer bulletGFX;

    [Header("Screenwrap")]
    [SerializeField] private Vector2 wrapBoundsSize = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 wrapBoundsOffset = Vector2.zero;
    [SerializeField] private int maxWraps = 3;

    private float ignoreTimer = 0f;
    private Rigidbody2D bulletRigidbody2D; 
    private bool hasRegisteredNearMiss = false;
    private float nearMissCooldown = 0.5f;
    private float nearMissTimer = 0f;

    private bool homingEnabled = false;
    private float homingStrength = 0f;
    private LevelManager levelManager;

    protected override void OnEntityAwake()
    {
        bulletRigidbody2D = GetComponent<Rigidbody2D>();
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    public void Setup(BulletOwner owner, Vector2 direction, float overrideSpeed = -1)
    {
        Owner = owner;
        ScreenwrapsUsed = 0;
        ScreenwrapManager.Register(this);

        if(overrideSpeed > 0) 
        { 
            bulletSpeed = overrideSpeed; 
        }

        bulletRigidbody2D.linearVelocity = direction.normalized * bulletSpeed;

        // Set sprite and collision ignore timer based on owner...
        if(Owner == BulletOwner.PLAYER)
        {
            ignoreTimer = IGNORE_TIME;

            Player player = levelManager.GetEntity<Player>();

            if(player != null)
            {
                maxWraps += player.BulletWrapUpgradeCount;

                if(player.HasHomingBullets) 
                {
                    EnableHoming(player.HomingStrength); 
                }
            }
        }
        else
        {
            ignoreTimer = 0f;
        }
    }

    public void EnableHoming(float strength)
    {
        homingEnabled = true;
        homingStrength = strength;
    }

    private void FixedUpdate()
    {
        if(ignoreTimer > 0f)
        {
            ignoreTimer -= Time.fixedDeltaTime;
        }

        Vector2 rotatedOffset = Quaternion.Euler(0, 0, EntityEulerAngles.z) * hitBoxOffset;
        Vector2 checkPos = EntityPosition + rotatedOffset;

        // Homing bullet logic...
        if(homingEnabled && Owner == BulletOwner.PLAYER)
        {
            Enemy[] enemies = levelManager.GetEntities<Enemy>().ToArray();

            float minDist = float.MaxValue;
            Enemy nearest = null;

            foreach(Enemy enemy in enemies)
            {
                float dist = Vector2.Distance(enemy.CenterOfMass, checkPos);
                if(dist < minDist) { minDist = dist; nearest = enemy; }
            }

            if(nearest != null)
            {
                Vector2 toTarget = (nearest.CenterOfMass - checkPos).normalized;
                Vector2 currentDir = bulletRigidbody2D.linearVelocity.normalized;
                Vector2 newDir = Vector2.Lerp(currentDir, toTarget, homingStrength);
                bulletRigidbody2D.linearVelocity = newDir * bulletSpeed;
            }
        }

        LayerMask layerMask = LayerManager.Masks.ASTEROID;

        if(Owner == BulletOwner.PLAYER)
        {
            layerMask |= LayerManager.Masks.ENEMY;
            if(ignoreTimer <= 0f) { layerMask |= LayerManager.Masks.PLAYER; }
        }
        else
        {
            layerMask |= LayerManager.Masks.PLAYER;
            if(ignoreTimer <= 0f) { layerMask |= LayerManager.Masks.ENEMY; }
        }

        Collider2D hitCollider = Physics2D.OverlapBox(checkPos, hitBoxSize, EntityEulerAngles.z, layerMask);

        if(hitCollider != null && hitCollider.gameObject != EntityObject)
        {
            Asteroid asteroidEntity = hitCollider.GetComponentInParent<Asteroid>();
            asteroidEntity?.TakeDamage(1, this);

            Player playerEntity = hitCollider.GetComponentInParent<Player>();
            playerEntity?.TakeDamage();

            Enemy enemyEntity = hitCollider.GetComponentInParent<Enemy>();
            enemyEntity?.TakeDamage(1, this);

            ScreenwrapManager.Unregister(this);
            DestroyEntity();
        }

        CheckEnemyBulletNearMiss();
    }

    private void CheckEnemyBulletNearMiss()
    {
        if(Owner == BulletOwner.ENEMY && !hasRegisteredNearMiss)
        {
            const float CHECK_DISTANCE = 1.5f;
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            Player playerEntity = levelManager.GetEntity<Player>();

            if(playerEntity != null)
            {
                Vector2 rotatedOffset = Quaternion.Euler(0, 0, EntityEulerAngles.z) * hitBoxOffset;
                Vector3 checkPos = EntityPosition + rotatedOffset;
                float distance = Vector2.Distance(playerEntity.CenterOfMass, checkPos);

                if(distance <= CHECK_DISTANCE)
                {
                    playerEntity.PlayerStatistics.RegisterNearMiss();
                    hasRegisteredNearMiss = true;
                    nearMissTimer = nearMissCooldown;
                }
            }
        }

        if(hasRegisteredNearMiss)
        {
            nearMissTimer -= Time.fixedDeltaTime;

            if(nearMissTimer <= 0f)
            {
                hasRegisteredNearMiss = false;
            }
        }
    }

    public void OnScreenwrap()
    {
        if(Owner == BulletOwner.PLAYER)
        {
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            Player player = levelManager.GetEntity<Player>();

            if(player != null)
            {
                player.PlayerStatistics.RegisterRecursion();
            }
        }
    }

    protected override void OnDrawEntityGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 rotatedOffset = Quaternion.Euler(0, 0, EntityEulerAngles.z) * hitBoxOffset;
        Vector3 checkPos = EntityPosition + rotatedOffset;
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(checkPos, Quaternion.Euler(0, 0, EntityEulerAngles.z), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitBoxSize);
        Gizmos.matrix = originalMatrix;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 pos = EntityPosition + ScreenwrapBoundsOffset;
        Vector3 center3D = new Vector3(pos.x, pos.y, 0f);
        Vector3 size3D = new Vector3(ScreenwrapBoundsSize.x, ScreenwrapBoundsSize.y, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center3D, size3D);
    }

    public BulletOwner Owner { get; private set; }

    public Vector2 ScreenwrapBoundsSize { get => wrapBoundsSize; }
    public Vector2 ScreenwrapBoundsOffset { get => wrapBoundsOffset; }
    public int MaxScreenwraps { get => maxWraps; }
    public int ScreenwrapsUsed { get; set; }

    public enum BulletOwner 
    { 
        PLAYER, 
        ENEMY 
    }
}
