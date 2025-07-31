using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to player input...
/// </summary>
public class PlayerInput : Singleton<PlayerInput>
{
    public static bool IsButtonPressed(string buttonName) => InputManager.IsButtonPressed(buttonName) && InputEnabled;
    public static bool IsButtonHeld(string buttonName) => InputManager.IsButtonHeld(buttonName) && InputEnabled;
    public static float GetAxis(string axisName) => InputEnabled ? InputManager.GetAxis(axisName) : 0;
    public static float GetAxisRaw(string axisName) => InputEnabled ? InputManager.GetAxisRaw(axisName) : 0;

    public static bool InputEnabled { get; set; } = false;
}
