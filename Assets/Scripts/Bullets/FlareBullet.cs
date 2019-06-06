﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FlareBullet : Bullet
{
    [SerializeField]
    private Transform _visualTransform;

    [SerializeField]
    private Transform _shadowTransform;

    [SerializeField]
    private GameObject _signalFlarePrefab;

    public override void UpdateBullet()
    {
        _visualTransform.rotation = Quaternion.LookRotation(Vector3.forward, _direction);
        _shadowTransform.rotation = Quaternion.LookRotation(Vector3.forward, _direction);

        base.UpdateBullet();
    }

    public override void BeforeDestroyed(GameObject hitTarget)
    {
        if (hitTarget != null)
        {
            Instantiate(_signalFlarePrefab, transform.position, _visualTransform.rotation, hitTarget.transform);
        }
        else
        {
            Instantiate(_signalFlarePrefab, transform.position, _visualTransform.rotation);
        }
    }
}