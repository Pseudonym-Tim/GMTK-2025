using UnityEngine;
using System.Collections;

/// <summary>
/// Strategic sniper enemy spaceship...
/// </summary>
public class EnemySniperSpaceship : Enemy
{
    [Header("Movement")]
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float moveAcceleration = 5f;
    [SerializeField] private float moveDeceleration = 2.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;
    [SerializeField] private float wanderRange = 5f;
    [SerializeField] private float retreatDistanceThreshold = 5f;

    [Header("Shooting")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float predictionLeadTime = 1f;
    [SerializeField] private float minimumChargeTime = 1f;

    private float wanderTimer = 0f;
    private Vector2 wanderDirection = Vector2.zero;
    private float shootTimer = 0f;
    private Rigidbody2D playerRigidbody2D;
    private bool isCharging = false;
    private Vector2? lockedTargetPos = null;

    protected override void OnEntityAwake()
    {
        base.OnEntityAwake();
    }

    public override void OnEntitySpawn()
    {
        base.OnEntitySpawn();
        wanderTimer = wanderChangeInterval;
        wanderDirection = Random.insideUnitCircle.normalized * wanderRange;
        shootTimer = Random.Range(0f, shootCooldown);
        playerRigidbody2D = playerEntity != null ? playerEntity.GetComponent<Rigidbody2D>() : null;
    }

    private void FixedUpdate()
    {
        if(playerEntity == null)
        {
            return;
        }

        wanderTimer -= Time.fixedDeltaTime;

        if(wanderTimer <= 0f)
        {
            wanderDirection = Random.insideUnitCircle.normalized * wanderRange;
            wanderTimer = wanderChangeInterval;
        }

        Vector2 currentPos = EntityPosition;
        Vector2 realTargetPos = playerEntity.CenterOfMass;
        Vector2 velocity = playerRigidbody2D != null ? playerRigidbody2D.linearVelocity : Vector2.zero;
        Vector2 predictedPos = realTargetPos + velocity * predictionLeadTime;
        float distance = Vector2.Distance(currentPos, realTargetPos);

        Vector2 desiredVelocity;

        if(distance < retreatDistanceThreshold)
        {
            Vector2 fleeDir = (currentPos - realTargetPos).normalized;
            desiredVelocity = fleeDir * moveSpeed;
        }
        else
        {
            desiredVelocity = wanderDirection * moveSpeed;
        }

        if(desiredVelocity.magnitude > 0.1f)
        {
            enemyRigidbody2D.linearVelocity = Vector2.MoveTowards(enemyRigidbody2D.linearVelocity, desiredVelocity, moveAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            enemyRigidbody2D.linearVelocity = Vector2.MoveTowards(enemyRigidbody2D.linearVelocity, Vector2.zero, moveDeceleration * Time.fixedDeltaTime);
        }

        float angle = Mathf.Atan2(desiredVelocity.y, desiredVelocity.x) * Mathf.Rad2Deg - 90f;
        enemyRigidbody2D.MoveRotation(Mathf.LerpAngle(enemyRigidbody2D.rotation, angle, rotateSpeed * Time.fixedDeltaTime));

        shootTimer -= Time.fixedDeltaTime;

        if(shootTimer <= 0f && distance <= detectionRange && !isCharging)
        {
            StartCoroutine(ChargeAndShoot(predictedPos));
            shootTimer = shootCooldown;
        }
    }

    private IEnumerator ChargeAndShoot(Vector2 aimPos)
    {
        isCharging = true;
        lockedTargetPos = aimPos;
        float timer = 0f;

        sfxManager.Play2DSound("sniper_charge");

        while(timer < minimumChargeTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if(lockedTargetPos.HasValue)
        {
            Vector2 dir = lockedTargetPos.Value - (Vector2)shootPoint.position;
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0f, 0f, angleDeg - 90f);
            BulletProjectile bullet = (BulletProjectile)levelManager.SpawnEntity("bullet_projectile", shootPoint.position, rot);
            bullet.Setup(BulletProjectile.BulletOwner.ENEMY, dir.normalized);

            sfxManager.Play2DSound("sniper_fire");
        }
        else
        {
            sfxManager.Play2DSound("sniper_charge_cancel");
        }

        lockedTargetPos = null;
        isCharging = false;
    }

    protected override void OnDrawEntityGizmos()
    {
        base.OnDrawEntityGizmos();
        Vector3 centerOfMass = new Vector3(CenterOfMass.x, CenterOfMass.y, 0f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerOfMass, detectionRange);
        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(centerOfMass, wanderRange);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(centerOfMass, centerOfMass + EntityTransform.up);
        Gizmos.color = Color.pink;
        Gizmos.DrawWireSphere(centerOfMass, retreatDistanceThreshold);

        if(playerEntity != null)
        {
            Vector2 realTarget = playerEntity.CenterOfMass;
            Vector2 linearVel = playerRigidbody2D != null ? playerRigidbody2D.linearVelocity : Vector2.zero;
            Vector3 predicted = new Vector3(realTarget.x + linearVel.x * predictionLeadTime, realTarget.y + linearVel.y * predictionLeadTime, 0f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(predicted, 0.1f);
            Gizmos.DrawLine(centerOfMass, predicted);
        }
    }
}