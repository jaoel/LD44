using UnityEngine;

public class ShootingEnemy : Enemy
{
    public BulletDescription bulletDescription;

    public float shotTimer = float.MaxValue;
    public float bulletSpeed;

    protected override void Awake()
    {
        _stoppingDistance = 3.0f;

        base.Awake();
    }

    protected override void FixedUpdate()
    {
        if (!_player.IsAlive)
            return;

        shotTimer += Time.deltaTime;
        if (PlayerIsVisible() && shotTimer > description.shotCooldown)
            Shoot();

        base.FixedUpdate();
    }

    private void Shoot()
    {
        Vector3 dirToPlayer = (_player.transform.position - transform.position);
        dirToPlayer.z = 0.0f;
        BulletManager.Instance.SpawnBullet(bulletDescription, transform.position, dirToPlayer.normalized * bulletSpeed, 
            gameObject);
        shotTimer = 0.0f;
    }
} 
