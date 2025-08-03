using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything to do with the background of the level...
/// </summary>
public class SpaceBackground : Singleton<SpaceBackground>
{
    [Header("Sprite Pairs")]
    [SerializeField] private List<SpritePair> spritePairs = new List<SpritePair>();
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private int renderLayer = -1;

    [Header("Animation Settings")]
    public float animationMinDelay = 1f;
    public float animationMaxDelay = 2f;
    [Range(0f, 1f)] public float animationChancePerTile = 0.2f;
    public int minTilesAnimate = 1;
    public int maxTilesAnimate = 5;

    private class BackgroundTile
    {
        public SpriteRenderer renderer;
        public SpritePair pair;
        public bool isFrameOne;
    }

    [System.Serializable]
    public class SpritePair
    {
        public Sprite frame1;
        public Sprite frame2; 
        [Range(0.1f, 10f)] public float weight = 1f;
    }

    private List<BackgroundTile> tilesList = new List<BackgroundTile>();

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
                GameObject tileObject = new GameObject("BackgroundTile");
                tileObject.transform.SetParent(transform);
                tileObject.transform.position = new Vector3(transform.position.x + (x * spacing - offsetX), transform.position.y + (y * spacing - offsetY), 0);

                SpriteRenderer sr = tileObject.AddComponent<SpriteRenderer>();
                sr.sortingOrder = renderLayer;

                SpritePair chosenPair = GetWeightedRandomPair();

                bool frameOne = Random.value > 0.5f;
                sr.sprite = frameOne ? chosenPair.frame1 : chosenPair.frame2;

                BackgroundTile tile = new BackgroundTile();
                tile.renderer = sr;
                tile.pair = chosenPair;
                tile.isFrameOne = frameOne;
                tilesList.Add(tile);
            }
        }
    }

    private SpritePair GetWeightedRandomPair()
    {
        float totalWeight = 0f;
        foreach(SpritePair pair in spritePairs)
        {
            totalWeight += pair.weight;
        }

        float randomValue = Random.value * totalWeight;
        float cumulative = 0f;

        foreach(SpritePair pair in spritePairs)
        {
            cumulative += pair.weight;
            if(randomValue <= cumulative)
            {
                return pair;
            }
        }
        return spritePairs[0];
    }

    private IEnumerator AnimateBackground()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(animationMinDelay, animationMaxDelay));

            int maxCount = Mathf.Min(maxTilesAnimate, tilesList.Count);
            int animateCount = Random.Range(minTilesAnimate, maxCount);
            int changedCount = 0;

            for(int i = 0; i < tilesList.Count && changedCount < animateCount; i++)
            {
                if(Random.value <= animationChancePerTile)
                {
                    BackgroundTile tile = tilesList[Random.Range(0, tilesList.Count)];
                    tile.isFrameOne = !tile.isFrameOne;
                    tile.renderer.sprite = tile.isFrameOne ? tile.pair.frame1 : tile.pair.frame2;
                    changedCount++;
                }
            }
        }
    }
}