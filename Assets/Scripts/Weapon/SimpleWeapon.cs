using UnityEngine;

public class SimpleWeapon : Weapon
{
    public override void Shoot(Vector2 aimVector, Vector2 position, GameObject owner)
    {
        if (description.loopSound)
        {
            if (_shotSound == null)
            {
                _shotSound = SoundManager.Instance.PlaySound(description.soundPrefab, description.loopSound);
            }
        }
        else
        {
            SoundManager.Instance.PlaySound(description.soundPrefab, description.loopSound);
        }

        BulletManager.Instance.SpawnBullet(description.bulletDescription, position + aimVector.normalized * 0.3f, 
            aimVector * description.bulletSpeed, owner);
    }
} 
