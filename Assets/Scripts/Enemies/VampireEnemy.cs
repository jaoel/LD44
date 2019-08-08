using System.Collections.Generic;
using UnityEngine;

public class VampireEnemy : Enemy
{
    [SerializeField] private GameObject _humanoidFormGameObject;
    [SerializeField] private GameObject _batFormGameObject;
    [SerializeField] private float _batFormMaxSpeed;
    [SerializeField] private float _batFormAcceleration;
    private Form _currentForm = Form.Humanoid;
    private float switchFormTimestamp = 0f;

    enum Form
    {
        Humanoid,
        Bat
    }

    protected override void Start()
    {

        base.Start();
        SwitchForm(Form.Humanoid);

    }

    private void SwitchForm(Form toForm)
    {
        if (toForm == Form.Bat)
        {
            _humanoidFormGameObject.SetActive(false);
            _batFormGameObject.SetActive(true);
            _navigation.SetMaxSpeed(_batFormMaxSpeed);
            _navigation.SetAcceleration(_batFormAcceleration);
            gameObject.layer = Layers.FlyingEnemy;
            switchFormTimestamp = Time.time + Random.Range(5f, 10f);
        }
        else
        {
            _humanoidFormGameObject.SetActive(true);
            _batFormGameObject.SetActive(false);
            _navigation.SetMaxSpeed(_maxSpeed);
            _navigation.SetAcceleration(_acceleration);
            gameObject.layer = Layers.Enemy;
            switchFormTimestamp = Time.time + Random.Range(5f, 10f);
        }
        _currentForm = toForm;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!_player.IsAlive || !IsAlive)
        {
            _navigation.Stop();
            return;
        }

        if (_target != null)
        {
            float distanceToPlayer = Vector2.Distance(_target.transform.position.ToVector2(), transform.position.ToVector2());
            if ((distanceToPlayer > 8f || switchFormTimestamp < Time.time) && _currentForm == Form.Humanoid)
            {
                SwitchForm(Form.Bat);
            }
            else if ((switchFormTimestamp < Time.time) && _currentForm == Form.Bat)
            {
                SwitchForm(Form.Humanoid);
            }
        }
    }

    public override bool PlayerIsVisible(float viewDistance)
    {
        int layerMask = 0;
        if (_hasAggro && _currentForm == Form.Humanoid)
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

    protected override bool PlayAttackAnimation()
    {
        if (_target == null || _currentForm == Form.Bat)
        {
            return false;
        }

        return Vector2.Distance(_target.transform.position.ToVector2(), transform.position.ToVector2()) < 1f;
    }

    protected override void Die(Vector2 velocity)
    {
        SwitchForm(Form.Humanoid);
        _dieDirection = Random.rotation * Vector2.up;
        base.Die(velocity);
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (IsAlive && collision.gameObject.layer == Layers.Player)
        {
            if(_currentForm == Form.Bat)
            {
                if (_player.ReceiveDamage(_meleeDamage, -collision.contacts[0].normal, true))
                {
                    _rigidbody.velocity = Vector2.zero;
                }
                SwitchForm(Form.Humanoid);
            }
            else
            {
                if (_player.ReceiveDamage(_meleeDamage, -collision.contacts[0].normal))
                {
                    _rigidbody.velocity = Vector2.zero;
                }
            }
        }
    }
}
