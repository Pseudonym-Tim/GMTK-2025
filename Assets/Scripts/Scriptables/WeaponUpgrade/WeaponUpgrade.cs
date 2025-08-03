using UnityEngine;

/// <summary>
/// Base class for weapon upgrades...
/// </summary>
public abstract class WeaponUpgrade : ScriptableObject
{
    public abstract void Shoot(Player player);

    public virtual bool TryGetRotationLock(Player player, out float rotation)
    {
        rotation = 0f;
        return false;
    }
}