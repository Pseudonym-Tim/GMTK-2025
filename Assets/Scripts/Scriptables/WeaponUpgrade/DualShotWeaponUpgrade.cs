using UnityEngine;

[CreateAssetMenu(fileName = "NewDualShotWeaponUpgrade", menuName = GameManager.GAME_NAME + "/WeaponUpgrade/DualShotWeaponUpgrade")]
public class DualShotWeaponUpgrade : WeaponUpgrade
{
    public override void Shoot(Player playerEntity)
    {
        Transform left = playerEntity.ShotPointLeft;
        Transform right = playerEntity.ShotPointRight;

        SpawnBullet(playerEntity, left);
        SpawnBullet(playerEntity, right);

        SFXManager sfxManager = FindFirstObjectByType<SFXManager>();
        sfxManager.Play2DSound("basic_shoot");
    }

    private void SpawnBullet(Player player, Transform point)
    {
        BulletProjectile bullet = (BulletProjectile)player.LevelManager.SpawnEntity("bullet_projectile", point.position, point.rotation);
        bullet.Setup(BulletProjectile.BulletOwner.PLAYER, point.up);
    }
}
