﻿using System.Linq;
using UnityEngine;

public class ShootingEnemy : Enemy, IWeaponOwner
{
    [SerializeField]
    private Weapon _weapon;

    [SerializeField]
    private float _stoppingDistance;

    private Vector2 _overshootPosition;

    private Vector2 AimVector => (_target.transform.position.ToVector2() - transform.position.ToVector2()).normalized;

    protected override void Start()
    {
        _weapon.SetOwner(this);

        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!_player.IsAlive || !IsAlive)
        {
            _navigation.Stop();
            return;
        }

        if (_statusEffects.Any(x => x.OverrideNavigation))
        {
            foreach (StatusEffect x in _statusEffects)
            {
                x.Navigation();
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            bool playerVisible = PlayerIsVisible(_aggroDistance);
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

            if (!playerVisible && _hasAggro)
            {
                _overshootPosition = Vector2.zero;
                _navigation.MoveTo(_target, playerVisible);
            }

            if (_overshootPosition != Vector2.zero)
            {
                if ((_overshootPosition - transform.position.ToVector2()).magnitude < 1.0f)
                {
                    _overshootPosition = Vector2.zero;
                }
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

    protected override void Die(Vector2 velocity)
    {
        _weapon.StopShooting();
        base.Die(velocity);
    }

    Vector2 IWeaponOwner.GetAimVector()
    {
        return AimVector;
    }

    Vector2 IWeaponOwner.GetBulletOrigin()
    {
        return transform.position.ToVector2() + new Vector2(AimVector.normalized.x / 2.0f, 0.35f);
    }

    void IWeaponOwner.Knockback(Vector2 direction, float force)
    {
    }

    GameObject IWeaponOwner.GetGameObject()
    {
        return gameObject;
    }

    void IWeaponOwner.AddWeapon(GameObject weaponPrefab)
    {

    }
}
