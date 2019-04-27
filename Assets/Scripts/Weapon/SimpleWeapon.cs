using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SimpleWeapon : Weapon
{
    public override void Shoot(Vector2 aimVector, Vector2 position)
    {
        BulletManager.Instance.SpawnBullet(Description.BulletDescription, position, aimVector * Description.BulletSpeed);
    }
} 
