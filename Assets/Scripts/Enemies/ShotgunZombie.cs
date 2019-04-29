
using UnityEngine;

public class ShotgunZombie : ShootingEnemy
{
    protected override void Shoot()
    {
        SoundManager.Instance.PlayShotSound(false);

        Vector3 dirToPlayer = (_player.transform.position - transform.position).normalized;
        dirToPlayer.z = 0.0f;
        BulletManager.Instance.SpawnBullet(bulletDescription, transform.position, dirToPlayer * bulletSpeed,
            gameObject);
        BulletManager.Instance.SpawnBullet(bulletDescription, transform.position, Quaternion.Euler(0, 0, 20) * dirToPlayer * bulletSpeed,
           gameObject);
        BulletManager.Instance.SpawnBullet(bulletDescription, transform.position, Quaternion.Euler(0, 0, -20) * dirToPlayer * bulletSpeed,
           gameObject);

        shotTimer = 0.0f;
        shotsFired++;
    }
} 
