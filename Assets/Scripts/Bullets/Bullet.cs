using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    protected float _lifetime;

    [SerializeField]
    protected float _speed;

    [SerializeField]
    protected Vector2 _size;

    [SerializeField]
    protected float _damage;

    [SerializeField]
    protected Color _tint;

    [SerializeField]
    protected SpriteRenderer _spriteRenderer;

    protected GameObject _owner;
    protected Vector2 _direction;
    protected float _currentLifetime;
    protected float _charge;

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {

    }

    public virtual void Initialize(float charge, Vector2 direction, GameObject owner)
    {
        _currentLifetime = _lifetime;
        _charge = charge;
        SetTint(_tint);
        SetSize(_size);
        SetDirection(direction);
        SetOwner(owner);
    }

    public void SetTint(Color color)
    {
        _spriteRenderer.color = color;
    }

    public void SetSize(Vector2 size)
    {
        _spriteRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    public void SetOwner(GameObject owner)
    {
        _owner = owner;
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }

    public virtual bool UpdateLifetime()
    {
        _currentLifetime -= Time.deltaTime;
        return _currentLifetime >= 0.0f;
    }

    public virtual void UpdateBullet()
    {
        transform.position += _direction.ToVector3() * _speed * Time.deltaTime;
    }

    public virtual void BeforeDestroyed(GameObject hitTarget)
    {
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner)
        {
            return;
        }

        bool active = false;

        if (collision.gameObject.layer == Layers.Map)
        {
            BeforeDestroyed(null);
        }
        else if (collision.gameObject.layer == Layers.Enemy || collision.gameObject.layer == Layers.FlyingEnemy)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy.IsAlive)
            {
                collision.gameObject.GetComponent<Enemy>().ReceiveDamage((int)_damage, _direction);
                BeforeDestroyed(collision.gameObject);
                CameraManager.Instance.ShakeCamera(0.15f, 0.1f, 0.1f, 30);
            }
            else
            {
                active = true;
            }
        }
        else if (collision.gameObject.layer == Layers.Player)
        {
            collision.gameObject.GetComponent<Player>().ReceiveDamage((int)_damage, _direction);
            BeforeDestroyed(collision.gameObject);
        }

        gameObject.SetActive(active);
    }
}
