using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    public bool IsAlive => _currentHealth > 0;

    [SerializeField]
    protected float _maxHealth;

    [SerializeField]
    protected float _maxSpeed;

    [SerializeField]
    protected float _acceleration;

    [SerializeField]
    protected float _aggroDistance;

    [SerializeField]
    protected float _meleeDamage;

    [SerializeField]
    protected Rigidbody2D _rigidbody;

    [SerializeField]
    protected CapsuleCollider2D _collider;

    [SerializeField]
    protected Navigation _navigation;

    [SerializeField]
    protected CharacterAnimation _characterAnimation;

    [SerializeField]
    protected ParticleSystemContainer _particleSystemContainer;

    [SerializeField]
    protected ItemContainer _itemContainer;

    [SerializeField]
    protected GameObject _minimapIcon;

    protected float _currentHealth;
    protected bool _hasAggro;
    protected Player _player;
    protected GameObject _target;
    protected Vector2 _spawnPosition;

    protected Vector2 _dieDirection = Vector2.down;
    private static int _killsSinceLastDrop = 0;

    protected virtual void Awake()
    {
        _currentHealth = _maxHealth;
        _hasAggro = false;
        _player = GameObject.Find("Player").GetComponent<Player>();
        _navigation.Initialize(_rigidbody, _maxSpeed, _acceleration);
        _spawnPosition = transform.position.ToVector2();
    }

    protected virtual void FixedUpdate()
    {
        CalculateAnimation();

        if (!_player.IsAlive || !IsAlive)
        {
            _navigation.Stop();
            return;
        }

        if (_hasAggro)
        {
            if ((_target.transform.position.ToVector2() - transform.position.ToVector2()).magnitude > _aggroDistance * 2.0f)
            {
                _hasAggro = false;
                _navigation.MoveTo(_spawnPosition, false);
                return;
            }

            _navigation.MoveTo(_target, PlayerIsVisible());
        }
        else
        {
            CheckAggro();
        }
    }

    protected virtual void CheckAggro()
    {
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < _aggroDistance && PlayerIsVisible())
        {
            AggroPlayer();
        }
    }

    private void AggroPlayer()
    {
        _hasAggro = true;
        _target = _player.gameObject;

        SoundManager.Instance.PlayMonsterAggro();
    }

    protected virtual bool PlayerIsVisible()
    {
        Vector2 origin = transform.position.ToVector2();
        Vector2 target = _player.transform.position.ToVector2();

        float viewDistance = _aggroDistance;
        if (_hasAggro)
        {
            viewDistance *= 3.0f;
        }

        if ((target - origin).magnitude <= viewDistance)
        {
            int layerMask = Layers.CombinedLayerMask(Layers.Map, Layers.Player);
            RaycastHit2D hit = Physics2D.Raycast(origin, (target - origin).normalized, viewDistance, layerMask);

            if (hit.collider?.gameObject.layer == Layers.Player)
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool PlayAttackAnimation()
    {
        if (_target == null)
        {
            return false;
        }

        return Vector2.Distance(_target.transform.position.ToVector2(), transform.position.ToVector2()) < 1.0f;
    }

    private void CalculateAnimation()
    {
        CharacterAnimation.AnimationType type;
        Vector2 direction;
        
        if (!IsAlive)
        {
            type = CharacterAnimation.AnimationType.Die;
            direction = _dieDirection;
        }
        else
        {
            if (PlayAttackAnimation())
            {
                type = CharacterAnimation.AnimationType.Attack;
                direction = _target.transform.position - transform.position;
            }
            else if (_rigidbody.velocity.magnitude < 0.1f)
            {
                type = CharacterAnimation.AnimationType.Idle;
                direction = Vector2.down;
            }
            else
            {
                type = CharacterAnimation.AnimationType.Run;
                direction = _rigidbody.velocity;
            }
        }

        direction = direction.normalized;
        _characterAnimation.UpdateAnimation(type, direction);
    }

    public virtual void ApplyDamage(int damage, Vector2 velocity)
    {
        if (IsAlive)
        {
            SoundManager.Instance.PlayMonsterPainSound();

            ParticleSystem bloodSpray = Instantiate(_particleSystemContainer.bloodSpray, transform.position, Quaternion.identity);

            Vector3 dir = new Vector3(velocity.normalized.x, velocity.normalized.y, 0);
            bloodSpray.transform.DOLookAt(transform.position + dir, 0.0f);
            bloodSpray.gameObject.SetActive(true);
            bloodSpray.Play();

            _currentHealth -= damage;
            if (!IsAlive)
            {
                Die(velocity);
            }

            if (!_hasAggro)
            {
                AggroPlayer();
            }
        }
    }

    protected virtual void Die(Vector2 velocity)
    {
        _dieDirection = velocity;
        _rigidbody.velocity = _dieDirection.normalized * 2.5f;
        _minimapIcon.SetActive(false);

        _collider.size = new Vector2(0.6f, 0.6f);
        _collider.offset = new Vector2(_collider.offset.x, -0.3f);

        DropItem();
    }

    public virtual void DropItem()
    {
        float killsForGuaranteedDrop = 5.0f + Main.Instance.CurrentLevel;
        float probability = Mathf.Lerp(0.1f, 1.0f, _killsSinceLastDrop / killsForGuaranteedDrop);
        
        if (Random.Range(0.0f, 1.0f) < probability)
        {
            _killsSinceLastDrop = 0;
            Item drop = Instantiate(_itemContainer.GetEnemyDrop().itemPrefab, transform.position, Quaternion.identity);
            drop.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            drop.gameObject.SetActive(true);
        
            Main.Instance.AddInteractiveObject(drop.gameObject);
        }
        else
        {
            _killsSinceLastDrop++;
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (IsAlive && collision.gameObject.layer == Layers.Player)
        {
            if(_player.ReceiveDamage(_meleeDamage, -collision.contacts[0].normal))
            {
                _rigidbody.velocity = Vector2.zero;
            }
        }
    }
}
