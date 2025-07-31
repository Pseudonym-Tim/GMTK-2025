using Newtonsoft.Json.Linq;

/// <summary>
/// Holds data for level entities...
/// </summary> 
[System.Serializable]
public class LevelEntityData
{
    public string name;
    public string id;
    public string prefab;
    public JObject jsonData;
}