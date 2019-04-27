using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public TileContainer TileContainer;
    public InteractiveDungeonObject InteractiveDungeonObjectContainer;
    public ItemContainer ItemContainer;
    public Player Player;

    MapGenerator _mapGen;
    Map _currentMap;
    bool _renderBSPGrid; 
    
    void Start()
    {
        _mapGen = new MapGenerator(TileContainer, InteractiveDungeonObjectContainer, ItemContainer);
        _renderBSPGrid = false;

        GenerateMap();
    }

    public void GenerateMap()
    {
        _currentMap = _mapGen.GenerateDungeon(Random.Range(1, 10), Random.Range(100, 500), Random.Range(100, 500));
        _currentMap.MovePlayerToSpawn(Player);
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
