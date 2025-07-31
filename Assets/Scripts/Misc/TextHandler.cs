using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Handles everything related to in-game text...
/// </summary>
public static class TextHandler
{
    public static JObject JsonData
    {
        get
        {
            return JsonUtility.ParseJson("game_english");
        }
    }

    public static string GetText(string key, string category = null)
    {
        if(JsonData == null)
        {
            Debug.LogError("JSON data not loaded!");
            return null;
        }

        JToken jsonToken = category == null ? JsonData[key] : JsonData[category]?[key];

        if(jsonToken != null)
        {
            return jsonToken.ToString();
        }
        else
        {
            string message = category == null ? $"Text not found for key: {key}" : $"Text not found for category: {category}, key: {key}";
            Debug.LogError(message);
            return null;
        }
    }
}
