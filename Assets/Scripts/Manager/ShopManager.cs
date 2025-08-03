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
        shopUI.UpdateShopMessage("shopIntroductionText");
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

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        Player playerEntity = levelManager.GetEntity<Player>();

        if(scoreManager.CurrentScore >= costAmount)
        {
            scoreManager.RemoveScore(costAmount);
            shopItem.Apply(playerEntity);

            shopUI.UpdateRemainingPoints(scoreManager.CurrentScore);
            shopUI.UpdateShopItemCostText(index, TextHandler.GetText("shopInfoBoughtText", "shop_ui"));
            shopUI.UpdateShopMessage("shopPurchaseSuccessText");

            shopUI.IsInteractionEnabled = false;
            FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
            fadeUI.FadeOut();
            StartCoroutine(CloseStoreAfterDelay(FadeUI.DEFAULT_FADE_TIME));
        }
        else
        {
            shopUI.UpdateShopMessage("shopNotEnoughPoints");
        }
    }

    private IEnumerator CloseStoreAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShopUI shopUI = UIManager.GetUIComponent<ShopUI>();
        shopUI.Show(false);
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI.FadeIn();
        EnemyWaveManager enemyWaveManager = FindFirstObjectByType<EnemyWaveManager>();
        enemyWaveManager.OnShopSelectionComplete();
    }

    public IReadOnlyList<ShopItem> CurrentItems
    {
        get
        {
            return currentItems;
        }
    }
}
