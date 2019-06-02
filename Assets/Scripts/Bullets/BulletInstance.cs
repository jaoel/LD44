using UnityEngine;

public class BulletInstance
{
    public BulletDescription description;
    public Vector2 velocity = Vector2.zero;
    public float charge = 1.0f;
    public Bullet instance = null;
    public bool active = false;
    public float lifetime = 0f;

    public Transform transform => instance?.transform;
}
