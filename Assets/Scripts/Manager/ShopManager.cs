using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to the shop system...
/// </summary>
public class ShopManager : Singleton<ShopManager>
{
    private const int ITEMS_TO_SHOW = 3;

    [SerializeField] private List<ShopItem> shopItemsList = new List<ShopItem>();

    private List<ShopItem> currentItems = new List<ShopItem>();

    private ScoreManager scoreManager;

    private void Awake()
    {
        scoreManager = FindAnyObjectByType<ScoreManager>();
    }

    public void OpenShop()
    {
        GenerateShopItems();
        ShopUI shopUI = UIManager.GetUIComponent<ShopUI>();
        shopUI.Show(true);
        shopUI.UpdateRemainingPoints(scoreManager.CurrentScore);
        shopUI.DisplayShopItems(currentItems);
    }

    private void GenerateShopItems()
    {
        currentItems.Clear();
        List<ShopItem> pool = new List<ShopItem>(shopItemsList);

        for(int i = 0; i < ITEMS_TO_SHOW && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            currentItems.Add(pool[index]);
            pool.RemoveAt(index);
        }
    }

    public void PurchaseItem(int index)
    {
        if(index < 0 || index >= currentItems.Count)
        {
            return;
        }

        ShopItem shopItem = currentItems[index];
        int costAmount = shopItem.GetCost();
        ShopUI shopUI = UIManager.GetUIComponent<ShopUI>();

        if(scoreManager.CurrentScore >= costAmount)
        {
            scoreManager.RemoveScore(costAmount);
            shopItem.Apply();

            shopUI.UpdateRemainingPoints(scoreManager.CurrentScore);
            shopUI.UpdateShopItemCostText(index, TextHandler.GetText("shopInfoBoughtText", "shop_ui"));
            shopUI.UpdateShopMessage(TextHandler.GetText("shopPurchaseSuccessText", "shop_ui"));

            shopUI.IsInteractionEnabled = false;
            StartCoroutine(CloseStoreAfterDelay(2f));
        }
        else
        {
            shopUI.UpdateShopMessage(TextHandler.GetText("shopNotEnoughPoints", "shop_ui"));
        }
    }

    private IEnumerator CloseStoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShopUI shopUI = UIManager.GetUIComponent<ShopUI>();
        shopUI.Show(false);
    }

    public IReadOnlyList<ShopItem> CurrentItems
    {
        get
        {
            return currentItems;
        }
    }
}
