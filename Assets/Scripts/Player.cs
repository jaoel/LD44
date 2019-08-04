using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class Player : MonoBehaviour, IWeaponOwner, IBuffable
{
    [SerializeField]
    private BoxCollider2D _pitCollider;

    private Key _goldKey;
    private Queue<Key> _skeletonKeys;

    public ParticleSystemContainer particleSystemContainer;

    public float maxSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 15f;
    public float invulnTime = 1.0f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 2f;
    public float dashSpeed = 25f;

    public CharacterAnimation characterAnimation;
    public Weapon CurrentWeapon;
    public int MaxWeaponCount;
    private List<Weapon> _weapons = new List<Weapon>();
    public SpriteRenderer visual;

    private int _currentHealth;
    private int _maxHealth;

    private new Rigidbody2D _rigidbody;
    private new SpriteRenderer _renderer;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _inputVector = Vector3.zero;
    private Vector2 _aimVector = Vector2.zero;
    private Vector2 _dieDirection = Vector2.down;
    private bool _shouldDash = false;
    private float _dashTimer = 0f;
    private float _dashCooldownTimer = 0f;
    private Vector3 _dashDirection = Vector2.zero;

    private float _invulnTimer = float.MaxValue;

    private float _slowTimer = float.MinValue;
    private float _slowFactor = float.MaxValue;

    private List<StatusEffect> _statusEffects;
    private bool _disabled;
    private Vector3 _lastPosition;

    public bool GodMode { get; set; } = false;
    public bool IsInvulnerable => _invulnTimer < invulnTime;

    public int Health
    {
        get
        {
            return _currentHealth;
        }
        set
        {
            _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
        }
    }

    public int MaxHealth
    {
        get
        {
            return _maxHealth;
        }
        set
        {
            _maxHealth = Mathf.Max(0, value);
            if (_maxHealth < _currentHealth)
            {
                Health = _maxHealth;
            }
        }
    }

    public bool IsAlive => _currentHealth > 0;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _renderer = GetComponentInChildren<SpriteRenderer>();

        ResetPlayer();
    }

    public void ResetPlayer(bool healPlayer = true)
    {
        MaxHealth = Main.Instance.sessionData.playerMaxHealth;
        Main.Instance.sessionData.AddWeapons(this);

        if (healPlayer)
        {
            Health = MaxHealth;
        }

        _invulnTimer = float.MaxValue;
        _velocity = Vector3.zero;
        _inputVector = Vector3.zero;
        _skeletonKeys = new Queue<Key>();
        _statusEffects = new List<StatusEffect>();
        _disabled = false;

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

    private void SwapWeapon()
    {
        Weapon newWeapon = null;
        if (Keybindings.WeaponSlot1)
        {
            if (_weapons.IndexOf(CurrentWeapon) == 0)
            {
                return;
            }
            newWeapon = _weapons[0];
        }

        if (Keybindings.WeaponSlot2 && _weapons.Count > 1)
        {
            if (_weapons.IndexOf(CurrentWeapon) == 1)
            {
                return;
            }
            newWeapon = _weapons[1];
        }

        if (newWeapon != null)
        {
            CurrentWeapon.StopShooting();
            CurrentWeapon = newWeapon;

            if (CurrentWeapon.BulletsLeft == 0)
            {
                CurrentWeapon.TriggerReload();
            }

            CurrentWeapon.UpdatePlayerUI();
            UIManager.Instance.playerUI.weaponImage.sprite = CurrentWeapon.uiImage;
        }
    }

    private void Update()
    {
        CalculateAnimation();

        UIManager.Instance.playerUI.SetHealthbar(_currentHealth, _maxHealth);
        UIManager.Instance.playerUI.SetChargeMeterPosition(transform.position);

        if (!IsAlive || Main.Instance.Paused || _disabled)
        {
            CurrentWeapon?.StoppedShooting();
            return;
        }

        CalculateInputVector();

        if(Keybindings.Dash && !_shouldDash)
        {
            _dashDirection = _inputVector;
            _shouldDash = true;
        }

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

        SwapWeapon();
    }

    void FixedUpdate()
    {
        _lastPosition = transform.position;

        if (Main.Instance.Paused || _disabled)
        {
            return;
        }

        HandleStatusEffects();

        if (!IsAlive)
        {
            if (_velocity.magnitude > 0.001f)
            {
                _velocity -= _velocity * 2.0f * Time.deltaTime;
            }
            else
            {
                _velocity = Vector2.zero;
            }
            _rigidbody.velocity = _velocity;
            return;
        }
        _dieDirection = _rigidbody.velocity;

        CalculateDeceleration();
        CalculateVelocity();

        if (_invulnTimer < invulnTime)
        {
            _invulnTimer += Time.deltaTime;
        }

        CalculateDash();
    }

    private void CalculateDash()
    {
        if (_dashTimer <= 0f && _shouldDash)
        {
            if (_dashCooldownTimer <= 0f)
            {
                _dashTimer = dashDuration;
                gameObject.layer = Layers.PlayerGhost;
            }
            _shouldDash = false;
        }
        if (_dashTimer > 0f)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer < 0f)
            {
                _dashCooldownTimer = dashCooldown;
                _dashTimer = 0f;
                gameObject.layer = Layers.Player;
            }
        }
        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer = Mathf.Max(_dashCooldownTimer - Time.deltaTime, 0f);
        }
    }

    private void CalculateInputVector()
    {
        _inputVector = Vector3.zero;
        _inputVector.x -= Keybindings.MoveLeft;
        _inputVector.x += Keybindings.MoveRight;
        _inputVector.y += Keybindings.MoveUp;
        _inputVector.y -= Keybindings.MoveDown;

        Vector3 mousePositionInWorldSpace = CameraManager.Instance.MainCamera.ScreenToWorldPoint(Keybindings.MousePosition);
        mousePositionInWorldSpace.z = 0f;
        Vector3 aimVector3 = mousePositionInWorldSpace - transform.position;
        aimVector3.z = 0f;
        _aimVector = aimVector3.normalized;
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
            if (_velocity.magnitude < 0.1f)
            {
                if (Keybindings.Attack)
                {
                    type = CharacterAnimation.AnimationType.Attack;
                    direction = _aimVector;
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
                    direction = _aimVector;
                }
                else
                {
                    direction = _velocity;
                }
            }
        }

        characterAnimation.UpdateAnimation(type, direction);
    }

    private void CalculateDeceleration()
    {
        if (_inputVector.x <= 0f && _velocity.x > 0f)
        {
            _velocity.x -= deceleration * Time.deltaTime;
            if (_velocity.x < 0f)
            {
                _velocity.x = 0f;
            }
        }
        if (_inputVector.x >= 0f && _velocity.x < 0f)
        {
            _velocity.x += deceleration * Time.deltaTime;
            if (_velocity.x > 0f)
            {
                _velocity.x = 0f;
            }
        }

        if (_inputVector.y <= 0f && _velocity.y > 0f)
        {
            _velocity.y -= deceleration * Time.deltaTime;
            if (_velocity.y < 0f)
            {
                _velocity.y = 0f;
            }
        }
        if (_inputVector.y >= 0f && _velocity.y < 0f)
        {
            _velocity.y += deceleration * Time.deltaTime;
            if (_velocity.y > 0f)
            {
                _velocity.y = 0f;
            }
        }
    }

    private void CalculateVelocity()
    {
        _velocity += _inputVector.normalized * acceleration * Time.deltaTime;

        if (_velocity.magnitude > maxSpeed)
        {
            _velocity = _velocity.normalized * maxSpeed;
        }

        if (_slowTimer > 0)
        {
            _slowTimer -= Time.deltaTime;
            _velocity *= _slowFactor;
        }
        else
        {
            _slowFactor = float.MaxValue;
        }


        if (_dashTimer > 0f)
        {
            _rigidbody.velocity = _dashDirection.normalized * dashSpeed;
        }
        else
        {
            _rigidbody.velocity = _velocity;
        }
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
        if (_invulnTimer >= invulnTime && !GodMode && _dashTimer == 0f)
        {
            CameraManager.Instance.ShakeCamera(0.5f, 0.1f, 25);

            int loopCount = 10;
            Sequence colorFlashSequence = DOTween.Sequence();
            colorFlashSequence.Append(_renderer.material.DOColor(Color.red, invulnTime / loopCount)
                .SetLoops(loopCount, LoopType.Yoyo));
            colorFlashSequence.Append(_renderer.material.DOColor(Color.white, 0.0f));
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

    private IEnumerator HandlePit(Vector3 lastPosition)
    {
        _disabled = true;
        _pitCollider.enabled = false;
        _rigidbody.velocity = Vector3.zero;
        _velocity = Vector3.zero;
        _inputVector = Vector3.zero;
        ReceiveDamage(5, Vector2.zero, false, false);

        if (!IsAlive)
        {
            _disabled = false;
            _pitCollider.enabled = true;

            yield break;
        }

        visual.enabled = false;
        yield return new WaitForSeconds(1.0f);
        visual.enabled = true;

        Vector3 direction = transform.position - lastPosition;
        transform.DOMove(lastPosition - direction.normalized, 0.5f);
        yield return new WaitForSeconds(0.5f);

        _disabled = false;
        _pitCollider.enabled = true;
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Layers.Pits && !_disabled)
        {
            StartCoroutine(HandlePit(_lastPosition));
        }
    }

    Vector2 IWeaponOwner.GetAimVector()
    {
        return _aimVector;
    }

    Vector2 IWeaponOwner.GetBulletOrigin()
    {
        return transform.position.ToVector2() + new Vector2(_aimVector.normalized.x / 2.0f, 0.35f);
    }

    void IWeaponOwner.Knockback(Vector2 direction, float force)
    {
        _rigidbody.AddForce(direction.normalized * force);
    }

    public void AddWeapon(GameObject weaponPrefab)
    {
        if (_weapons == null)
        {
            _weapons = new List<Weapon>();
        }

        Weapon newWeapon = GameObject.Instantiate(weaponPrefab, transform).GetComponent<Weapon>();

        if (_weapons.Contains(newWeapon))
        {
            Destroy(newWeapon);
            return;
        }

        newWeapon.SetOwner(this, true);

        CurrentWeapon?.StopShooting();

        if (_weapons.Count < MaxWeaponCount)
        {
            _weapons.Add(newWeapon);

            CurrentWeapon = _weapons[_weapons.Count - 1];
        }
        else
        {
            int index = _weapons.IndexOf(CurrentWeapon);

            GameObject oldWeapon = _weapons[index].gameObject;
            oldWeapon.SetActive(false);
            Destroy(oldWeapon);

            _weapons[index] = newWeapon;
            CurrentWeapon = _weapons[index];
        }

        UIManager.Instance.playerUI.weaponImage.sprite = CurrentWeapon.uiImage;
    }

    public void ClearWeapons()
    {
        _weapons.ForEach(x =>
        {
            Destroy(x.gameObject);
        });

        _weapons.Clear();
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
