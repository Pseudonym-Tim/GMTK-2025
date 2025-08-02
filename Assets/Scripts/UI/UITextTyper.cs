using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// Helper class for typing out characters individually into a text field...
/// </summary>
public static class UITextTyper
{
    public static bool IsTyping { get; set; } = false;

    public static IEnumerator TypeText(string message, TextMeshProUGUI messageText, float typeDelay)
    {
        IsTyping = true;

        // Fix escape characters...
        message = message.Replace("\\n", "\n");

        string pattern = @"(<.*?>|\d+\s+\w+|[^<]+)";
        //string pattern = @"(<.*?>|[+-]?\d+\s+\w+|[^<]+)";
        MatchCollection matches = Regex.Matches(message, pattern);
        messageText.text = "";

        foreach(Match match in matches)
        {
            if(match.Value.StartsWith("<"))
            {
                // Handle rich text tag...
                messageText.text += match.Value;
            }
            else
            {
                // Handle regular text...
                foreach(char letter in match.Value)
                {
                    messageText.text += letter;
                    yield return new WaitForSecondsRealtime(typeDelay);
                }
            }
        }

        IsTyping = false;
    }
}