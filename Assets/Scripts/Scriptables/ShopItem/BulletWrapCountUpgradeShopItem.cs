using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletWrapCountUpgradeShopItem", menuName = GameManager.GAME_NAME + "/ShopItem/BulletWrapCountUpgradeShopItem")]
public class BulletWrapCountUpgradeShopItem : ShopItem
{
    public int wrapCountIncrease = 1;

    public override void Apply(Player playerEntity)
    {
        if(playerEntity != null)
        {
            playerEntity.BulletWrapUpgradeCount += wrapCountIncrease;
        }
    }
}
