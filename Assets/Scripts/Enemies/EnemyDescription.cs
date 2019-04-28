using UnityEngine;

[CreateAssetMenu]
public class EnemyDescription : ScriptableObject
{
    public int maxHealth;
    public float maxSpeed;
    public float acceleration;
    public float aggroDistance;
    public int damage;
    public float shotCooldown;
} 
