using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to level objects...
/// </summary>
public class LevelObject : MonoBehaviour
{
    public void DestroyObject(float destroyTime = 0)
    {
        if(Application.isPlaying) { Destroy(gameObject, destroyTime); }
        else { DestroyImmediate(gameObject); }
    }

    public void ShowObject(bool showObject = true)
    {
        gameObject.SetActive(showObject);
    }

    public void SetParent(Transform parentTransform, bool worldPositionStays = true)
    {
        ObjectTransform.SetParent(parentTransform, worldPositionStays);
    }

    public T AddComponent<T>() where T : Component
    {
        return gameObject.AddComponent<T>();
    }

    public Transform ObjectTransform { get { return transform; } }
    public Vector2 ObjectPosition { get { return ObjectTransform.position; } set { ObjectTransform.position = value; } }
    public Vector2 ObjectLocalPosition { get { return ObjectTransform.localPosition; } set { ObjectTransform.localPosition = value; } }
    public Quaternion ObjectRotation { get { return ObjectTransform.rotation; } set { ObjectTransform.rotation = value; } }
    public Vector3 ObjectEulerAngles { get { return ObjectTransform.eulerAngles; } set { ObjectTransform.eulerAngles = value; } }
    public Vector3 ObjectLocalEulerAngles { get { return ObjectTransform.localEulerAngles; } set { ObjectTransform.localEulerAngles = value; } }
    public Quaternion ObjectLocalRotation { get { return ObjectTransform.localRotation; } set { ObjectTransform.localRotation = value; } }
    public Vector3 ObjectLocalScale { get { return ObjectTransform.localScale; } set { ObjectTransform.localScale = value; } }
}
