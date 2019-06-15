using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class Player : MonoBehaviour, IWeaponOwner, IBuffable
{
    private Key _goldKey;
    private Queue<Key> _skeletonKeys;

    public ParticleSystemContainer particleSystemContainer;

    public float maxSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 15f;
    public float invulnTime = 1.0f;

    public CharacterAnimation characterAnimation;
    public Weapon CurrentWeapon;
    public SpriteRenderer visual;

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

    public float firingRateModifier = 1.0f;
    private float _slowTimer = float.MinValue;
    private float _slowFactor = float.MaxValue;

    private List<StatusEffect> _statusEffects;

    public bool GodMode { get; set; } = false;
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
        MaxHealth = Main.Instance.sessionData.playerMaxHealth;
        Main.Instance.sessionData.AddWeapons(this);

        Health = MaxHealth;
        _invulnTimer = float.MaxValue;
        velocity = Vector3.zero;
        inputVector = Vector3.zero;
        _skeletonKeys = new Queue<Key>();
        _statusEffects = new List<StatusEffect>();

        //CurrentWeapon.SetOwner(this, true);
        //
        //UIManager.Instance.playerUI.weaponImage.sprite = CurrentWeapon?.uiImage;
        UIManager.Instance.playerUI.SetGoldKey(false);
        UIManager.Instance.playerUI.RemoveSkeletonKey(_skeletonKeys.Count);
        UIManager.Instance.playerUI.chargeMeter.value = 0.0f;
    }

    public void AddKey(Key key, bool isGoldKey)
    {
        if (isGoldKey)
        {
            _goldKey = key;
            UIManager.Instance.playerUI.SetGoldKey(true);
        }
        else
        {
            _skeletonKeys.Enqueue(key);
            UIManager.Instance.playerUI.AddSkeletonKey(_skeletonKeys.Count);
        }
    }

    public bool UseKey(in Door door)
    {
        if (door.IsGoalDoor && _goldKey != null)
        {
            UIManager.Instance.playerUI.SetGoldKey(false);
            Destroy(_goldKey);
            return true;
        }
        else if (_skeletonKeys.Count > 0 && !door.IsGoalDoor)
        {
            Key key = _skeletonKeys.Dequeue();
            UIManager.Instance.playerUI.RemoveSkeletonKey(_skeletonKeys.Count);
            Destroy(key);
            return true;
        }

        return false;
    }

    private void Update()
    {
        CalculateAnimation();

        UIManager.Instance.playerUI.SetHealthbar(currentHealth, maxHealth);
        UIManager.Instance.playerUI.SetChargeMeterPosition(transform.position);

        if (!IsAlive || Main.Instance.Paused)
        {
            CurrentWeapon?.StoppedShooting();
            return;
        }

        CalculateInputVector();

        if (Keybindings.Attack)
        {
            CurrentWeapon?.Shoot();
        }
        else if (!Keybindings.Attack)
        {
            CurrentWeapon?.StoppedShooting();
        }

        if (Keybindings.Reload)
        {
            CurrentWeapon?.TriggerReload();
        }
    }

    void FixedUpdate()
    {
        if (Main.Instance.Paused)
        {
            return;
        }

        HandleStatusEffects();

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

        Vector3 mousePositionInWorldSpace = CameraManager.Instance.MainCamera.ScreenToWorldPoint(Keybindings.MousePosition);
        mousePositionInWorldSpace.z = 0f;
        Vector3 aimVector3 = mousePositionInWorldSpace - transform.position;
        aimVector3.z = 0f;
        aimVector = aimVector3.normalized;
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
        if (GodMode || (slowFactor > _slowFactor && _slowTimer > 0.0f))
        {
            return;
        }

        _slowFactor = slowFactor;
        _slowTimer = slowTimer;
    }

    public bool ReceiveDamage(int damage, Vector2 velocity, bool maxHealth = false, bool spawnBloodSpray = true)
    {
        if (_invulnTimer >= invulnTime && !GodMode)
        {
            CameraManager.Instance.ShakeCamera(1.0f, 0.3f, 1.0f);

            int loopCount = 10;
            Sequence colorFlashSequence = DOTween.Sequence();
            colorFlashSequence.Append(renderer.material.DOColor(Color.red, invulnTime / loopCount)
                .SetLoops(loopCount, LoopType.Yoyo));
            colorFlashSequence.Append(renderer.material.DOColor(Color.white, 0.0f));
            colorFlashSequence.Play();

            ParticleSystem bloodSpray = Instantiate(particleSystemContainer.bloodSpray, transform.position, Quaternion.identity);

            Vector3 dir = new Vector3(velocity.normalized.x, velocity.normalized.y, 0);
            bloodSpray.transform.DOLookAt(transform.position + dir, 0.0f);
            bloodSpray.gameObject.SetActive(true);
            bloodSpray.Play();

            _invulnTimer = 0;
            Health -= (int)damage;
            if (maxHealth)
            {
                MaxHealth -= (int)damage;
            }

            if (Health <= 0)
            {
                SoundManager.Instance.PlayPlayerDeath(Random.Range(0.0f, 1.0f) < 0.1f);
            }
            else
            {
                SoundManager.Instance.PlayPainSound();
            }

            return true;
        }

        return false;
    }

    Vector2 IWeaponOwner.GetAimVector()
    {
        return aimVector;
    }

    Vector2 IWeaponOwner.GetBulletOrigin()
    {
        return transform.position;
    }

    void IWeaponOwner.Knockback(Vector2 direction, float force)
    {
        rigidbody.AddForce(direction.normalized * force);
    }

    GameObject IWeaponOwner.GetGameObject()
    {
        return gameObject;
    }

    public virtual SpriteRenderer GetSpriteRenderer()
    {
        return visual;
    }

    public virtual void AddStatusEffect(StatusEffect effect)
    {
        effect.OnApply(this, this.gameObject, visual);
        _statusEffects.Add(effect);
    }

    public virtual void HandleStatusEffects()
    {
        _statusEffects.RemoveAll(x => x == null);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
