using UnityEngine;

/// <summary>
/// Handles everything related to sprite animations...
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] sprites;
    public float speed = 1.0f;
    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(sprites == null || sprites.Length == 0)
        {
            return;
        }

        timer += Time.deltaTime * speed;

        if(timer >= 1.0f)
        {
            int framesToAdvance = (int)timer;
            timer -= framesToAdvance;
            currentSpriteIndex = (currentSpriteIndex + framesToAdvance) % sprites.Length;
            spriteRenderer.sprite = sprites[currentSpriteIndex];
        }
    }
}