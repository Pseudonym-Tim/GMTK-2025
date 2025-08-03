using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipShotgunUpgradeShopItem", menuName = GameManager.GAME_NAME + "/ShopItem/EquipShotgunUpgradeShopItem")]
public class EquipShotgunUpgradeShopItem : ShopItem
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
