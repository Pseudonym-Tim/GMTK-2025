using UnityEngine;

/// <summary>
/// A bullet projectile entity...
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BulletProjectile : Entity
{
    private const int LIFETIME = 10;

    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private Vector2 overlapBoxSize = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 overlapBoxOffset = Vector2.zero;

    private Rigidbody2D bulletRigidbody2D;

    protected override void OnEntityAwake()
    {
        bulletRigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Setup(Vector2 direction, float overrideSpeed = -1)
    {
        DestroyEntity(LIFETIME);
        if(overrideSpeed > 0) { bulletSpeed = overrideSpeed; }
        bulletRigidbody2D.linearVelocity = direction.normalized * bulletSpeed;
    }

    private void FixedUpdate()
    {
        Vector2 rotatedOffset = Quaternion.Euler(0, 0, EntityEulerAngles.z) * overlapBoxOffset;

        Vector3 checkPos = EntityPosition + rotatedOffset;

        LayerMask layerMask = LayerManager.Masks.ENEMY;
        Collider2D hitCollider = Physics2D.OverlapBox(checkPos, overlapBoxSize, EntityEulerAngles.z, layerMask);

        if(hitCollider != null && hitCollider.gameObject != EntityObject)
        {
            // TODO: Deal damage or destroy hit enemy entity...
            DestroyEntity();
        }
    }

    protected override void OnDrawEntityGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 rotatedOffset = Quaternion.Euler(0, 0, EntityEulerAngles.z) * overlapBoxOffset;
        Vector3 checkPos = EntityPosition + rotatedOffset;
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(checkPos, Quaternion.Euler(0, 0, EntityEulerAngles.z), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, overlapBoxSize);
        Gizmos.matrix = originalMatrix;
    }
}
