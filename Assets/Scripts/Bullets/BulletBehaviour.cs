using UnityEngine;

[RequireComponent(typeof(Bullet))]
public class BulletBehaviour : MonoBehaviour {
    // With great power comes great responsibility!
    public virtual void UpdateBullet(BulletInstance bullet) {
        bullet.transform.position += (Vector3)bullet.velocity * Time.deltaTime;
    }

    public virtual void BeforeDestroyed(GameObject hitTarget) {
    }
}
