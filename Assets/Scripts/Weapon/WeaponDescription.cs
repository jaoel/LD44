﻿using UnityEngine;
        
[CreateAssetMenu]
public class WeaponDescription : ScriptableObject
{
    public float Cooldown;
    public float BulletSpeed;
    public BulletDescription BulletDescription;
    public GameObject soundPrefab;
    public bool loopSound;
}  
