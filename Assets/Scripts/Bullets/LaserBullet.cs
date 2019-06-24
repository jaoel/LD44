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
    public int splitCount;
    public int pierceCount;

    [SerializeField]
    private int _bulletsPerSplit;

    [SerializeField]
    private Bullet _laserBulletPrefab;

    [SerializeField]
    private TrailRenderer _trail;

    [SerializeField]
    private CapsuleCollider2D _capsule;

    private Vector2 _reflectionNormal;
    private int _startSplitCount;
    private int _startPierceCount;
    protected override void Awake()
    {
        _startSplitCount = splitCount;
        _startPierceCount = pierceCount;

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Initialize(float charge, Vector2 direction, GameObject owner)
    {
        base.Initialize(charge, direction, owner);

        _charge = Mathf.Max(_charge, 0.3f);
        _currentLifetime *= _charge;
        _currentDamage *= _charge;
    }

    public void WallCollisionHandling()
    {
        _capsule.enabled = false;
        StartCoroutine(EnableCollider());
    }

    private IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(0.1f);

        _capsule.enabled = true;

        yield return null;
    }

    public override void UpdateBullet()
    {
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
        splitCount = _startSplitCount;
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
            RaycastHit2D hit = Physics2D.Raycast(transform.position.ToVector2(), _direction, 1.0f, 
                Layers.CombinedLayerMask(Layers.Map));

            _reflectionNormal = hit.normal;
            _direction = Vector2.Reflect(_direction.normalized, _reflectionNormal);

            SplitBullet(hit.point);
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
            }
        }

        gameObject.SetActive(active);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Layers.Map)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position.ToVector2(), _direction, 1.0f,
                Layers.CombinedLayerMask(Layers.Map));

            if (_reflectionNormal != hit.normal)
            {
                _reflectionNormal = hit.normal;
                _direction = Vector2.Reflect(_direction.normalized, _reflectionNormal);

                SplitBullet(hit.point);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _reflectionNormal = Vector2.zero;
    }

    private bool SplitBullet(Vector2 hitPosition)
    {
        bool performedSplit = false;
        if (splitCount > 0)
        {
            splitCount--;

            for(int i = 0; i < _bulletsPerSplit; i++)
            {
                float angle = UnityEngine.Random.Range(-45.0f, 45.0f);
                Vector2 direction = Quaternion.Euler(0.0f, 0.0f, angle) * _reflectionNormal;
                LaserBullet newBullet = (LaserBullet)BulletManager.Instance.SpawnBullet(_laserBulletPrefab, 
                    hitPosition + direction.normalized * 0.2f, direction, _charge, _owner);
                newBullet.splitCount = splitCount;
                newBullet.SetOwner(null);
                newBullet._currentDamage = _currentDamage * 0.2f;
                newBullet.pierceCount = pierceCount - 1;
                newBullet.WallCollisionHandling();
            }
            performedSplit = true;
        }

        WallCollisionHandling();
        return performedSplit;
    }
}
