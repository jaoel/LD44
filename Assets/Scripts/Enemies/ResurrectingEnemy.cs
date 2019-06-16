using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResurrectingEnemy : Enemy
{
    [SerializeField]
    protected int _lives;

    [SerializeField]
    protected float _minResurrectionTime;

    [SerializeField]
    protected float _maxResurrectionTime;

    protected int _livesLeft;

    protected override void Awake()
    {
        _livesLeft = _lives;

        base.Awake();
    }

    protected override void Die(Vector2 velocity)
    {
        _livesLeft--;

        if (_livesLeft > 0)
        {
            _dieDirection = velocity;
            _rigidbody.velocity = _dieDirection.normalized * 2.5f;

            StartCoroutine(Resurrect());
        }
        else
        {
            base.Die(velocity);
        }
    }

    protected virtual IEnumerator Resurrect()
    {
        yield return new WaitForSeconds(Random.Range(_minResurrectionTime, _maxResurrectionTime));

        Resurrection();
    }

    protected virtual void Resurrection()
    {
        _currentHealth = _maxHealth;
        SoundManager.Instance.PlayMonsterAggro();
    }
}
