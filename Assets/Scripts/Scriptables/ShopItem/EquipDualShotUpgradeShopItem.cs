using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipDualShotUpgradeShopItem", menuName = GameManager.GAME_NAME + "/ShopItem/EquipDualShotUpgradeShopItem")]
public class EquipDualShotUpgradeShopItem : ShopItem
{
    [SerializeField] private WeaponUpgrade weaponUpgrade;

    public override void Apply(Player playerEntity)
    {
        if(playerEntity != null)
        {
            playerEntity.EquipWeaponUpgrade(weaponUpgrade);
        }
    }
}
