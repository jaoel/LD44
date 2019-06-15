using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GameSessionData
{
    [SerializeField]
    public Weapon defaultWeapon;

    [SerializeField]
    public int playerMaxHealth;


    public void AddWeapons(Player player)
    {
        if (player.CurrentWeapon == null)
        {
            player.CurrentWeapon = GameObject.Instantiate(defaultWeapon, player.transform).GetComponent<Weapon>();
            player.CurrentWeapon.SetOwner(player, true);
            UIManager.Instance.playerUI.weaponImage.sprite = player.CurrentWeapon.uiImage;
        }

        //if (player.CurrentWeapon != null)
        //{
        //    GameObject oldWeapon = player.CurrentWeapon?.gameObject;
        //    oldWeapon.SetActive(false);
        //    GameObject.Destroy(oldWeapon);
        //}    
    }

    public void LoadData()
    {
        SetDefaultData();
    }

    public void UpdateSessionData(Player player)
    {
        playerMaxHealth = player.MaxHealth;
    }

    private void SetDefaultData()
    {
        //playerMaxHealth = 30;
    }
}
