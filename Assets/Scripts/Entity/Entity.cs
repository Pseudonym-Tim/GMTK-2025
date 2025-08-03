using UnityEngine;

/// <summary>
/// Base class for all entities...
/// </summary>
public class Entity : MonoBehaviour
{
    private void Awake() => OnEntityAwake();
    private void OnDestroy() => OnEntityDestroyed();
    private void Update() => OnEntityUpdate();
    private void OnCollisionEnter2D(Collision2D collision2D) => OnEntityCollision2D(collision2D);

    protected virtual void OnEntityAwake() { }
    public virtual void OnEntitySpawn() { }
    protected virtual void OnEntityUpdate() { }
    protected virtual void OnEntityCollision2D(Collision2D collision2D) { }
    protected virtual void OnEntityDestroyed() { }

    public void DestroyEntity(float destroyTime = 0)
    {
        if(EntityObject != null)
        {
            if(Application.isPlaying) { Destroy(EntityObject, destroyTime); }
            else { DestroyImmediate(EntityObject); }
        }
    }

    public void ShowEntity(bool showEntity = true)
    {
        EntityObject.SetActive(showEntity);
    }

    public void SetParent(Transform parentTransform, bool worldPositionStays = true)
    {
        EntityTransform.SetParent(parentTransform, worldPositionStays);
    }

    protected void SetupEntityAnimator() => EntityAnim = new EntityAnimator(this);

    public T AddComponent<T>() where T : Component
    {
        return EntityObject.AddComponent<T>();
    }

    public virtual void Teleport(Vector2 newPos) => EntityPosition = newPos;

    private void OnDrawGizmos() => OnDrawEntityGizmos();

    protected virtual void OnDrawEntityGizmos()
    {

    }

    public virtual Vector2 CenterOfMass { get { return EntityPosition; } }
    public EntityAnimator EntityAnim { get; set; } = null;
    public LevelEntityData LevelEntityData { get; set; } = null;
    public GameObject EntityObject { get { return gameObject; } }
    public Transform EntityTransform { get { return transform; } }
    public Vector2 EntityPosition { get { return EntityTransform.position; } set { EntityTransform.position = value; } }
    public Vector2 EntityLocalPosition { get { return EntityTransform.localPosition; } set { EntityTransform.localPosition = value; } }
    public Quaternion EntityRotation { get { return EntityTransform.rotation; } set { EntityTransform.rotation = value; } }
    public Vector3 EntityEulerAngles { get { return EntityTransform.eulerAngles; } set { EntityTransform.eulerAngles = value; } }
    public Vector3 EntityLocalEulerAngles { get { return EntityTransform.localEulerAngles; } set { EntityTransform.localEulerAngles = value; } }
    public Quaternion EntityLocalRotation { get { return EntityTransform.localRotation; } set { EntityTransform.localRotation = value; } }
    public Vector3 EntityLocalScale { get { return EntityTransform.localScale; } set { EntityTransform.localScale = value; } }
}