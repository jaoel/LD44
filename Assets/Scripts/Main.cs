using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public TileContainer tileContainer;
    public InteractiveDungeonObject interactiveDungeonObjectContainer;
    public ItemContainer itemContainer;
    public EnemyContainer enemyContainer;
    public Player player;

    MapGenerator _mapGen;
    Map _currentMap;
    bool _renderBSPGrid; 
    
    void Start()
    {
        _mapGen = new MapGenerator(tileContainer, interactiveDungeonObjectContainer, itemContainer,
            enemyContainer);
        _renderBSPGrid = false;

        GenerateMap();
    }

    public void GenerateMap()
    {
        _currentMap = _mapGen.GenerateDungeon(Random.Range(5, 10), 100, 100);
        _currentMap.MovePlayerToSpawn(player);
    }

    private void OnDrawGizmos()
    {
        if (_mapGen != null && _renderBSPGrid)
            _mapGen.DrawDebug();
    }

    void Update()
    {
    }
}
