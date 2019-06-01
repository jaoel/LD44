﻿using System.Collections;
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

    private int _livesLeft;

    protected override void Awake()
    {
        base.Awake();

        _livesLeft = _lives;
    }

    protected override void Die(Vector2 velocity)
    {
        _livesLeft--;

        if (_livesLeft > 0)
        {
            _dieDirection = velocity;
            _rigidbody.velocity = _dieDirection.normalized * 2.5f;
            _minimapIcon.SetActive(false);

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
        _minimapIcon.SetActive(true);
        _currentHealth = _maxHealth;
        SoundManager.Instance.PlayMonsterAggro();
    }
}