using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Armory : MonoBehaviour 
{
    public List<ArmoryTrigger> spawnPositions = new List<ArmoryTrigger>();
    private List<GameObject> _armoryItems = new List<GameObject>();

    private static Armory _instance;

    public static Armory Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<Armory>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a Armory");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        
    }

    private void Start()
    {
        UpdateArmory();
    }

    public void UpdateArmory()
    {
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            if (i >= Main.Instance.sessionData.ownedItems.Count)
            {
                break;
            }

            spawnPositions[i].SetPrefab(Main.Instance.sessionData.ownedItems[i]);
        }
    }
}
