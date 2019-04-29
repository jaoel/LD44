using UnityEngine;

public class SimpleWeapon : Weapon
{
    public override void Shoot(Vector2 aimVector, Vector2 position, GameObject owner)
    {
        if (Description.loopSound)
        {
            if (_shotSound == null)
                _shotSound = SoundManager.Instance.PlaySound(Description.soundPrefab, Description.loopSound);
        }
        else
        {
            SoundManager.Instance.PlaySound(Description.soundPrefab, Description.loopSound);
        }

        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position + aimVector.normalized * 0.3f, 
            aimVector * Description.BulletSpeed, owner);
    }
} 
