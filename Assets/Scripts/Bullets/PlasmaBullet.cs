﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class PlasmaBullet : Bullet
{
    public int splitCount;
    public int maxSize;
    public int minSize;
    public int minSpeed;

    [SerializeField]
    private int _bulletsPerSplit;

    [SerializeField]
    private Bullet _plasmaBulletPrefab;

    [SerializeField]
    private CircleCollider2D _collider;

    private Vector2 _reflectionNormal;
    private Vector2 _lastPosition;
    private int _startSplitCount;

    private float _splitInterval;
    private float _splitTimer;

    protected override void Awake()
    {
        _startSplitCount = splitCount;
        _splitInterval = 0.5f;

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Initialize(float charge, Vector2 direction, GameObject owner, bool superCharged)
    {
        base.Initialize(charge, direction, owner, superCharged);

        _collider.radius = Utility.ConvertRange(0.0f, 1.0f, 0.075f, 0.3f, _charge);
        float size = Utility.ConvertRange(0.0f, 1.0f, minSize, maxSize, _charge);
        SetSize(new Vector2(size, size));

        _currentSpeed = Utility.ConvertRange(0.0f, 1.0f, minSpeed, _currentSpeed, 1.0f - _charge);
        _currentDamage = Utility.ConvertRange(0.0f, 1.0f, 0.0f, _damage, _charge);
        _currentLifetime = Utility.ConvertRange(0.0f, 1.0f, 0.0f, _lifetime, _charge);

        if (_superCharged)
        {
            SetTint(Color.red);
        }
    }

    public override void UpdateBullet()
    {
        _lastPosition = transform.position;
        _direction = Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(-7.5f, 7.5f)) * _direction;
        _direction.Normalize();

        base.UpdateBullet();


        if (_superCharged)
        {
            _splitTimer += Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (_superCharged)
        {
            if (_splitTimer >= _splitInterval)
            {
                _splitTimer = 0.0f;
                SplitBullet(transform.position, true);
                splitCount++;
            }
        }
    }

    public override void BeforeDestroyed(GameObject hitTarget)
    {
        splitCount = _startSplitCount;
        base.BeforeDestroyed(hitTarget);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner)
        {
            return;
        }

        bool active = true;

        if (collision.gameObject.layer == Layers.Map)
        {
              RaycastHit2D hit = Physics2D.Raycast(_lastPosition, _direction, 4.0f,
                Layers.CombinedLayerMask(Layers.Map));
            
            _reflectionNormal = hit.normal;
            SplitBullet(hit.point, false);

            BeforeDestroyed(collision.gameObject);
            active = false;
        }
        else if (collision.gameObject.layer == Layers.Enemy || collision.gameObject.layer == Layers.FlyingEnemy)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy.IsAlive)
            {
                collision.gameObject.GetComponent<Enemy>().ReceiveDamage((int)_currentDamage, _direction);
                BeforeDestroyed(collision.gameObject);

                SplitBullet(transform.position, true);

                active = false;
            }
        }

        gameObject.SetActive(active);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _reflectionNormal = Vector2.zero;
    }

    private bool SplitBullet(Vector2 hitPosition, bool splitInCircle)
    {
        bool performedSplit = false;
        if (splitCount > 0)
        {
            splitCount--;

            for(int i = 0; i < _bulletsPerSplit; i++)
            {
                Vector2 direction = Vector2.zero;
                if (splitInCircle)
                {
                    direction = UnityEngine.Random.onUnitSphere.ToVector2();
                }
                else
                {
                    float angle = UnityEngine.Random.Range(-45.0f, 45.0f);
                    direction = Quaternion.Euler(0.0f, 0.0f, angle) * _reflectionNormal;
                }
                
                PlasmaBullet newBullet = (PlasmaBullet)BulletManager.Instance.SpawnBullet(_plasmaBulletPrefab, _lastPosition, 
                    direction.normalized, _charge * 0.66f, _owner, false);
                newBullet.splitCount = splitCount;
            }
            performedSplit = true;
        }

        return performedSplit;
    }
}
