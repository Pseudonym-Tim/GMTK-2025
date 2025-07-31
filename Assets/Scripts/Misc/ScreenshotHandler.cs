using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles the capturing and saving of in-game screenshots...
/// </summary>
public static class ScreenshotHandler
{
    public static void TakeScreenshot()
    {
        // Get the current time...
        DateTime currentTime = DateTime.Now;

        // Create an array to format the filename...
        object[] timeArgs = new object[]
        {
            currentTime.Year,
            currentTime.Month,
            currentTime.Day,
            currentTime.Hour,
            currentTime.Minute,
            currentTime.Second
        };

        // Format the filename...
        string fileName = string.Format("{0}-{1:00}-{2:00}_{3:00}-{4:00}-{5:00}.png", timeArgs);

        // Create the full file path...
        string filePath = ScreenshotDir + fileName;

        // Create the directory if it doesn't exist...
        if(!Directory.Exists(ScreenshotDir))
        {
            Directory.CreateDirectory(ScreenshotDir);
        }

        // Capture the screenshot and save it to the directory...
        ScreenCapture.CaptureScreenshot(filePath);

        // Log the filename of the saved screenshot...
        Debug.Log($"Saved screenshot as: [<u>{fileName}</u>]");
    }

    public static string ScreenshotDir
    {
        get
        {
            return Application.persistentDataPath + "/screenshots/";
        }
    }
}
