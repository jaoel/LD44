using System;
using DG.Tweening;
using UnityEngine;

public class SlingshotBullet : Bullet
{
    [SerializeField]
    private Transform _visualTransform;

    [SerializeField]
    private Transform _shadowTransform;

    private float _rotation = 0f;

    protected override void Start()
    {
        base.Start();
    }

    public override void Initialize(float charge, Vector2 direction, GameObject owner)
    {
        base.Initialize(charge, direction, owner);

        _charge = Mathf.Max(_charge, 0.5f);
        _currentLifetime *= _charge;
        _rotation = UnityEngine.Random.Range(0.0f, 360.0f);
        //_visualTransform.localPosition = new Vector3(_visualTransform.localPosition.x, 0.0f, _visualTransform.localPosition.z);
        //_shadowTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //_visualTransform.DOLocalMoveY(-0.5f, _currentLifetime).SetEase(Ease.OutBounce);
        //_visualTransform.DOScale(new Vector3(0.0f, 0.0f, 0.0f), _currentLifetime).SetEase(Ease.InCirc);
        //_shadowTransform.DOScale(new Vector3(0.0f, 0.0f, 0.0f), _currentLifetime).SetEase(Ease.InCirc);
    }

    public override void UpdateBullet()
    {
        _rotation += 300.0f * Time.deltaTime;
        _visualTransform.rotation = Quaternion.Euler(0.0f, 0.0f, _rotation);

        transform.position += _direction.ToVector3() * _speed * Time.deltaTime * _charge;
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
