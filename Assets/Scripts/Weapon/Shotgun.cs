using UnityEngine;

public class Shotgun : Weapon
{
    public override void Shoot(Vector2 aimVector, Vector2 position, GameObject owner)
    {
        SoundManager.Instance.PlaySound(description.soundPrefab, description.loopSound);

        BulletManager.Instance.SpawnBullet(description.bulletDescription, position + aimVector.normalized * 0.3f, 
            Quaternion.Euler(0, 0, 20) * aimVector * description.bulletSpeed, owner);
        BulletManager.Instance.SpawnBullet(description.bulletDescription, position + aimVector.normalized * 0.3f, 
            aimVector * description.bulletSpeed, owner);
        BulletManager.Instance.SpawnBullet(description.bulletDescription, position + aimVector.normalized * 0.3f, 
            Quaternion.Euler(0, 0, -20) * aimVector * description.bulletSpeed, owner);

    }  
}
