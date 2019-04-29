using UnityEngine;

public class ShootingEnemy : Enemy
{
    public BulletDescription bulletDescription;

    public float shotTimer = float.MaxValue;
    public float reloadTimer = float.MaxValue;
    public int shotsFired = 0;
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

        if (shotsFired > description.magazineSize)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= description.reloadTime)
            {
                shotsFired = 0;
                reloadTimer = 0;
            }
        } 
        
        if (IsAlive &&PlayerIsVisible() && shotTimer > description.shotCooldown && shotsFired <= description.magazineSize)
        {
            Shoot();
        }  

        base.FixedUpdate();
    }

    protected virtual void Shoot()
    {
        SoundManager.Instance.PlayShotSound(false);

        Vector3 dirToPlayer = (_player.transform.position - transform.position).normalized;
        dirToPlayer.z = 0.0f;
        BulletManager.Instance.SpawnBullet(bulletDescription, transform.position, dirToPlayer * bulletSpeed, 
            gameObject);
        shotTimer = 0.0f;
        shotsFired++;
    }

    protected override bool PlayAttackAnimation() {
        return Vector2.Distance(_target, transform.position) < _stoppingDistance;
    }
} 
