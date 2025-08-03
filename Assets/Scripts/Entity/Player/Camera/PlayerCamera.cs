using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Handles everything related to the player camera...
/// </summary>
public class PlayerCamera : Entity
{
    private const int DEFAULT_ZOOM = 2;

    [SerializeField] private Transform shakeTransform;
    [SerializeField] private PixelCameraSettings pixelCameraSettings;

    private PixelPerfectCamera pixelPerfectCamera;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

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
        originalPosition = shakeTransform.localPosition;
    }

    private void ApplyPixelPerfectSettings()
    {
        pixelPerfectCamera.assetsPPU = pixelCameraSettings.pixelsPerUnit;
        pixelPerfectCamera.upscaleRT = pixelCameraSettings.upscaleRenderTexture;
        pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / DEFAULT_ZOOM);
        pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / DEFAULT_ZOOM);
    }

    public void Shake(float duration, float intensity)
    {
        if(shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeTransform.localPosition = originalPosition;
        }

        shakeCoroutine = StartCoroutine(PerformShake(duration, intensity));
    }

    private IEnumerator PerformShake(float duration, float intensity)
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            shakeTransform.localPosition = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeTransform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    public Camera Camera { get { return Camera.main; } }
    public Player PlayerEntity { get; set; } = null;
}
