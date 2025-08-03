using UnityEngine;

[CreateAssetMenu(fileName = "NewRestoreShipHealthShopItem", menuName = GameManager.GAME_NAME + "/ShopItem/RestoreShipHealthShopItem")]
public class RestoreShipHealthShopItem : ShopItem
{
    public override void Apply(Player playerEntity)
    {
        if(playerEntity != null)
        {
            playerEntity.RestoreHealthFull();
            Debug.Log("Player health restored to full!");
        }
    }
}
