using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
        
[CreateAssetMenu]
public class WeaponDescription : ScriptableObject
{
    public float Cooldown;
    public float BulletSpeed;
    public BulletDescription BulletDescription; 
}  
