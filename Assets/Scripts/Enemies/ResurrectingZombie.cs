using System.Collections;
using UnityEngine;

public class ResurrectingZombie : ResurrectingEnemy
{
    [SerializeField]
    protected float _resurrectedMaxSpeed;

    [SerializeField]
    protected float _resurrectedMaxHealth;
    
    protected override void Resurrection()
    {
        _maxSpeed = _resurrectedMaxSpeed;
        _maxHealth = _resurrectedMaxHealth;

        _navigation.Initialize(_rigidbody, _maxSpeed, _acceleration * 2.0f);

        base.Resurrection();
    }
}
