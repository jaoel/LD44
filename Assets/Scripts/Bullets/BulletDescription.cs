using UnityEngine;

[CreateAssetMenu]
public class BulletDescription : ScriptableObject {
    public Bullet bulletPrefab;
    public float lifetime = 2f;
    public Vector2 size = Vector2.one;
    public int damage = 1;
    public Color tint = Color.white;
}
