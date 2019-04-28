using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public ParticleSystemContainer particleSystemContainer;

    public float maxSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 15f;
    public float invulnTime = 1.0f;

    public Weapon CurrentWeapon;
    public Animator animator;

    public int startHealth = 100;

    private int currentHealth;
    private int maxHealth;

    private new Rigidbody2D rigidbody;
    private new SpriteRenderer renderer;
    private Vector3 velocity = Vector3.zero;
    private Vector3 inputVector = Vector3.zero;
    private Vector2 aimVector = Vector2.zero;

    private float _invulnTimer = float.MaxValue; 

    private float cooldownEndTime = 0f;

    public int Health {
        get {
            return currentHealth;
        }
        set {
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

    public bool IsAlive
    {
        get { return currentHealth > 0; }
    }

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
        CalculateInputVector();

        Vector3 mousePositionInWorldSpace = CameraManager.Instance.MainCamera.ScreenToWorldPoint(Keybindings.MousePosition);
        mousePositionInWorldSpace.z = 0f;
        Vector3 aimVector3 = mousePositionInWorldSpace - transform.position;
        aimVector3.z = 0f;
        aimVector = aimVector3.normalized;

        if (Keybindings.Attack && Time.time >= cooldownEndTime)
        {
            cooldownEndTime = Time.time + CurrentWeapon.Description.Cooldown;
            CurrentWeapon.Shoot(aimVector, transform.position, gameObject);
        }

        CalculateAnimation();

        UIManager.Instance.playerUI.SetHealthbar(currentHealth, maxHealth);
    }

    void FixedUpdate()
    {
        if (!IsAlive)
            return;

        CalculateDeceleration();
        CalculateVelocity();

        if (_invulnTimer < invulnTime)
            _invulnTimer += Time.deltaTime;
    }

    private void CalculateInputVector() {
        inputVector = Vector3.zero;
        inputVector.x -= Keybindings.MoveLeft;
        inputVector.x += Keybindings.MoveRight;
        inputVector.y += Keybindings.MoveUp;
        inputVector.y -= Keybindings.MoveDown;
    }

    private void CalculateAnimation() {
        if (velocity.magnitude < 0.1f) {
            animator.SetInteger("direction", 0);
        } else {
            Vector2 direction = Keybindings.Attack ? aimVector : (Vector2)velocity;
            int animationIndex = Mathf.RoundToInt((135f + Vector2.SignedAngle(new Vector2(1f, 1f).normalized, direction)) / 90f);
            animator.SetInteger("direction", animationIndex + 1);
        }
    }

    private void CalculateDeceleration() {
        if (inputVector.x <=  0f && velocity.x > 0f) {
            velocity.x -= deceleration * Time.deltaTime;
            if (velocity.x < 0f) velocity.x = 0f;
        }
        if (inputVector.x >= 0f && velocity.x < 0f)
        {
            velocity.x += deceleration * Time.deltaTime;
            if (velocity.x > 0f) velocity.x = 0f;
        }

        if (inputVector.y <= 0f && velocity.y > 0f)
        {
            velocity.y -= deceleration * Time.deltaTime;
            if (velocity.y < 0f) velocity.y = 0f;
        }
        if (inputVector.y >= 0f && velocity.y < 0f)
        {
            velocity.y += deceleration * Time.deltaTime;
            if (velocity.y > 0f) velocity.y = 0f;
        }
    }

    private void CalculateVelocity()
    {
        velocity += inputVector.normalized * acceleration * Time.deltaTime;

        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        rigidbody.velocity = velocity;
    }

    public void ReceiveDamage(int damage)
    {
        if (_invulnTimer >= invulnTime)
        {
            CameraManager.Instance.ShakeCamera(1.0f, 0.4f, 0.3f);

            int loopCount = 10;
            Sequence colorFlashSequence = DOTween.Sequence();
            colorFlashSequence.Append(renderer.material.DOColor(Color.red, invulnTime / loopCount)
                .SetLoops(loopCount, LoopType.Yoyo));
            colorFlashSequence.Append(renderer.material.DOColor(Color.white, 0.0f));

            colorFlashSequence.Play();
                                         
            _invulnTimer = 0;
            Health -= damage;
        }
    }
}
