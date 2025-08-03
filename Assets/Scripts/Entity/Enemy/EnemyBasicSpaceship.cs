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

    [Header("Shooting")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float predictionLeadTime = 1f;

    [Header("Avoidance")]
    [SerializeField] private float avoidanceRange = 3f;
    [SerializeField] private float avoidanceStrength = 0.5f;

    private float shootTimer;
    private Rigidbody2D playerRigidbody2D;

    protected override void OnEntityAwake()
    {
        base.OnEntityAwake();
    }

    public override void OnEntitySpawn()
    {
        base.OnEntitySpawn();
        shootTimer = Random.Range(0f, shootCooldown);
        playerRigidbody2D = playerEntity != null ? playerEntity.GetComponent<Rigidbody2D>() : null;
    }

    private void FixedUpdate()
    {
        if(playerEntity == null)
        {
            return;
        }

        Vector2 currentPos = EntityPosition;
        Vector2 realTargetPos = playerEntity.CenterOfMass;
        Vector2 velocity = playerRigidbody2D != null ? playerRigidbody2D.linearVelocity : Vector2.zero;
        Vector2 predictedPos = realTargetPos + velocity * predictionLeadTime;
        Vector2 wrappedPredictedPos = ScreenwrapManager.GetBestWrappedPosition(currentPos, predictedPos);

        Vector2 rotationDir = (wrappedPredictedPos - currentPos).normalized;
        UpdateRotation(rotationDir);

        Vector2 wrappedTargetPos = ScreenwrapManager.GetBestWrappedPosition(currentPos, realTargetPos);
        Vector2 movementDir = (wrappedTargetPos - currentPos).normalized;
        UpdateMovement(movementDir);

        UpdateShooting(currentPos, predictedPos, wrappedPredictedPos);
    }

    private void UpdateRotation(Vector2 direction)
    {
        float desiredAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = enemyRigidbody2D.rotation;
        float deltaAngle = Mathf.DeltaAngle(currentAngle, desiredAngle);
        float turnAmount = Mathf.Sign(deltaAngle) * rotateSpeed * Time.fixedDeltaTime;

        if(Mathf.Abs(turnAmount) > Mathf.Abs(deltaAngle))
        {
            turnAmount = deltaAngle;
        }

        enemyRigidbody2D.MoveRotation(currentAngle + turnAmount);
    }

    private void UpdateMovement(Vector2 direction)
    {
        Vector2 avoidanceDir = Vector2.zero;

        List<Asteroid> asteroids = levelManager.GetEntities<Asteroid>(CenterOfMass, avoidanceRange);
        asteroids.RemoveAll(delegate (Asteroid item) { return item == null; });

        foreach(Asteroid asteroidEntity in asteroids)
        {
            if(asteroidEntity == null)
            {
                continue;
            }

            Vector2 away = EntityPosition - asteroidEntity.CenterOfMass;
            avoidanceDir += away.normalized / away.magnitude;
        }

        List<Enemy> otherEnemies = levelManager.GetEntities<Enemy>(CenterOfMass, avoidanceRange);
        otherEnemies.RemoveAll(delegate (Enemy item) { return item == null || item == this; });

        foreach(Enemy enemyEntity in otherEnemies)
        {
            if(enemyEntity == null)
            {
                continue;
            }

            Vector2 awayEnemy = EntityPosition - enemyEntity.CenterOfMass;
            avoidanceDir += awayEnemy.normalized / awayEnemy.magnitude;
        }

        float playerDistance = Vector2.Distance(CenterOfMass, playerEntity.CenterOfMass);

        if(playerDistance < avoidanceRange)
        {
            Vector2 awayFromPlayer = EntityPosition - playerEntity.CenterOfMass;

            if(awayFromPlayer != Vector2.zero)
            {
                avoidanceDir = awayFromPlayer.normalized;
            }
        }
        else if(avoidanceDir != Vector2.zero)
        {
            avoidanceDir.Normalize();
            avoidanceDir = (direction + avoidanceDir * avoidanceStrength).normalized;
        }

        Vector2 desiredVel = avoidanceDir * moveSpeed;
        Vector2 newVel = Vector2.MoveTowards(enemyRigidbody2D.linearVelocity, desiredVel, moveAcceleration * Time.fixedDeltaTime);
        enemyRigidbody2D.linearVelocity = newVel;
    }

    private void UpdateShooting(Vector2 currentPos, Vector2 realPredictedPos, Vector2 wrappedPredictedPos)
    {
        shootTimer -= Time.deltaTime;

        if(shootTimer > 0f || Vector2.Distance(CenterOfMass, playerEntity.CenterOfMass) > detectionRange || !IsVisibleOnScreen())
        {
            return;
        }

        Vector2 aimDir = (wrappedPredictedPos - currentPos).normalized;
        float aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
        enemyRigidbody2D.MoveRotation(aimAngle);

        BulletProjectile bullet = (BulletProjectile)levelManager.SpawnEntity("bullet_projectile", shootPoint.position, shootPoint.rotation);
        bullet.Setup(BulletProjectile.BulletOwner.ENEMY, shootPoint.up);

        sfxManager.Play2DSound("basic_shoot");
        shootTimer = shootCooldown;
    }

    protected override void OnDrawEntityGizmos()
    {
        base.OnDrawEntityGizmos();

        Vector3 centerOfMass = new Vector3(CenterOfMass.x, CenterOfMass.y, 0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerOfMass, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerOfMass, avoidanceRange);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(centerOfMass, centerOfMass + EntityTransform.up);

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