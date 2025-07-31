using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Holds and manages a database of prefabs...
/// </summary>
[CreateAssetMenu(fileName = "PrefabDatabase", menuName = GameManager.GAME_NAME + "/PrefabDatabase")]
public class PrefabDatabase : ScriptableObject
{
    [SerializeField] private List<GameObject> registeredPrefabs = new List<GameObject>();

    /// <summary>
    /// Registers a prefab if it doesn't already exist in the database...
    /// </summary>
    public void RegisterPrefab(GameObject prefab)
    {
        if(registeredPrefabs.Any(prefabObject => prefabObject.name == prefab.name))
        {
            Debug.LogWarning($"Prefab: [{prefab.name}] already exists!");
            return;
        }

        registeredPrefabs.Add(prefab);
        Debug.Log($"Prefab: [{prefab.name}] registered in {nameof(PrefabDatabase)}!");
    }

    /// <summary>
    /// Spawns a prefab by name at the specified position and rotation...
    /// </summary>
    public GameObject SpawnPrefab(string name, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = GetPrefab(name);

        if(prefab != null)
        {
            return Instantiate(prefab, position, rotation);
        }

        Debug.LogWarning($"Prefab: [{name}] could not be found in {nameof(PrefabDatabase)}!");
        return null;
    }

    /// <summary>
    /// Spawns a prefab by name at the specified position and rotation and returns a component of a specific type...
    /// </summary>
    public T SpawnPrefab<T>(string name, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject prefab = GetPrefab(name);

        if(prefab == null)
        {
            Debug.LogWarning($"Prefab: [{name}] could not be found!");
            return null;
        }

        GameObject instance = Instantiate(prefab, position, rotation);
        T component = instance.GetComponent<T>();

        if(component == null)
        {
            Debug.LogWarning($"Component of type {typeof(T)} not found on prefab: [{name}]!");
            Destroy(instance);
        }

        return component;
    }

    /// <summary>
    /// Retrieves a prefab by its name from the database...
    /// </summary>
    public GameObject GetPrefab(string name)
    {
        return registeredPrefabs.FirstOrDefault(prefab => prefab.name == name);
    }

    /// <summary>
    /// Retrieves a component of type T from a prefab by its name from the database...
    /// </summary>
    public T GetPrefab<T>(string name) where T : Component
    {
        GameObject prefab = GetPrefab(name);
        return prefab != null ? prefab.GetComponent<T>() : null;
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/Register Prefab", false, 10)]
    private static void CheckRegisterPrefab()
    {
        GameObject selectedPrefab = Selection.activeGameObject;

        if(selectedPrefab != null)
        {
            // Get the current asset path...
            string prefabPath = AssetUtility.GetAssetPath(selectedPrefab);

            if(!prefabPath.StartsWith("Assets/Prefabs"))
            {
                string folderPath = "Assets/Prefabs";

                // Save the prefab to the prefabs folder...
                prefabPath = AssetUtility.SavePrefabToFolder(selectedPrefab, folderPath);
            }

            // Load the prefab from the prefabs folder...
            GameObject prefab = AssetUtility.LoadAsset<GameObject>(prefabPath);

            // Register the prefab...
            PrefabDatabase prefabDatabase = GetPrefabDatabase();
            prefabDatabase?.RegisterPrefab(prefab);

            // Mark the database as dirty to ensure it is saved...
            EditorUtility.SetDirty(prefabDatabase);
        }
        else
        {
            Debug.LogWarning("No prefab selected to register!");
        }
    }

    private static PrefabDatabase GetPrefabDatabase()
    {
        string[] assetGUIDs = AssetDatabase.FindAssets("t:PrefabDatabase");

        if(assetGUIDs.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
            return AssetDatabase.LoadAssetAtPath<PrefabDatabase>(assetPath);
        }

        Debug.LogWarning("PrefabDatabase asset not found. Please create one!");
        return null;
    }
#endif
}
