using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything to do with the background of the level...
/// </summary>
public class SpaceBackground : Singleton<SpaceBackground>
{
    [SerializeField] private Sprite background1;
    [SerializeField] private Sprite background2;
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private int renderLayer = -1;

    [Header("Animation Settings")]
    public float twinkleMinDelay = 1f;
    public float twinkleMaxDelay = 2f;
    [Range(0f, 1f)] public float twinkleChancePerStar = 0.2f;
    public int minStarsTwinkle = 1;
    public int maxStarsTwinkle = 5;

    private List<SpriteRenderer> backgroundGFXList = new List<SpriteRenderer>();

    private void Awake()
    {
        GenerateBackground();
        StartCoroutine(AnimateBackground());
    }

    private void GenerateBackground()
    {
        float offsetX = (gridWidth - 1) * spacing * 0.5f;
        float offsetY = (gridHeight - 1) * spacing * 0.5f;

        for(int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                GameObject background = new GameObject("Background");
                background.transform.SetParent(transform);
                background.transform.position = new Vector3(transform.position.x + (x * spacing - offsetX), transform.position.y + (y * spacing - offsetY), 0);

                SpriteRenderer backgroundGFX = background.AddComponent<SpriteRenderer>();
                backgroundGFX.sortingOrder = renderLayer;
                backgroundGFX.sprite = Random.value > 0.5f ? background1 : background2;

                backgroundGFXList.Add(backgroundGFX);
            }
        }
    }

    private IEnumerator AnimateBackground()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(twinkleMinDelay, twinkleMaxDelay));

            int twinkleCount = Random.Range(minStarsTwinkle, Mathf.Min(maxStarsTwinkle, backgroundGFXList.Count));
            int changedCount = 0;

            for(int i = 0; i < backgroundGFXList.Count && changedCount < twinkleCount; i++)
            {
                if(Random.value <= twinkleChancePerStar)
                {
                    SpriteRenderer backgroundGFX = backgroundGFXList[Random.Range(0, backgroundGFXList.Count)];
                    backgroundGFX.sprite = backgroundGFX.sprite == background1 ? background2 : background1;
                    changedCount++;
                }
            }
        }
    }
}
