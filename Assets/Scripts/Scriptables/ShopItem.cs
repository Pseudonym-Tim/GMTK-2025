using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = GameManager.GAME_NAME + "/ShopItem")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea(3, 3)] public string description;
    public bool scaleWithPoints;
    [Range(0f, 1f)] public float costPercentage = 0.25f;
    public int baseCost = 100;
    public int minCost = 1;
    
    public int GetCost()
    {
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        int currentPoints = scoreManager.CurrentScore;
        int itemCost = 0;

        if(scaleWithPoints)
        {
            itemCost = Mathf.RoundToInt(currentPoints * costPercentage);
        }
        else
        {
            itemCost = baseCost;
        }

        return Mathf.Max(minCost, itemCost);
    }

    public void Apply()
    {
        // TODO: Implement the actual effect of this item/upgrade/weapon...
        Debug.Log("Use shop item!");
    }
}
