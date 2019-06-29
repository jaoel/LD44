using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System.Collections;

public class Enemy : MonoBehaviour, IBuffable 
{
    public bool IsAlive => _currentHealth > 0;
    public bool HasAggro => _hasAggro;
    public bool HasDrop => _itemDrops?.Count > 0;

    [SerializeField]
    protected float _maxHealth;

    [SerializeField]
    protected float _maxSpeed;

    [SerializeField]
    protected float _acceleration;

    [SerializeField]
    protected float _aggroDistance;

    [SerializeField]
    protected int _meleeDamage;

    [SerializeField]
    protected Rigidbody2D _rigidbody;

    [SerializeField]
    protected Collider2D _collider;

    [SerializeField]
    protected SpriteRenderer _visual;

    [SerializeField]
    protected Navigation _navigation;

    [SerializeField]
    protected CharacterAnimation _characterAnimation;

    [SerializeField]
    protected ParticleSystemContainer _particleSystemContainer;

    protected float _currentHealth;
    protected bool _hasAggro;
    protected Player _player;
    protected GameObject _target;
    protected Vector2 _spawnPosition;

    protected Vector2 _dieDirection = Vector2.down;
    protected HurtBlink _colorController;

    protected List<Item> _itemDrops;

    protected HashSet<StatusEffect> _statusEffects;

    private float _corpseCollisionTimer;

    protected virtual void Awake()
    {
        _corpseCollisionTimer = 0.0f;

        _currentHealth = _maxHealth;
        _hasAggro = false;
        _player = GameObject.Find("Player").GetComponent<Player>();
        _navigation.Initialize(_rigidbody, _maxSpeed, _acceleration);
        _spawnPosition = transform.position.ToVector2();
        _colorController = GetComponent<HurtBlink>();
        _itemDrops = new List<Item>();
        _statusEffects = new HashSet<StatusEffect>();

        _navigation.MoveTo(transform.position.ToVector2() + Utility.RandomPointOnCircleEdge(1.0f), true);
    }

    protected virtual void FixedUpdate()
    {
        HandleStatusEffects();
        CalculateAnimation();

        if (!_player.IsAlive || !IsAlive)
        {
            _navigation.Stop();     
            return;
        }

        if (_statusEffects.Any(x => x.OverrideNavigation))
        {
            foreach(StatusEffect x in _statusEffects)
            {
                x.Navigation();
            }
        }
        else
        {
            if (_hasAggro)
            {
                if ((_target.transform.position.ToVector2() - transform.position.ToVector2()).magnitude > _aggroDistance * 2.0f)
                {
                    _hasAggro = false;
                    _navigation.MoveTo(_spawnPosition, false);

                    return;
                }

                bool playerVisible = PlayerIsVisible(_aggroDistance);

                _navigation.MoveTo(_target, playerVisible);

                if (!playerVisible && !_navigation.HasPath)
                {
                    _navigation.Stop();
                }
            }
            else
            {
                if (!CheckAggro())
                {
                    if (_navigation.AtDestination())
                    {
                        _navigation.Stop();
                    }
                }
            }
        }
    }

    protected virtual bool CheckAggro()
    {
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < _aggroDistance && PlayerIsVisible(_aggroDistance))
        {
            AggroPlayer(true, 1);
        }

        return _hasAggro;
    }

    private void AggroPlayer(bool aggroSurrounding, int recursions = 0)
    {
        _hasAggro = true;
        _target = _player.gameObject;
        SoundManager.Instance.PlayMonsterAggro();

        if (aggroSurrounding)
        {
            List<Enemy> enemies = MapManager.Instance.CurrentMap.GetEnemiesInCircle(transform.position, _aggroDistance);
            int layerMask = Layers.CombinedLayerMask(Layers.Map, Layers.Enemy, Layers.FlyingEnemy);
            int rec = recursions - 1;
            _collider.enabled = false;

            enemies.ForEach(x =>
            {
                if (!x.HasAggro)
                {
                    if (IsVisible(_aggroDistance * 3.0f, x.transform.position.ToVector2(), layerMask,
                        new List<int>() { Layers.Enemy, Layers.FlyingEnemy }))
                    {
                        x.AggroPlayer(recursions > 0, rec);
                    }
                }
            });

            _collider.enabled = true;
        }
    }

    protected bool IsVisible(float viewDistance, Vector2 target, int layerMask, List<int> layers)
    {
        Vector2 origin = transform.position.ToVector2();

        if ((target - origin).magnitude <= viewDistance)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, (target - origin).normalized, viewDistance, layerMask);

            if(hit.collider != null && layers.Contains(hit.collider.gameObject.layer))
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool PlayerIsVisible(float viewDistance)
    {
        int layerMask = 0;
        if (_hasAggro)
        {
            layerMask = Layers.CombinedLayerMask(Layers.Map, Layers.Pits, Layers.Player);
            viewDistance *= 3.0f;
        }
        else
        {
            layerMask = Layers.CombinedLayerMask(Layers.Map, Layers.Player);
        }

        return IsVisible(viewDistance, _player.transform.position.ToVector2(), layerMask, new List<int>() { Layers.Player });
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

    protected virtual void Die(Vector2 velocity)
    {
        _dieDirection = velocity;
        _rigidbody.velocity = _dieDirection.normalized * 2.5f;

        _collider.offset = new Vector2(_collider.offset.x, 0.3f);
        
        if (_collider is CircleCollider2D)
        {
            ((CircleCollider2D)_collider).radius *= 0.5f;
        }

        gameObject.layer = Layers.Corpse;

        DropItem();
    }

    public void AddDrop(Item item)
    {
        _itemDrops.Add(item);
    }

    public virtual void DropItem()
    {
        _itemDrops.ForEach(x =>
        {
            Item drop = Instantiate(x, transform.position, Quaternion.identity);
            drop.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            drop.gameObject.SetActive(true);

            MapManager.Instance.AddInteractiveObject(drop.gameObject);
        });
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == Layers.Player)
        {
            if (IsAlive)
            {
                if (_player.ReceiveDamage((int)_meleeDamage, -collision.contacts[0].normal))
                {
                    _rigidbody.velocity = Vector2.zero;
                }
            }
            else
            {
                _corpseCollisionTimer += Time.deltaTime;

                if (_corpseCollisionTimer >= 0.5f)
                {
                    Physics2D.IgnoreLayerCollision(Layers.Player, Layers.Corpse);
                }
            }
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsAlive && collision.gameObject.layer == Layers.Pits && gameObject.layer == Layers.Corpse)
        {
            gameObject.layer = Layers.Pits;

            Vector3 hitPosition = Vector3.zero;
            hitPosition.x = collision.contacts[0].point.x - 0.01f * collision.contacts[0].normal.x;
            hitPosition.y = collision.contacts[0].point.y - 0.01f * collision.contacts[0].normal.y;

            Vector3 offset = new Vector3(0.5f, 0.5f, 0.0f);
            transform.DOMove(MapManager.Instance.CurrentMap.WorldToCell(hitPosition).ToVector3() + offset, 0.5f);
            transform.DOScale(0.5f, 1.0f);
            _visual.color = Color.green;
            _visual.DOColor(Utility.RGBAColor(0, 255, 0, 0.5f), 0.5f);

        }
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsAlive && collision.gameObject.layer == Layers.Player)
        {
            if (_corpseCollisionTimer > 0.5f)
            {
                StartCoroutine(UnignoreCorpseCollision());
            }
            _corpseCollisionTimer = 0.0f;
        }
    }

    private IEnumerator UnignoreCorpseCollision()
    {
        yield return new WaitForSeconds(1.0f);
        Physics2D.IgnoreLayerCollision(Layers.Player, Layers.Corpse, false);
        yield return null;
    }

    public bool ReceiveDamage(int damage, Vector2 velocity, bool maxHealth = false, bool spawnBloodSpray = true)
    {
        if (IsAlive)
        {
            SoundManager.Instance.PlayMonsterPainSound();

            if (spawnBloodSpray)
            {
                ParticleSystem bloodSpray = Instantiate(_particleSystemContainer.bloodSpray, transform.position, Quaternion.identity);

                Vector3 dir = new Vector3(velocity.normalized.x, velocity.normalized.y, 0);
                bloodSpray.transform.DOLookAt(transform.position + dir, 0.0f);
                bloodSpray.gameObject.SetActive(true);
                bloodSpray.Play();
            }
           
            _colorController.Blink(new Color(1f, 1f, 1f, 0.5f), 0.25f);

            _currentHealth -= damage;
            if (!IsAlive)
            {
                Die(velocity);
                return true;
            }

            if (!_hasAggro)
            {
                AggroPlayer(true);
            }
        }

        return false;
    }

    public virtual SpriteRenderer GetSpriteRenderer()
    {
        return _visual;
    }

    public virtual void AddStatusEffect(StatusEffect effect)
    {
        effect.OnApply(this, gameObject, _visual, _navigation);
        _statusEffects.Add(effect);
    }

    public virtual void HandleStatusEffects()
    {
        _statusEffects.RemoveWhere(x => x == null);
    }
}
