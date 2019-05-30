using UnityEngine;

public class VampireEnemy : Enemy
{
    [SerializeField] private GameObject _humanoidFormGameObject;
    [SerializeField] private GameObject _batFormGameObject;
    [SerializeField] private float _batFormMaxSpeed;
    private Form _currentForm = Form.Humanoid;

    enum Form
    {
        Humanoid,
        Bat
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        SwitchForm(Form.Humanoid);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SwitchForm(_currentForm == Form.Bat ? Form.Humanoid : Form.Bat);
        }
    }

    private void SwitchForm(Form toForm)
    {
        if (toForm == Form.Bat)
        {
            _humanoidFormGameObject.SetActive(false);
            _batFormGameObject.SetActive(true);
            _navigation.SetMaxSpeed(_batFormMaxSpeed);
            gameObject.layer = Layers.FlyingEnemy;
        }
        else
        {
            _humanoidFormGameObject.SetActive(true);
            _batFormGameObject.SetActive(false);
            _navigation.SetMaxSpeed(_maxSpeed);
            gameObject.layer = Layers.Enemy;
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
            if (distanceToPlayer > 8f && _currentForm == Form.Humanoid)
            {
                SwitchForm(Form.Bat);
            }
            else if (distanceToPlayer < 1.5f && _currentForm == Form.Bat)
            {
                SwitchForm(Form.Humanoid);
            }
        }
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
        base.Die(velocity);
    }
}
