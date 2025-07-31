using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Handles everything related to the player camera...
/// </summary>
public class PlayerCamera : Entity
{
    private const int DEFAULT_ZOOM = 4;

    [SerializeField] private PixelCameraSettings pixelCameraSettings;

    private PixelPerfectCamera pixelPerfectCamera;

    [System.Serializable]
    public class PixelCameraSettings
    {
        public int pixelsPerUnit = 16;
        public bool upscaleRenderTexture = true;
    }

    public void Setup()
    {
        PlayerEntity = GetComponentInParent<Player>();
        pixelPerfectCamera = GetComponentInChildren<PixelPerfectCamera>();
        ApplyPixelPerfectSettings();
        SetParent(null);
    }

    private void ApplyPixelPerfectSettings()
    {
        pixelPerfectCamera.assetsPPU = pixelCameraSettings.pixelsPerUnit;
        pixelPerfectCamera.upscaleRT = pixelCameraSettings.upscaleRenderTexture;
        pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / DEFAULT_ZOOM);
        pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / DEFAULT_ZOOM);
    }

    public Camera Camera { get { return Camera.main; } }
    public Player PlayerEntity { get; set; } = null;
}
