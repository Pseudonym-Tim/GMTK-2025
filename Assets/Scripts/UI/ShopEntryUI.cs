using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Holds info for shop entries and handles their selection...
/// </summary>
public class ShopEntryUI : UIComponent
{
    private const float INFO_PANEL_OFFSET = 50f;

    public TextMeshProUGUI nameText;
    public Image iconImage;
    public Image selectionOverlayImage;

    [Header("Info Panel")]
    public Transform infoPanel;
    public TextMeshProUGUI infoDescriptionText;
    public TextMeshProUGUI infoPanelCostText;

    private Vector3 infoPanelDefaultPosition;
    private Vector3 infoPanelTargetPosition;
    private ShopItem currentShopItem;

    public void Setup(ShopItem shopItem, bool isSelected)
    {
        currentShopItem = shopItem;
        nameText.text = shopItem.itemName;
        iconImage.sprite = shopItem.icon;
        infoDescriptionText.text = shopItem.description;
        infoPanelCostText.text = shopItem.GetCost().ToString() + "$";
        selectionOverlayImage.enabled = isSelected;
        infoPanelDefaultPosition = infoPanel.localPosition;
        infoPanelTargetPosition = infoPanelDefaultPosition + Vector3.up * INFO_PANEL_OFFSET;
        infoPanel.localScale = new Vector3(0f, 1f, 1f);
        infoPanel.localPosition = infoPanelDefaultPosition;
    }

    public void SetHovered(bool isHovered)
    {
        selectionOverlayImage.enabled = isHovered;
        StopAllCoroutines();
        StartCoroutine(AnimateInfoPanel(isHovered, 0.25f));
    }

    private IEnumerator AnimateInfoPanel(bool isOpen, float duration)
    {
        Vector3 startScale = infoPanel.localScale;
        Vector3 endScale = isOpen ? new Vector3(1f, 1f, 1f) : new Vector3(0f, 1f, 1f);
        Vector3 startPos = infoPanel.localPosition;
        Vector3 endPos = isOpen ? infoPanelTargetPosition : infoPanelDefaultPosition;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;
            infoPanel.localScale = Vector3.Lerp(startScale, endScale, t);
            infoPanel.localPosition = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        infoPanel.localScale = endScale;
        infoPanel.localPosition = endPos;
    }
}
