using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBurstShotWeaponUpgrade", menuName = GameManager.GAME_NAME + "/WeaponUpgrade/BurstShotWeaponUpgrade")]
public class BurstShotWeaponUpgrade : WeaponUpgrade
{
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.1f;

    public override void Shoot(Player playerEntity)
    {
        playerEntity.StartCoroutine(FireBurst(playerEntity));
    }

    private IEnumerator FireBurst(Player player)
    {
        for(int i = 0; i < burstCount; i++)
        {
            SpawnBullet(player);
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void SpawnBullet(Player player)
    {
        Transform point = player.ShootPoint;
        BulletProjectile bulletProjectile = (BulletProjectile)player.LevelManager.SpawnEntity("bullet_projectile", point.position, point.rotation);
        bulletProjectile.Setup(BulletProjectile.BulletOwner.PLAYER, point.up);

        SFXManager sfxManager = FindFirstObjectByType<SFXManager>();
        sfxManager.Play2DSound("basic_shoot");
    }
}
