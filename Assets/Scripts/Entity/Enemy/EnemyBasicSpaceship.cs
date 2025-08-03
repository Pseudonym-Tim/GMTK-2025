using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic stupid enemy spaceship...
/// </summary>
public class EnemyBasicSpaceship : Enemy
{
    [Header("Movement")]
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float moveAcceleration = 5f;
    [SerializeField] private float moveDeceleration = 2.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;
    [SerializeField] private float wanderRange = 5f;

    [Header("Shooting")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float predictionLeadTime = 1f;

    private float wanderTimer = 0f;
    private Vector2 wanderDirection = Vector2.zero;
    private float shootTimer = 0f;
    private Rigidbody2D playerRigidbody2D;

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

        Vector2 desiredVelocity = wanderDirection * moveSpeed;

        if(desiredVelocity.magnitude > 0.1f)
        {
            enemyRigidbody2D.linearVelocity = Vector2.MoveTowards(enemyRigidbody2D.linearVelocity, desiredVelocity, moveAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            enemyRigidbody2D.linearVelocity = Vector2.MoveTowards(enemyRigidbody2D.linearVelocity, Vector2.zero, moveDeceleration * Time.fixedDeltaTime);
        }

        float angle = Mathf.Atan2(desiredVelocity.y, desiredVelocity.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        enemyRigidbody2D.MoveRotation(Mathf.LerpAngle(enemyRigidbody2D.rotation, angle, rotateSpeed * Time.fixedDeltaTime));

        shootTimer -= Time.fixedDeltaTime;

        if(shootTimer <= 0f && Vector2.Distance(currentPos, realTargetPos) <= detectionRange)
        {
            BulletProjectile bullet = (BulletProjectile)levelManager.SpawnEntity("bullet_projectile", shootPoint.position, shootPoint.rotation);
            bullet.Setup(BulletProjectile.BulletOwner.ENEMY, (predictedPos - currentPos).normalized);
            shootTimer = shootCooldown;
            sfxManager.Play2DSound("basic_shoot");
        }
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
        Vector3 forwardDir = EntityTransform.up;
        Gizmos.DrawLine(centerOfMass, centerOfMass + forwardDir);

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
