using UnityEngine;
using System.Collections;

public class GrenadeBulletBehaviour : BulletBehaviour {
    public GameObject explosionPrefab;

    private float rot = 0f;
    private Vector3 lastPosition = Vector3.zero;
    private float damageRadius = 2f;
    private int damage = 10;

    private void Start() {
        rot = Random.Range(0f, 360f);
    }

    public override void UpdateBullet(BulletInstance bullet) {
        damage = bullet.description.damage;
        Vector3 velocity = bullet.velocity;
        velocity.z = 0f;
        float speed = Mathf.Clamp(bullet.lifetime - 1f, 0f, 1f);
        bullet.transform.position += velocity * speed * speed * Time.deltaTime;
        rot += 300f * (0.01f + speed) * Time.deltaTime;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, rot);
        lastPosition = bullet.transform.position;
    }

    public override void BeforeDestroyed(GameObject hitTarget) {
        SoundManager.Instance.PlayExplosionSound();
        CameraManager.Instance.ShakeCamera(0.6f, 0.25f, 1.25f);
        Main.Instance.DamageAllEnemiesInCircle(lastPosition, damageRadius, damage, true);
        Instantiate(explosionPrefab, lastPosition, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
    }
}
