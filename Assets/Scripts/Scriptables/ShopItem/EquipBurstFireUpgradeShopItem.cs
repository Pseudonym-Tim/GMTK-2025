using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipBurstFireUpgradeShopItem", menuName = GameManager.GAME_NAME + "/ShopItem/EquipBurstFireUpgradeShopItem")]
public class EquipBurstFireUpgradeShopItem : ShopItem
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
