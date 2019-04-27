using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyDescription description;

    private int _currentHealth;
    protected Vector3 _target;
    protected bool _moveToTarget = false;
    protected Vector3 _velocity = Vector3.zero;
    protected float _stoppingDistance = 1.0f;
    protected Player _player;

    private void Awake()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        _currentHealth = description.maxHealth;
    }

    private void Start()
    {
    }

    public virtual void SetTarget(Vector3 target)
    {
        this._target = target;
        _moveToTarget = true;
    }

    protected virtual void FixedUpdate()
    {
        KillMe();
        CheckAggro();
        if (_moveToTarget)
        {
            MoveToTarget();
            TargetReached();
        }
    }

    public virtual void CheckAggro()
    {
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < description.aggroDistance)
            SetTarget(_player.transform.position);
    }

    public virtual bool TargetReached()
    {
        if (_target != Vector3.zero && _moveToTarget)
        {
            float sqrDistToTarget = (_target - transform.position).magnitude;
            if (sqrDistToTarget <= _stoppingDistance)
            {
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
        transform.position += _velocity * Time.deltaTime;
    }

    protected virtual void CalculateVelocity()
    {
        _velocity += (_target - transform.position).normalized * description.acceleration * Time.deltaTime;

        if (_velocity.magnitude > description.maxSpeed)
        {
            _velocity = _velocity.normalized * description.maxSpeed;
        }     
    } 

    public virtual void ApplyDamage(int damage)
    {
        _currentHealth -= damage;
    }

    public virtual void KillMe()
    {
        if (_currentHealth <= 0)
            Destroy(gameObject);
    }
}
