using UnityEngine;

public class SimpleWeapon : Weapon
{
    public override void Shoot(Vector2 aimVector, Vector2 position, GameObject owner)
    {
        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position, 
            aimVector * Description.BulletSpeed, owner);
    }
} 
