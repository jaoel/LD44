using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public ParticleSystemContainer particleSystemContainer;
    public ParticleSystem dustTrail;

    public float maxSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 15f;
    public float invulnTime = 1.0f;

    public CharacterAnimation characterAnimation;
    public Weapon CurrentWeapon;

    public int startHealth = 100;

    private int currentHealth;
    private int maxHealth;

    private new Rigidbody2D rigidbody;
    private new SpriteRenderer renderer;
    private Vector3 velocity = Vector3.zero;
    private Vector3 inputVector = Vector3.zero;
    private Vector2 aimVector = Vector2.zero;
    private Vector2 dieDirection = Vector2.down;

    private float _invulnTimer = float.MaxValue;

    private float cooldownEndTime = 0f;

    public float firingRateModifier = 1.0f;
    private float _slowTimer = float.MinValue;
    private float _slowFactor = float.MaxValue;

    public bool IsInvulnerable => _invulnTimer < invulnTime;

    public int Health
    {
        get
        {
            return currentHealth;
        }
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
        }
    }

    public int MaxHealth
    {
        get
        {
            return maxHealth;
        }
        set
        {
            maxHealth = Mathf.Max(0, value);
            if (maxHealth < currentHealth)
            {
                Health = maxHealth;
            }
        }
    }

    public bool IsAlive => currentHealth > 0;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponentInChildren<SpriteRenderer>();

        ResetPlayer();
    }

    public void ResetPlayer()
    {
        MaxHealth = startHealth;
        Health = startHealth;
        _invulnTimer = float.MaxValue;
        velocity = Vector3.zero;
        cooldownEndTime = 0f;
        inputVector = Vector3.zero;
    }

    private void Update()
    {
        CalculateAnimation();

        UIManager.Instance.playerUI.SetHealthbar(currentHealth, maxHealth);

        if (!IsAlive || Main.Instance.gamePaused)
        {
            CurrentWeapon.StoppedShooting();
            return;
        }

        CalculateInputVector();

        Vector3 mousePositionInWorldSpace = CameraManager.Instance.MainCamera.ScreenToWorldPoint(Keybindings.MousePosition);
        mousePositionInWorldSpace.z = 0f;
        Vector3 aimVector3 = mousePositionInWorldSpace - transform.position;
        aimVector3.z = 0f;
        aimVector = aimVector3.normalized;

        if (Keybindings.Attack && Time.time >= cooldownEndTime)
        {
            cooldownEndTime = Time.time + CurrentWeapon.description.cooldown / firingRateModifier;
            CurrentWeapon.Shoot(aimVector, transform.position, gameObject);
        }
        else if (!Keybindings.Attack)
        {
            CurrentWeapon.StoppedShooting();
        }
    }

    void FixedUpdate()
    {
        if (Main.Instance.gamePaused)
        {
            return;
        }

        if (!IsAlive)
        {
            if (velocity.magnitude > 0.001f)
            {
                velocity -= velocity * 2.0f * Time.deltaTime;
            }
            else
            {
                velocity = Vector2.zero;
            }
            rigidbody.velocity = velocity;
            return;
        }
        dieDirection = rigidbody.velocity;

        CalculateDeceleration();
        CalculateVelocity();

        if (_invulnTimer < invulnTime)
        {
            _invulnTimer += Time.deltaTime;
        }
    }

    private void CalculateInputVector()
    {
        inputVector = Vector3.zero;
        inputVector.x -= Keybindings.MoveLeft;
        inputVector.x += Keybindings.MoveRight;
        inputVector.y += Keybindings.MoveUp;
        inputVector.y -= Keybindings.MoveDown;
    }

    private void CalculateAnimation()
    {
        CharacterAnimation.AnimationType type;
        Vector2 direction;

        if (!IsAlive)
        {
            type = CharacterAnimation.AnimationType.Die;
            direction = dieDirection;
        }
        else
        {
            if (velocity.magnitude < 0.1f)
            {
                if (Keybindings.Attack)
                {
                    type = CharacterAnimation.AnimationType.Attack;
                    direction = aimVector;
                }
                else
                {
                    type = CharacterAnimation.AnimationType.Idle;
                    direction = Vector2.down;
                }
            }
            else
            {
                type = CharacterAnimation.AnimationType.Run;
                if (Keybindings.Attack)
                {
                    direction = aimVector;
                }
                else
                {
                    direction = velocity;
                }
            }
        }

        characterAnimation.UpdateAnimation(type, direction);
    }

    private void CalculateDeceleration()
    {
        if (inputVector.x <= 0f && velocity.x > 0f)
        {
            velocity.x -= deceleration * Time.deltaTime;
            if (velocity.x < 0f)
            {
                velocity.x = 0f;
            }
        }
        if (inputVector.x >= 0f && velocity.x < 0f)
        {
            velocity.x += deceleration * Time.deltaTime;
            if (velocity.x > 0f)
            {
                velocity.x = 0f;
            }
        }

        if (inputVector.y <= 0f && velocity.y > 0f)
        {
            velocity.y -= deceleration * Time.deltaTime;
            if (velocity.y < 0f)
            {
                velocity.y = 0f;
            }
        }
        if (inputVector.y >= 0f && velocity.y < 0f)
        {
            velocity.y += deceleration * Time.deltaTime;
            if (velocity.y > 0f)
            {
                velocity.y = 0f;
            }
        }
    }

    private void CalculateVelocity()
    {
        velocity += inputVector.normalized * acceleration * Time.deltaTime;

        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        if (_slowTimer > 0)
        {
            _slowTimer -= Time.deltaTime;
            velocity *= _slowFactor;
        }
        else
        {
            _slowFactor = float.MaxValue;
        }

        rigidbody.velocity = velocity;
    }

    public void SetSlow(float slowFactor, float slowTimer)
    {
        if (slowFactor > _slowFactor && _slowTimer > 0.0f)
        {
            return;
        }

        _slowFactor = slowFactor;
        _slowTimer = slowTimer;
    }

    public void ReceiveDamage(int damage, Vector2 direction)
    {
        if (_invulnTimer >= invulnTime)
        {
            CameraManager.Instance.ShakeCamera(1.0f, 0.3f, 1.0f);

            int loopCount = 10;
            Sequence colorFlashSequence = DOTween.Sequence();
            colorFlashSequence.Append(renderer.material.DOColor(Color.red, invulnTime / loopCount)
                .SetLoops(loopCount, LoopType.Yoyo));
            colorFlashSequence.Append(renderer.material.DOColor(Color.white, 0.0f));
            colorFlashSequence.Play();

            ParticleSystem bloodSpray = Instantiate(particleSystemContainer.bloodSpray, transform.position, Quaternion.identity);

            Vector3 dir = new Vector3(direction.normalized.x, direction.normalized.y, 0);
            bloodSpray.transform.DOLookAt(transform.position + dir, 0.0f);
            bloodSpray.gameObject.SetActive(true);
            bloodSpray.Play();

            _invulnTimer = 0;
            Health -= damage;

            if (Health <= 0)
            {
                SoundManager.Instance.PlayPlayerDeath(Random.Range(0.0f, 1.0f) < 0.1f);
            }
            else
            {
                SoundManager.Instance.PlayPainSound();
            }
        }
    }
}
