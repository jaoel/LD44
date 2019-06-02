using UnityEngine;

public class ShootingEnemy : Enemy, IWeaponOwner
{
    [SerializeField]
    private Weapon _weapon;

    [SerializeField]
    private float _stoppingDistance;

    private Vector2 _overshootPosition;

    private Vector2 AimVector => (_target.transform.position.ToVector2() - transform.position.ToVector2()).normalized;

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
            if (_rigidbody.velocity.magnitude > 0.1f)
            {
                _characterAnimation.UpdateAnimation(CharacterAnimation.AnimationType.Run, AimVector);
            }
            else
            {
                _characterAnimation.UpdateAnimation(CharacterAnimation.AnimationType.Attack, AimVector);
            }

            if (_overshootPosition == Vector2.zero || (_overshootPosition - _target.transform.position.ToVector2()).magnitude > _aggroDistance / 2.0f)
            {
                _overshootPosition = Utility.RandomPointOnCircleEdge(_aggroDistance) + _target.transform.position.ToVector2();
            }
            _navigation.MoveTo(_overshootPosition, true);
        }

        if (!playerVisible)
        {
            _overshootPosition = Vector2.zero;
        }

        if (_overshootPosition != Vector2.zero)
        {
            if ((_overshootPosition - transform.position.ToVector2()).magnitude < 1.0f)
            {
                _overshootPosition = Vector2.zero;
            }
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
        return AimVector;
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
