using UnityEngine;

/// <summary>
/// Handles overall input management related stuff...
/// </summary>
public class InputManager : Singleton<InputManager>
{
    public static bool IsButtonPressed(string buttonName) => Input.GetButtonDown(buttonName);
    public static float GetAxis(string axisName) => Input.GetAxis(axisName);
    public static float GetAxisRaw(string axisName) => Input.GetAxisRaw(axisName);
    public static bool IsButtonHeld(string buttonName) => Input.GetButton(buttonName);

    private void Awake()
    {
        LockCursor(false);
        ShowHardwareCursor(true);
    }

    public static void EnableInput()
    {
        PlayerInput.InputEnabled = true;
    }

    public static void DisableInput()
    {
        PlayerInput.InputEnabled = false;
    }

    public static void LockCursor(bool lockCursor = false)
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.Confined;
    }

    public static void ShowHardwareCursor(bool showCursor = true)
    {
        Cursor.visible = showCursor;
    }
}