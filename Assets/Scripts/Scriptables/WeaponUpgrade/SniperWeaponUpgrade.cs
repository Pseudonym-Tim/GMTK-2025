using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "NewSniperWeaponUpgrade", menuName = GameManager.GAME_NAME + "/WeaponUpgrade/SniperWeaponUpgrade")]
public class SniperWeaponUpgrade : WeaponUpgrade
{
    [SerializeField] private float minimumChargeTime = 1f;

    private bool isCharging = false;
    private Vector2? lockedTargetPos = null;
    private SFXManager sfxManager;

    public override void Shoot(Player player)
    {
        if(isCharging)
        {
            return;
        }

        sfxManager = FindFirstObjectByType<SFXManager>();
        player.StartCoroutine(ChargeAndFire(player));
    }

    public override bool TryGetRotationLock(Player player, out float rotation)
    {
        if(isCharging && lockedTargetPos.HasValue)
        {
            Vector2 dir = lockedTargetPos.Value - player.CenterOfMass;
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rotation = angleDeg - 90f;
            return true;
        }

        rotation = 0f;
        return false;
    }

    private IEnumerator ChargeAndFire(Player playerEntity)
    {
        isCharging = true;
        sfxManager.Play2DSound("sniper_charge");

        float timer = 0f;

        while(PlayerInput.IsButtonHeld("Shoot"))
        {
            timer += Time.deltaTime;
            lockedTargetPos = FindNearestEnemyPosition(playerEntity.CenterOfMass, playerEntity.LevelManager);
            yield return null;
        }

        if(timer >= minimumChargeTime && lockedTargetPos.HasValue)
        {
            Vector2 dir = lockedTargetPos.Value - (Vector2)playerEntity.ShootPoint.position;
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0f, 0f, angleDeg - 90f);

            BulletProjectile proj = (BulletProjectile)playerEntity.LevelManager.SpawnEntity("bullet_projectile", playerEntity.ShootPoint.position, rot);
            proj.Setup(BulletProjectile.BulletOwner.PLAYER, dir.normalized);

            sfxManager.Play2DSound("sniper_fire");
        }
        else
        {
            sfxManager.Play2DSound("sniper_charge_cancel");
        }

        lockedTargetPos = null;
        isCharging = false;
    }

    private Vector2? FindNearestEnemyPosition(Vector2 origin, LevelManager levelManager)
    {
        Enemy[] enemies = levelManager.GetEntities<Enemy>().ToArray();
        Vector2? bestPos = null;
        float bestDist = float.MaxValue;

        for(int i = 0; i < enemies.Length; i++)
        {
            float distance = Vector2.Distance(origin, enemies[i].CenterOfMass);

            if(distance < bestDist)
            {
                bestDist = distance;
                bestPos = enemies[i].CenterOfMass;
            }
        }

        return bestPos;
    }
}
