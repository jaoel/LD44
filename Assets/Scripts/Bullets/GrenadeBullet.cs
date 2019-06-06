using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GrenadeBullet : Bullet
{
    [SerializeField]
    private GameObject _explosionPrefab;

    [SerializeField]
    private float _explosionRadius;

    [SerializeField]
    private float _explosionDamage;

    private float _rotation;
    private Vector3 _lastPosition;

    protected override void Start()
    {
        _rotation = UnityEngine.Random.Range(0.0f, 360.0f);
        base.Start();
    }

    public override void UpdateBullet()
    {
        float acceleration = Mathf.Clamp(_currentLifetime - 1.0f, 0.0f, 1.0f);
        transform.position += _direction.ToVector3() * _speed * acceleration * acceleration * Time.deltaTime * _charge;

        _rotation += 300.0f * (0.01f + acceleration) * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, _rotation);

        _lastPosition = transform.position;
    }

    public override void BeforeDestroyed(GameObject hitTarget)
    {
        SoundManager.Instance.PlayExplosionSound();
        CameraManager.Instance.ShakeCamera(0.6f, 0.25f, 1.25f);
        Main.Instance.DamageAllEnemiesInCircle(_lastPosition, _explosionRadius, (int)_explosionDamage, true);
        Instantiate(_explosionPrefab, _lastPosition, Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f)));
    }
}
