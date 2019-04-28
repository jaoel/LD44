using UnityEngine;

public class Shotgun : Weapon
{
    public override void Shoot(Vector2 aimVector, Vector2 position, GameObject owner)
    {
        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position, 
            Quaternion.Euler(0, 0, 20) * aimVector * Description.BulletSpeed, owner);
        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position, 
            aimVector * Description.BulletSpeed, owner);
        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position, 
            Quaternion.Euler(0, 0, -20) * aimVector * Description.BulletSpeed, owner);

    }  
}
