using System.Collections;
using UnityEngine;

public class ResurrectingZombie : ResurrectingEnemy
{
    [SerializeField]
    protected float _resurrectedMaxSpeed;

    [SerializeField]
    protected float _resurrectedMaxHealth;

    [SerializeField]
    protected SpriteRenderer _visual;
    
    protected override void Resurrection()
    {
        _maxSpeed = _resurrectedMaxSpeed;
        _maxHealth = _resurrectedMaxHealth;

        _navigation.Initialize(_rigidbody, _maxSpeed, _acceleration * 2.0f);
        _visual.color = Color.red;

        base.Resurrection();
    }
}
