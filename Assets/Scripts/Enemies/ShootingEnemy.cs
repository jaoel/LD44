using UnityEngine;

public class ShootingEnemy : Enemy, IWeaponOwner
{
    [SerializeField]
    private Weapon _weapon;

    [SerializeField]
    private float _stoppingDistance;
    //public BulletDescription bulletDescription;
    //
    //public float shotTimer = float.MaxValue;
    //public float reloadTimer = float.MaxValue;
    //public int shotsFired = 0;
    //public float bulletSpeed;

    protected override void Awake()
    {
        _weapon.SetOwner(this);

        base.Awake();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!_player.IsAlive || !IsAlive)
        {
            _navigation.Stop();
            return;
        }

        float distance = Vector3.Distance(transform.position, _player.transform.position);
        bool playerVisible = PlayerIsVisible();
        if (distance <= _aggroDistance && playerVisible)
        {
            _weapon.Shoot();
        }

        if (distance <= _stoppingDistance && playerVisible)
        {
            _navigation.Stop();
        }
    }

    protected override bool PlayAttackAnimation()
    {
        if (_target == null)
        {
            return false;
        }

        return Vector2.Distance(_target.transform.position.ToVector2(), transform.position.ToVector2()) < _stoppingDistance;
    }

    Vector2 IWeaponOwner.GetAimVector()
    {
        return (_target.transform.position.ToVector2() - transform.position.ToVector2()).normalized;
    }

    Vector2 IWeaponOwner.GetBulletOrigin()
    {
        return transform.position.ToVector2();
    }

    void IWeaponOwner.Knockback(Vector2 direction, float force)
    {
    }

    GameObject IWeaponOwner.GetGameObject()
    {
        return gameObject;
    }
}
