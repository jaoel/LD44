using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public WeaponDescription Description;

    public abstract void Shoot(Vector2 aimVector, Vector2 position, GameObject owner);
}  
