using UnityEngine;

/// <summary>
/// Player spaceship entity...
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity
{
    [Header("Movement")]
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float moveAcceleration = 10f;
    [SerializeField] private float moveDeceleration = 5f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Shooting")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootCooldown = 0.25f;

    private Rigidbody2D playerRigidbody2D;
    private LevelManager levelManager;
    private float targetRotation;
    private float shootTimer = 0f;

    protected override void OnEntityAwake()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        playerRigidbody2D = GetComponent<Rigidbody2D>();
        PlayerCamera = GetComponentInChildren<PlayerCamera>();
    }

    public override void OnEntitySpawn()
    {
        PlayerCamera?.Setup();
        PlayerInput.InputEnabled = true;
    }

    protected override void OnEntityUpdate()
    {
        if(!PlayerInput.InputEnabled) { return; }

        // Turn ship...
        float rotationInput = 0f;
        if(PlayerInput.IsButtonHeld("TurnLeft")) { rotationInput = 1f; }
        if(PlayerInput.IsButtonHeld("TurnRight")) { rotationInput = -1f; }
        targetRotation = playerRigidbody2D.rotation + rotationInput * rotateSpeed;

        UpdateShooting();
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
        bulletProjectile.Setup(shootPoint.up);
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

    public PlayerCamera PlayerCamera { get; private set; } = null;
}
