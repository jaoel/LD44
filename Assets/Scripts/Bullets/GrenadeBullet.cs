using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GrenadeBullet : Bullet
{
    [SerializeField]
    private Bullet _grenadePrefab;

    [SerializeField]
    private GameObject _attractorDebuff;

    [SerializeField]
    private GameObject _explosionPrefab;

    [SerializeField]
    private float _explosionRadius;

    [SerializeField]
    private float _explosionDamage;

    private float _rotation;
    private Vector3 _lastPosition;

    private List<AttractorDebuff> _attractorDebuffs;

    protected override void Start()
    {
        _rotation = UnityEngine.Random.Range(0.0f, 360.0f);
        base.Start();
    }

    public override void Initialize(float charge, Vector2 direction, GameObject owner, bool superCharged)
    {
        base.Initialize(charge, direction, owner, superCharged);
    }

    public override void UpdateBullet()
    {
        float acceleration = _owner == null ? 1.0f : Mathf.Clamp(_currentLifetime - 1.0f, 0.0f, 1.0f);
        transform.position += _direction.ToVector3() * _speed * acceleration * acceleration * Time.deltaTime * _charge;

        _rotation += 300.0f * (0.01f + acceleration) * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, _rotation);

        _lastPosition = transform.position;

        if (_superCharged)
        {
            List<Enemy> enemies = MapManager.Instance.CurrentMap.GetEnemiesInCircle(_lastPosition, _explosionRadius * 4.0f);
            _attractorDebuffs = new List<AttractorDebuff>();
            enemies.ForEach(x =>
            {
                AttractorDebuff debuff = Instantiate(_attractorDebuff.gameObject, x.transform).GetComponent<AttractorDebuff>();
                debuff.target = gameObject;
                x.AddStatusEffect(debuff);

                _attractorDebuffs.Add(debuff);
            });
        }
    }

    public override void BeforeDestroyed(GameObject hitTarget)
    {
        SoundManager.Instance.PlayExplosionSound();
        CameraManager.Instance.ShakeCamera(0.3f, 0.25f, 40);
        MapManager.Instance.DamageAllEnemiesInCircle(_lastPosition, _explosionRadius, (int)_explosionDamage, false);
        Instantiate(_explosionPrefab, _lastPosition, Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f)));

        if (_superCharged)
        {
            _attractorDebuffs.ForEach(x =>
            {
                Destroy(x);
            });
        }
    }
}
