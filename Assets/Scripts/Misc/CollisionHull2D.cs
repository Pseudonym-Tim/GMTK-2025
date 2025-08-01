using UnityEngine;

/// <summary>
/// Handles everything related to the collision hull...
/// </summary>
public class CollisionHull2D : MonoBehaviour
{
    [SerializeField] private HullData2D hullData = new HullData2D();
    private BoxCollider2D boxCollider;

    public enum Collider2DType
    {
        BOX,
        CIRCLE
    }

    private void Awake()
    {
        InitializeCollider();
    }

    private void InitializeCollider()
    {
        switch(hullData.Type)
        {
            case Collider2DType.BOX:
                BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.offset = hullData.Center;
                boxCollider.size = hullData.Size;
                break;

            case Collider2DType.CIRCLE:
                CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                circleCollider.offset = hullData.Center;
                circleCollider.radius = hullData.Radius;
                break;
        }
    }

    public static void AddCollisionHull(GameObject targetObject, HullData2D hullData)
    {
        CollisionHull2D collisionHull = targetObject.AddComponent<CollisionHull2D>();
        collisionHull.hullData = hullData;
        collisionHull.InitializeCollider();
    }

    public static void AddCollisionHull(Entity entity, HullData2D hullData)
    {
        AddCollisionHull(entity.EntityObject, hullData);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 worldCenter = transform.TransformPoint(hullData.Center);

        switch(hullData.Type)
        {
            case Collider2DType.BOX:
                Gizmos.DrawWireCube(worldCenter, new Vector3(hullData.Size.x, hullData.Size.y, 0f));
                break;

            case Collider2DType.CIRCLE:
                Gizmos.DrawWireSphere(worldCenter, hullData.Radius);
                break;
        }
    }

    [System.Serializable]
    public class HullData2D
    {
        public Collider2DType Type = Collider2DType.BOX;
        public Vector2 Center = Vector2.zero;
        public Vector2 Size = Vector2.one;
        public float Radius = 0.5f;
    }
}
