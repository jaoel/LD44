using UnityEngine;

public class Shotgun : Weapon
{
    public override void Shoot(Vector2 aimVector, Vector2 position)
    {
        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position, Quaternion.Euler(0, 0, 20) * aimVector * Description.BulletSpeed);
        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position, aimVector * Description.BulletSpeed);
        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position, Quaternion.Euler(0, 0, -20) * aimVector * Description.BulletSpeed);

    }  
}
