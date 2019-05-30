using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private Color _originalColor;
    private Vector3 _originalSize;

    public BulletDescription Description { get; set; }
    private GameObject _owner;
    private Vector2 _velocity;

    private BulletBehaviour _bulletBehaviour = null;

    private void Awake()
    {
        _originalColor = spriteRenderer.color;
        _originalSize = spriteRenderer.transform.localScale;
        _bulletBehaviour = GetComponent<BulletBehaviour>();
        if (_bulletBehaviour == null)
        {
            _bulletBehaviour = gameObject.AddComponent<BulletBehaviour>();
        }
    }

    public void Reset()
    {
        SetTint(_originalColor);
        SetSize(_originalSize);
    }

    public void SetTint(Color color)
    {
        spriteRenderer.color = color;
    }

    public void SetSize(Vector2 size)
    {
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

    public void UpdateBullet(BulletInstance bullet)
    {
        _bulletBehaviour.UpdateBullet(bullet);
    }

    public void BeforeDestroyed()
    {
        _bulletBehaviour.BeforeDestroyed(null);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner)
        {
            return;
        }

        bool active = false;

        if (collision.gameObject.layer == Layers.Map)
        {
            _bulletBehaviour.BeforeDestroyed(null);
        }
        else if (collision.gameObject.layer == Layers.Enemy || collision.gameObject.layer == Layers.FlyingEnemy)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy.IsAlive)
            {
                collision.gameObject.GetComponent<Enemy>().ApplyDamage(Description.damage, _velocity);
                _bulletBehaviour.BeforeDestroyed(collision.gameObject);
                CameraManager.Instance.ShakeCamera(0.15f, 0.1f, 0.1f, 30);
            }
            else
            {
                active = true;
            }
        }
        else if (collision.gameObject.layer == Layers.Player)
        {
            collision.gameObject.GetComponent<Player>().ReceiveDamage(Description.damage, _velocity);
            _bulletBehaviour.BeforeDestroyed(collision.gameObject);
        }

        gameObject.SetActive(active);
    }
}
