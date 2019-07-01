using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class DungeonData :   MonoBehaviour
{
    [SerializeField]
    public List<int> bossHealthCosts = new List<int>();

    [SerializeField]
    public MapGeneratorParameters parameters;

    [SerializeField]
    public TileContainer tileSet;

    [SerializeField]
    public TileContainer pitSet;

    [SerializeField]
    public TrapContainer trapSet;

    [SerializeField]
    public SpawnableContainer spawnables;

    [SerializeField]
    public InteractiveDungeonObject interactiveObjects;

    [SerializeField]
    public string bossScene;
}
