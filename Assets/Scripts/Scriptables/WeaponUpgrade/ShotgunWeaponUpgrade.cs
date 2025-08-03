using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShotgunWeaponUpgrade", menuName = GameManager.GAME_NAME + "/WeaponUpgrade/ShotgunWeaponUpgrade")]
public class ShotgunWeaponUpgrade : WeaponUpgrade
{
    [SerializeField] private int shotCount = 3;
    [SerializeField] private float spreadAngle = 45f;

    public override void Shoot(Player playerEntity)
    {
        Transform point = playerEntity.ShootPoint;
        Quaternion baseRot = point.rotation;

        for(Int32 i = 0; i < shotCount; i++)
        {
            float angle = -spreadAngle / 2f + spreadAngle * i / (shotCount - 1);
            Quaternion rot = baseRot * Quaternion.AngleAxis(angle, Vector3.forward);
            BulletProjectile pellet = (BulletProjectile)playerEntity.LevelManager.SpawnEntity("bullet_projectile", point.position, rot);
            pellet.Setup(BulletProjectile.BulletOwner.PLAYER, rot * Vector3.up);
        }
    }
}
