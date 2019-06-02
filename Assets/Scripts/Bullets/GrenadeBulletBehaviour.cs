using UnityEngine;
using System.Collections;

public class GrenadeBulletBehaviour : BulletBehaviour
{
    public GameObject explosionPrefab;

    private float _rotation = 0f;
    private Vector3 _lastPosition = Vector3.zero;
    private float _damageRadius = 2f;
    private int _damage = 10;

    private void Start()
    {
        _rotation = Random.Range(0f, 360f);
    }

    public override void UpdateBullet(BulletInstance bullet)
    {
        _damage = bullet.description.damage;
        Vector3 velocity = bullet.velocity;
        velocity.z = 0f;
        float speed = Mathf.Clamp(bullet.lifetime - 1f, 0f, 1f);
        bullet.transform.position += velocity * speed * speed * Time.deltaTime * bullet.charge;
        _rotation += 300f * (0.01f + speed) * Time.deltaTime;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, _rotation);
        _lastPosition = bullet.transform.position;
    }

    public override void BeforeDestroyed(GameObject hitTarget, Vector2 velocity = new Vector2())
    {
        SoundManager.Instance.PlayExplosionSound();
        CameraManager.Instance.ShakeCamera(0.6f, 0.25f, 1.25f);
        Main.Instance.DamageAllEnemiesInCircle(_lastPosition, _damageRadius, _damage, true);
        Instantiate(explosionPrefab, _lastPosition, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
    }
}
