using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponPickup : Item
{
    public GameObject weaponPrefab;

    public override void ApplyEffect(GameObject owner)
    {
        GameObject oldWeapon = owner.GetComponent<Player>().CurrentWeapon.gameObject;
        oldWeapon.SetActive(false);
        Destroy(oldWeapon);

        owner.GetComponent<Player>().CurrentWeapon = GameObject.Instantiate(weaponPrefab, owner.transform).GetComponent<Weapon>();
        owner.GetComponent<Player>().CurrentWeapon.SetOwner(owner.GetComponent<Player>(), true);
        UIManager.Instance.playerUI.weaponImage.sprite = owner.GetComponent<Player>().CurrentWeapon.uiImage;
    }
}
