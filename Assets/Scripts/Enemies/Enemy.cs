using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    public ParticleSystemContainer particleSystemContainer;
    public ItemContainer itemContainer;
    public EnemyDescription description;
    public CharacterAnimation characterAnimation;

    private int _currentHealth;
    private int aStarCooldown = 0;
    protected Vector3 _target;
    protected List<Vector2Int> _path;
    protected bool _followPath = false;
    protected bool _moveToTarget = false;
    protected Vector3 _velocity = Vector3.zero;
    protected float _stoppingDistance = 1.0f;
    protected Player _player;
    protected new Rigidbody2D rigidbody;

    protected bool _hasAggro;

    private Vector2 dieDirection = Vector2.down;
    private bool isDead = false;

    private static int killsSinceLastDrop = 0;

    public bool IsAlive => !isDead;

    protected virtual void Awake()
    {
        _path = new List<Vector2Int>();
        _player = GameObject.Find("Player").GetComponent<Player>();
        _currentHealth = description.maxHealth;
    }

    protected virtual void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>(); 
        aStarCooldown = Random.Range(0, 200);
    }

    public virtual void SetTarget(Vector3 target)
    {
        this._target = target;
        _moveToTarget = true;
    }

    private void SmoothStopVelocity() {
        if (_velocity.magnitude > 0.001f) {
            _velocity -= _velocity * 2.0f * Time.deltaTime;
        } else {
            _velocity = Vector2.zero;
        }
        rigidbody.velocity = _velocity;
    }

    protected virtual void FixedUpdate()
    {
        CalculateAnimation();

        if (!_player.IsAlive) {
            SmoothStopVelocity();
            return;
        }

        aStarCooldown--;
        if (KillMe()) {
            SmoothStopVelocity();
            return;
        }

        CheckAggro();

        if (_hasAggro)
        {
            if (PlayerIsVisible())
            {
                SetTarget(_player.transform.position);
                _followPath = false;
                _path = new List<Vector2Int>();
            }
            else if(aStarCooldown <= 0)
            {
                aStarCooldown = 200;
                Vector2Int start = new Vector2Int((int)transform.position.x, (int)transform.position.y);
                Vector2Int target = new Vector2Int((int)_player.transform.position.x, (int)_player.transform.position.y);
                _path = NavigationManager.Instance.AStar(start, target);
                
                if (_path != null && _path.Count > 0)
                {
                    _followPath = true;
                    SetTarget(new Vector3(_path[0].x, _path[0].y));
                    _path.RemoveAt(0);
                }  
            }
        }

        if (_moveToTarget)
        {
            MoveToTarget();
        }

        TargetReached();
    }

    public virtual void CheckAggro()
    {
        if (_hasAggro)
            return;

        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < description.aggroDistance && PlayerIsVisible())
        {
            _hasAggro = true;
            SoundManager.Instance.PlayMonsterAggro();
        }                                                     
    }

    public virtual bool PlayerIsVisible()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 target = new Vector2(_player.transform.position.x, _player.transform.position.y);

        int layerMask = LayerContainer.CombinedLayerMask("Map", "Player");
        RaycastHit2D hit = Physics2D.Raycast(origin, (target - origin).normalized, description.aggroDistance, layerMask);

        if (hit.collider?.gameObject.layer == LayerContainer.Instance.Layers["Player"])
        {
            return true;
        }

        return false;
    }

    public virtual bool TargetReached()
    {  
        if (_target != Vector3.zero && _moveToTarget)
        {
            Vector3 tempPos = transform.position;
            tempPos.z = 0.0f;
            float sqrDistToTarget = (_target - tempPos).magnitude;
            float stoppingDistance = _stoppingDistance;

            if (!_followPath)
                stoppingDistance = 0;

            if (sqrDistToTarget <= _stoppingDistance)
            {
                if (_followPath && _path.Count > 0)
                {
                    SetTarget(new Vector3(_path[0].x, _path[0].y));
                    _path.RemoveAt(0);
                    return false;
                }

                _target = Vector3.zero;
                _moveToTarget = false;
                rigidbody.velocity = Vector2.zero;
                return true;
            }
        }

        return false;
    }

    protected virtual void MoveToTarget()
    {
        CalculateVelocity();
        rigidbody.velocity = _velocity;
    }

    protected virtual void CalculateVelocity()
    {
        _velocity += (_target - transform.position).normalized * description.acceleration * Time.deltaTime;

        if (_velocity.magnitude > description.maxSpeed)
        {
            _velocity = _velocity.normalized * description.maxSpeed;
        }

        _velocity.z = 0.0f;
    }

    protected virtual bool PlayAttackAnimation() {
        return Vector2.Distance(_target, transform.position) < 1f;
    }

    private void CalculateAnimation() {
        CharacterAnimation.AnimationType type;
        Vector2 direction;

        if (!IsAlive) {
            type = CharacterAnimation.AnimationType.Die;
            direction = dieDirection;
        } else {
            if(PlayAttackAnimation()) {
                type = CharacterAnimation.AnimationType.Attack;
                direction = _target - transform.position;
            } else if (_velocity.magnitude < 0.1f) {
                type = CharacterAnimation.AnimationType.Idle;
                direction = Vector2.down;
            } else {
                type = CharacterAnimation.AnimationType.Run;
                direction = _velocity;
            }
        }

        characterAnimation.UpdateAnimation(type, direction);
    }

    public virtual void ApplyDamage(int damage, Vector2 velocity)
    {
        SoundManager.Instance.PlayMonsterPainSound();

        ParticleSystem bloodSpray = Instantiate(particleSystemContainer.bloodSpray, transform.position, Quaternion.identity);

        Vector3 dir = new Vector3(velocity.normalized.x, velocity.normalized.y, 0);
        bloodSpray.transform.DOLookAt(transform.position + dir, 0.0f);
        bloodSpray.gameObject.SetActive(true);
        bloodSpray.Play();

        _currentHealth -= damage;
        if(_currentHealth <= 0) {
            dieDirection = velocity;
            _velocity = dieDirection.normalized * 2.5f;
        }
    }

    public virtual bool KillMe()
    {
        if (_currentHealth <= 0)
        {
            if (isDead == false) {
                float killsForGuaranteedDrop = 5f + Main.Instance.CurrentLevel;
                float prob = Mathf.Lerp(0.1f, 1f, killsSinceLastDrop / killsForGuaranteedDrop);
                float rnd = Random.Range(0.0f, 1.0f);

                if (rnd < prob) {
                    killsSinceLastDrop = 0;
                    Item drop = Instantiate(itemContainer.GetEnemyDrop().itemPrefab, transform.position, Quaternion.identity);
                    drop.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    drop.gameObject.SetActive(true);
                } else {
                    killsSinceLastDrop++;
                }
            }
            isDead = true;
            return true;
        }

        return false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (IsAlive && collision.gameObject.layer == LayerContainer.Instance.Layers["Player"])
        {
            _player.ReceiveDamage(description.damage, -collision.contacts[0].normal);
        }
    }  
}
