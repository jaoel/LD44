using UnityEngine;

public class BulletInstance {
    public BulletDescription description;
    public Vector2 velocity = Vector2.zero;
    public Bullet instance = null;
    public bool active = false;
    public float lifetime = 0f;

    public Transform transform => instance?.transform;
}
