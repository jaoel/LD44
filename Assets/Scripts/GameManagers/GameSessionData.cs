using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GameSessionData
{
    public GameObject slingshotPrefab;

    [SerializeField]
    public GameObject defaultWeapon;

    [SerializeField]
    public int playerMaxHealth;

    public List<int> shopTiers = new List<int>();
    public List<Item> ownedItems = new List<Item>();

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

    public void UpdatePlayerData(Player player)
    {
        playerMaxHealth = player.MaxHealth;
    }

    public void AddItem(Item item)
    {
        ownedItems.Add(item);
        Armory.Instance.UpdateArmory();
    }

    private void SetDefaultData()
    {
        shopTiers = new List<int>();
        ownedItems = new List<Item>();

        ownedItems.Add(slingshotPrefab.GetComponent<Item>());
    }
}
