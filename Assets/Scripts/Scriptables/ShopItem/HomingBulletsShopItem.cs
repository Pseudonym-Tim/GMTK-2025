using UnityEngine;

[CreateAssetMenu(fileName = "NewHomingBulletsShopItem", menuName = GameManager.GAME_NAME + "/ShopItem/HomingBulletsShopItem")]
public class HomingBulletsShopItem : ShopItem
{
    [Range(0f, 1f)] public float homingStrength = 0.1f;

    public override void Apply(Player playerEntity)
    {
        if(playerEntity != null)
        {
            playerEntity.HasHomingBullets = true;
            playerEntity.HomingStrength = homingStrength;
        }
    }
}