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

    public List<GameObject> Weapons = new List<GameObject>();


    public void AddWeapons(Player player)
    {
        if (Weapons.Count == 0)
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
        Weapons = new List<GameObject>();
    }
}
