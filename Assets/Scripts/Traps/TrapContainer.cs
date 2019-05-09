using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TrapContainer : ScriptableObject
{
    public GameObject slowTrap;
    public GameObject spikeTrap;

    public GameObject GetRandomTrap()
    {
        List<GameObject> traps = GetTrapList();
        return traps[Random.Range(0, traps.Count)];
    }

    private List<GameObject> GetTrapList()
    {
        return new List<GameObject>()
        {
           slowTrap,
           spikeTrap
        };
    }
}
