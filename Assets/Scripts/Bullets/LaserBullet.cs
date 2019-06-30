using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class LaserBullet : Bullet
{
    public int pierceCount;


    [SerializeField]
    private TrailRenderer _trail;

    [SerializeField]
    private CapsuleCollider2D _capsule;

    private Vector2 _reflectionNormal;
    private Vector2 _lastPosition;
    private int _startPierceCount;
    protected override void Awake()
    {
        _startPierceCount = pierceCount;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Initialize(float charge, Vector2 direction, GameObject owner, bool superCharged)
    {
        base.Initialize(charge, direction, owner, superCharged);
    }

    public override void UpdateBullet()
    {
        _lastPosition = transform.position;

        if (_currentLifetime <= _trail.time)
        {
            _trail.emitting = false;
        }

        float angle = Vector2.SignedAngle(Vector2.up, _direction);
        _capsule.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);

        base.UpdateBullet();
    }

    public override void BeforeDestroyed(GameObject hitTarget)
    {
        _capsule.enabled = true;
        _trail.Clear();
        _trail.emitting = true;
        pierceCount = _startPierceCount;

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
            RaycastHit2D hit = Physics2D.Raycast(_lastPosition, _direction, 1.0f,
                Layers.CombinedLayerMask(Layers.Map));
            
            _reflectionNormal = hit.normal;
            _direction = Vector2.Reflect(_direction.normalized, _reflectionNormal);
            transform.position = _lastPosition;
        }
        else if (collision.gameObject.layer == Layers.Enemy || collision.gameObject.layer == Layers.FlyingEnemy)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy.IsAlive)
            {
                collision.gameObject.GetComponent<Enemy>().ReceiveDamage((int)_currentDamage, _direction);

                if (pierceCount <= 0)
                {
                    BeforeDestroyed(collision.gameObject);
                    active = false;
                }

                pierceCount--;
                _currentDamage -= _currentDamage * 0.33f;
            }
        }

        gameObject.SetActive(active);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Layers.Map)
        {
            RaycastHit2D hit = Physics2D.Raycast(_lastPosition, _direction, 1.0f,
                Layers.CombinedLayerMask(Layers.Map));
            
            if (_reflectionNormal != hit.normal)
            {
                _reflectionNormal = hit.normal;
                _direction = Vector2.Reflect(_direction.normalized, _reflectionNormal);
                transform.position = _lastPosition;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _reflectionNormal = Vector2.zero;
    }
}
