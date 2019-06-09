using System.Collections.Generic;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private float _maxSpeed;
    private float _acceleration;
    private float _deacceleration;

    private Vector2 _target;
    private Vector2 _destination;
    private bool _targetIsVisible;

    private float _minPathAge;
    private float _currentPathAge;
    private List<Vector2Int> _path;

    public bool HasPath => (_path != null && _path.Count > 0) || (_currentPathAge >= _minPathAge);
    public List<Vector2Int> Path => _path;

    public void Initialize(Rigidbody2D rigidbody, float maxSpeed, float acceleration)
    {
        _rigidbody = rigidbody;
        _maxSpeed = maxSpeed;
        _acceleration = acceleration;
        _deacceleration = 2.0f;

        _minPathAge = Random.Range(3.0f, 5.0f);
        _currentPathAge = _minPathAge;
        _path = new List<Vector2Int>();

        _target = Vector2.zero;
    }

    public void SetMaxSpeed(float maxSpeed)
    {
        _maxSpeed = Mathf.Max(0f, maxSpeed);
    }

    public void SetAcceleration(float acceleration)
    {
        _acceleration = Mathf.Max(0.1f, acceleration);
    }

    public void MoveTo(GameObject target, bool targetIsVisible)
    {
        MoveTo(target.transform.position.ToVector2(), targetIsVisible);
    }

    public void MoveTo(Vector2 target, bool targetIsVisible)
    {
        _target = target;
        _targetIsVisible = targetIsVisible;
    }

    public bool AtDestination(float stoppingDistance = 1.0f)
    {
        return (_target.ToVector3() - transform.position).magnitude <= stoppingDistance;
    }

    public void Stop()
    {
        _target = Vector2.zero;
        _targetIsVisible = false;
    }

    private void FixedUpdate()
    {
        if (_target != Vector2.zero)
        {
            if (_targetIsVisible)
            {
                _path = null;
                _currentPathAge = _minPathAge;
                _destination = _target;
            }
            else
            {
                if (_currentPathAge >= _minPathAge)
                {
                    _path = NavigationManager.Instance.AStar(MapManager.Instance.CurrentMap.WorldToCell(transform.position.ToVector2()),
                        MapManager.Instance.CurrentMap.WorldToCell(_target), out float distance);

                    _currentPathAge = 0.0f;

                    if (_path.Count > 0)
                    {
                        SetPathTarget();
                    }
                }
                else if (_path == null)
                {
                    _destination = _target;
                }
            }

            FollowPath();
            CalculateVelocity();
        }
        else
        {
            SmoothStop();
        }

        _currentPathAge += Time.deltaTime;
    }

    private void CalculateVelocity()
    {
        Vector2 direction = (_destination - transform.position.ToVector2()).normalized;
        _rigidbody.velocity += direction * _acceleration * Time.deltaTime;
        if (_rigidbody.velocity.magnitude > _maxSpeed)
        {
            _rigidbody.velocity = _rigidbody.velocity.normalized * _maxSpeed;
        }
    }

    private void SmoothStop()
    {
        if (_rigidbody.velocity.magnitude > 0.001f)
        {
            _rigidbody.velocity -= _rigidbody.velocity * _deacceleration * Time.deltaTime;
        }
        else
        {
            _rigidbody.velocity = Vector2.zero;
        }
    }

    private void FollowPath()
    {
        if (_path != null && _path.Count > 0 
            && (_destination - MapManager.Instance.CurrentMap.WorldToCell(transform.position.ToVector2())).magnitude <= 1.0f)
        {
            SetPathTarget();
        }
    }

    private void SetPathTarget()
    {
        _destination = _path[0];
        _path.RemoveAt(0);
    }
}
