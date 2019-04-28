using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float maxSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 15f;
    public float invulnTime = 1.0f;

    public Weapon CurrentWeapon;
    public Animator animator;

    public int currentHealth;
    public int maxHealth;

    private new Rigidbody2D rigidbody;
    private Vector3 velocity = Vector3.zero;
    private Vector3 inputVector = Vector3.zero;
    private Vector2 aimVector = Vector2.zero;

    private float _invulnTimer = float.MaxValue; 

    private float cooldownEndTime = 0f;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        CalculateInputVector();

        Vector3 mousePositionInWorldSpace = CameraManager.Instance.MainCamera.ScreenToWorldPoint(Keybindings.MousePosition);
        mousePositionInWorldSpace.z = 0f;
        aimVector = (mousePositionInWorldSpace - transform.position).normalized;
        if (Keybindings.Attack && Time.time >= cooldownEndTime)
        {
            cooldownEndTime = Time.time + CurrentWeapon.Description.Cooldown;
            CurrentWeapon.Shoot(aimVector, transform.position);
        }

        CalculateAnimation();

        UIManager.Instance.playerUI.SetHealthbar(currentHealth, maxHealth);
    }

    void FixedUpdate()
    {
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
            _invulnTimer = 0;
            currentHealth -= damage;
        }
    }
}
