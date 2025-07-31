#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Helper class for loading and saving assets...
/// </summary>
public static class AssetUtility
{
    /// <summary>
    /// Load an asset...
    /// </summary>
    public static T LoadAsset<T>(string assetPath) where T : Object
    {
        if(!assetPath.StartsWith("Assets/"))
        {
            assetPath = "Assets/" + assetPath;
        }

        T assetLoaded = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        if(assetLoaded == null)
        {
            Debug.LogWarning("Asset not found: " + assetPath);
            return null;
        }
        else
        {
            return assetLoaded;
        }
    }

    /// <summary>
    /// Load all assets at the specified path...
    /// </summary>
    public static List<T> LoadAllAssets<T>(string assetPath) where T : Object
    {
        if(!assetPath.StartsWith("Assets/"))
        {
            assetPath = "Assets/" + assetPath;
        }

        List<T> assetsLoaded = new List<T>();

        string[] assetGuids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { assetPath });

        foreach(string guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if(asset != null)
            {
                assetsLoaded.Add(asset);
            }
        }

        if(assetsLoaded.Count == 0)
        {
            Debug.LogWarning("No assets found at path: " + assetPath);
        }

        return assetsLoaded;
    }

    /// <summary>
    /// Create a new asset...
    /// </summary>
    public static void CreateAsset(Object asset, string assetPath, bool preventOverwrite = false)
    {
        if(!assetPath.StartsWith("Assets/"))
        {
            assetPath = "Assets/" + assetPath;
        }

        // Check if asset already exists to avoid overwriting it...
        if(LoadAsset<Object>(assetPath) != null && preventOverwrite)
        {
            Debug.LogWarning($"An asset already exists at the specified path: [{ assetPath }]!");
            return;
        }

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Asset created: [{ assetPath }]");
    }

    /// <summary>
    /// Get asset path name...
    /// </summary>
    public static string GetAssetPath(Object obj)
    {
        if(obj == null)
        {
            return string.Empty;
        }

        string path = AssetDatabase.GetAssetPath(obj);

        if(string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        else
        {
            return path;
        }
    }

    /// <summary>
    /// Ensure the directory exists, creating it if necessary...
    /// </summary>
    public static void EnsureDirectory(string directoryPath)
    {
        if(!AssetDatabase.IsValidFolder(directoryPath))
        {
            string parentFolder = System.IO.Path.GetDirectoryName(directoryPath);
            if(AssetDatabase.IsValidFolder(parentFolder))
            {
                AssetDatabase.CreateFolder(parentFolder, System.IO.Path.GetFileName(directoryPath));
            }
        }
    }

    /// <summary>
    /// Save the prefab asset to the specified folder, generating a unique name if needed....
    /// </summary>
    public static string SavePrefabToFolder(GameObject prefab, string folderPath)
    {
        EnsureDirectory(folderPath);

        string newPrefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{prefab.name}.prefab");
        PrefabUtility.SaveAsPrefabAsset(prefab, newPrefabPath);
        return newPrefabPath;
    }
}
#endif
