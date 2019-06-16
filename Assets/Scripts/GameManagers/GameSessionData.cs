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
    public GameObject defaultWeapon;

    [SerializeField]
    public int playerMaxHealth;

    public List<int> shopTiers = new List<int>();


    public void AddWeapons(Player player)
    {
        if (player.CurrentWeapon == null)
        {
            player.AddWeapon(defaultWeapon);
        }
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
        shopTiers = new List<int>();
    }
}
