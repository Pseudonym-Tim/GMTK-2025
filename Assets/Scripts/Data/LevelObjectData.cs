using Newtonsoft.Json.Linq;

/// <summary>
/// Holds data for level objects...
/// </summary> 
[System.Serializable]
public class LevelObjectData
{
    public string prefab;
    public string id;
    public JObject jsonData;
}