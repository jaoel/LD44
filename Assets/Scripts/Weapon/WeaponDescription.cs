using UnityEngine;
        
[CreateAssetMenu]
public class WeaponDescription : ScriptableObject
{
    public float cooldown;
    public float bulletSpeed;
    public BulletDescription bulletDescription;
    public GameObject soundPrefab;
    public bool loopSound;
}  
