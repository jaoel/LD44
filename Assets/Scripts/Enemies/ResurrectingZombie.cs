using System.Collections;
using UnityEngine;

public class ResurrectingZombie : ResurrectingEnemy
{
    [SerializeField]
    protected float _resurrectedMaxSpeed;

    [SerializeField]
    protected float _resurrectedMaxHealth;

    [SerializeField]
    protected int _resurrectedDamage;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Resurrection()
    {
        _maxSpeed = _resurrectedMaxSpeed;
        _maxHealth = _resurrectedMaxHealth;

        _navigation.Initialize(_rigidbody, _maxSpeed, _acceleration * 2.0f);
        _visual.color = Color.red;

        base.Resurrection();
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (IsAlive && collision.gameObject.layer == Layers.Player)
        {
            int damage = _livesLeft < _lives ? _resurrectedDamage : (int)_meleeDamage;

            if (_player.ReceiveDamage(damage, -collision.contacts[0].normal))
            {
                _rigidbody.velocity = Vector2.zero;
            }
        }
    }
}
