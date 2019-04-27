using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float maxSpeed = 8f;
    public float acceleration = 25f;
    public float deceleration = 15f;
    public BulletDescription bulletDescription;
    public float shootCooldown = 0.1f;
    public float bulletSpeed = 15f;
    public Weapon CurrentWeapon;

    private new Rigidbody2D rigidbody;
    private Vector3 velocity = Vector3.zero;
    private Vector3 inputVector = Vector3.zero;

    private float cooldownEndTime = 0f;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CalculateInputVector();

        Vector3 mousePositionInWorldSpace = CameraManager.Instance.MainCamera.ScreenToWorldPoint(Keybindings.MousePosition);
        mousePositionInWorldSpace.z = 0f;
        Vector2 aimVector = (mousePositionInWorldSpace - transform.position).normalized;
        if (Keybindings.Attack && Time.time >= cooldownEndTime)
        {
            cooldownEndTime = Time.time + CurrentWeapon.Description.Cooldown;
            CurrentWeapon.Shoot(aimVector, transform.position);
        }
    }

    void FixedUpdate()
    {
        CalculateDeceleration();
        CalculateVelocity();
    }

    private void CalculateInputVector()
    {
        inputVector = Vector2.zero;
        inputVector.x -= Keybindings.MoveLeft;
        inputVector.x += Keybindings.MoveRight;
        inputVector.y += Keybindings.MoveUp;
        inputVector.y -= Keybindings.MoveDown;
    }

    private void CalculateDeceleration()
    {
        if (inputVector.x <= 0f && velocity.x > 0f)
        {
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
}
