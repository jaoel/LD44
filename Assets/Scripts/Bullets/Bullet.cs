using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public SpriteRenderer spriteRenderer;

    private Color originalColor;
    private Vector3 originalSize;

    public BulletDescription description { get; set; }
    private GameObject _owner;
    private Vector2 _velocity;

    private BulletBehaviour bulletBehaviour = null;

    private void Start() {
        originalColor = spriteRenderer.color;
        originalSize = spriteRenderer.transform.localScale;
        bulletBehaviour = GetComponent<BulletBehaviour>();
        if (bulletBehaviour == null) {
            bulletBehaviour = gameObject.AddComponent<BulletBehaviour>();
        }
    }

    public void Reset() {
        SetTint(originalColor);
        SetSize(originalSize);
    }

    public void SetTint(Color color) {
        spriteRenderer.color = color;
    }

    public void SetSize(Vector2 size) {
        spriteRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    public void SetOwner(GameObject owner)
    {
        _owner = owner;
    }

    public void SetVelocity(Vector2 velocity)
    {
        _velocity = velocity;
    }

    public void UpdateBullet(BulletInstance bullet) {
        bulletBehaviour.UpdateBullet(bullet);
    }

    public void BeforeDestroyed() {
        bulletBehaviour.BeforeDestroyed(null);
    }
   
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == _owner)
            return;

        bool active = false;

        if (collision.gameObject.layer == LayerContainer.Instance.Layers["Map"])
        {
            bulletBehaviour.BeforeDestroyed(null);
        }
        else if (collision.gameObject.layer == LayerContainer.Instance.Layers["Enemy"])
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy.IsAlive) {
                collision.gameObject.GetComponent<Enemy>().ApplyDamage(description.damage, _velocity);
                CameraManager.Instance.ShakeCamera(1.0f, 0.2f, 0.1f);
                bulletBehaviour.BeforeDestroyed(collision.gameObject);
            } else {
                active = true;
            }
        }
        else if (collision.gameObject.layer == LayerContainer.Instance.Layers["Player"])
        {
            collision.gameObject.GetComponent<Player>().ReceiveDamage(description.damage, _velocity);
            bulletBehaviour.BeforeDestroyed(collision.gameObject);
        }

        gameObject.SetActive(active);
    }
}
