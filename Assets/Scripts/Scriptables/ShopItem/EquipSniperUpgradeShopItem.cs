using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipSniperUpgradeShopItem", menuName = GameManager.GAME_NAME + "/ShopItem/EquipSniperUpgradeShopItem")]
public class EquipSniperUpgradeShopItem : ShopItem
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
