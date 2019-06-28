using System;
using DG.Tweening;
using UnityEngine;

public class SlingshotBullet : Bullet
{
    [SerializeField]
    private Transform _visualTransform;

    [SerializeField]
    private Transform _shadowTransform;

    [SerializeField]
    private float _directionOffsetDeg;

    private float _rotation = 0f;
    private float _halfAngle;

    protected override void Start()
    {
        base.Start();
    }

    public override void Initialize(float charge, Vector2 direction, GameObject owner, bool superCharged)
    {
        base.Initialize(charge, direction, owner, superCharged);

        _charge = Mathf.Max(_charge, 0.5f);
        _currentLifetime *= _charge;
        _rotation = UnityEngine.Random.Range(0.0f, 360.0f);
        _halfAngle = _directionOffsetDeg / 2.0f;

        if (!_superCharged)
        {
            _direction = Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(-_halfAngle, _halfAngle)) * _direction;
        }
        else
        {
            _currentDamage *= 1.4f;
            _currentSpeed *= 1.2f;
        }
    }

    public override void UpdateBullet()
    {
        _rotation += 300.0f * Time.deltaTime;
        _visualTransform.rotation = Quaternion.Euler(0.0f, 0.0f, _rotation);

        transform.position += _direction.ToVector3() * _currentSpeed * Time.deltaTime * _charge;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner)
        {
            return;
        }

        bool active = false;

        if (collision.gameObject.layer == Layers.Enemy || collision.gameObject.layer == Layers.FlyingEnemy)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy.IsAlive)
            {
                collision.gameObject.GetComponent<Enemy>().ReceiveDamage((int)(_currentDamage * _charge), _direction);
                BeforeDestroyed(collision.gameObject);
                CameraManager.Instance.ShakeCamera(0.15f, 0.1f, 0.1f, 30);
            }
            else
            {
                active = true;
            }
        }

        gameObject.SetActive(active);
    }
}
