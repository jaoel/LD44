using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyDescription description;

    private int _currentHealth;
    private int aStarCooldown = 0;
    protected Vector3 _target;
    protected List<Vector2Int> _path;
    protected bool _followPath = false;
    protected bool _moveToTarget = false;
    protected Vector3 _velocity = Vector3.zero;
    protected float _stoppingDistance = 2.0f;
    protected Player _player;
    protected new Rigidbody2D rigidbody;
    protected new BoxCollider2D collider; 

    protected bool _hasAggro;

    private void Awake()
    {
        _path = new List<Vector2Int>();
        _player = GameObject.Find("Player").GetComponent<Player>();
        _currentHealth = description.maxHealth;
    }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();

        aStarCooldown = Random.Range(0, 200);
    }

    public virtual void SetTarget(Vector3 target)
    {
        this._target = target;
        _moveToTarget = true;
    }

    protected virtual void FixedUpdate()
    {
        aStarCooldown--;
        if (KillMe())
            return;

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
            float sqrDistToTarget = (_target - transform.position).magnitude;
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

    public virtual void ApplyDamage(int damage)
    {
        _currentHealth -= damage;
    }

    public virtual bool KillMe()
    {
        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerContainer.Instance.Layers["Player"])
        {
            _player.ReceiveDamage(description.damage);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {     
    }
        
}
