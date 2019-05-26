using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IWeaponOwner
{
    Vector2 GetAimVector();
    Vector2 GetBulletOrigin();
    void Knockback();
    GameObject GetGameObject();
}