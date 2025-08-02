using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles everything related to the shop UI...
/// </summary>
public class ShopUI : UIComponent
{
    [SerializeField] private Transform AnimationRoot;
    [SerializeField] private Transform itemEntryParent;
    [SerializeField] private ShopEntryUI shopEntryUIPrefab;
    [SerializeField] private TextMeshProUGUI pointsRemainingText;
    [SerializeField] private TextMeshProUGUI shopMessageText;

    private List<ShopEntryUI> shopEntryUIList = new List<ShopEntryUI>();
    private int selectedIndex;
    private ShopManager shopManager;
    private FadeUI fadeUI;

    public override void SetupUI()
    {
        fadeUI = UIManager.GetUIComponent<FadeUI>();
        shopManager = FindFirstObjectByType<ShopManager>();
        Show(false);
    }

    public override void Show(bool showUI = true)
    {
        IsInteractionEnabled = false;
        UICanvas.enabled = showUI;

        if(showUI)
        {
            fadeUI.FadeIn();
            AnimationRoot.localScale = Vector3.zero;
            AnimationRoot.localRotation = Quaternion.Euler(0f, 0f, 0f);
            StartCoroutine(AnimateTransformOpen(0.5f));
        }
    }

    private void Update()
    {
        if(!IsShown || !IsInteractionEnabled)
        {
            return;
        }

        if(InputManager.IsButtonPressed("NavigateLeft"))
        {
            SelectPrevious();
        }
        else if(InputManager.IsButtonPressed("NavigateRight"))
        {
            SelectNext();
        }
        else if(InputManager.IsButtonPressed("SelectOption") && !fadeUI.IsFading)
        {
            shopManager.PurchaseItem(selectedIndex);
        }
    }

    private void SelectPrevious()
    {
        shopEntryUIList[selectedIndex].SetHovered(false);
        selectedIndex = (selectedIndex + shopEntryUIList.Count - 1) % shopEntryUIList.Count;
        shopEntryUIList[selectedIndex].SetHovered(true);
    }

    private void SelectNext()
    {
        shopEntryUIList[selectedIndex].SetHovered(false);
        selectedIndex = (selectedIndex + 1) % shopEntryUIList.Count;
        shopEntryUIList[selectedIndex].SetHovered(true);
    }

    public void UpdateRemainingPoints(int currentPointAmount)
    {
        string pointsRemainingMessage = TextHandler.GetText("shopPointsText", "shop_ui");
        pointsRemainingMessage = pointsRemainingMessage.Replace("%currentPoints%", currentPointAmount.ToString("N0"));
        pointsRemainingText.text = pointsRemainingMessage;
    }

    public void UpdateShopMessage(string messageID)
    {
        string scoreMessage = TextHandler.GetText(messageID, "shop_ui");
        shopMessageText.text = scoreMessage;
    }

    public void DisplayShopItems(List<ShopItem> items)
    {
        foreach(ShopEntryUI shopEntryUI in shopEntryUIList)
        {
            shopEntryUI.DestroyUI();
        }

        shopEntryUIList.Clear();

        for(int i = 0; i < items.Count; i++)
        {
            ShopEntryUI entryUI = Instantiate(shopEntryUIPrefab, itemEntryParent);
            entryUI.Setup(items[i], i == 0);
            shopEntryUIList.Add(entryUI);
        }

        selectedIndex = 0; 
        
        if(shopEntryUIList.Count > 0)
        {
            shopEntryUIList[0].SetHovered(true);
        }
    }

    public void UpdateShopItemCostText(int index, string text)
    {
        if(index >= 0 && index < shopEntryUIList.Count)
        {
            ShopEntryUI entry = shopEntryUIList[index];

            if(entry != null && entry.infoPanelCostText != null)
            {
                entry.infoPanelCostText.text = text;
            }
        }
    }

    private IEnumerator AnimateTransformOpen(float duration)
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;
            AnimationRoot.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            AnimationRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -360f, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        AnimationRoot.localScale = Vector3.one;
        AnimationRoot.localRotation = Quaternion.Euler(0f, 0f, -360f);

        IsInteractionEnabled = true;
    }

    public bool IsInteractionEnabled { get; set; } = false;
}
