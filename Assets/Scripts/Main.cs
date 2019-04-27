using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public TileContainer TileContainer;
    public InteractiveDungeonObject InteractiveDungeonObjectContainer;
    public Player Player;

    MapGenerator _mapGen;
    Map _currentMap;
    bool _renderBSPGrid; 
    
    void Start()
    {
        _mapGen = new MapGenerator(TileContainer, InteractiveDungeonObjectContainer);
        _currentMap = _mapGen.GenerateDungeon(Random.Range(100, 500), Random.Range(100, 500));
        _renderBSPGrid = false;
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
