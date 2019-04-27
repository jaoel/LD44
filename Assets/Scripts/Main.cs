using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public TileContainer TileContainer;
    public InteractiveDungeonObject InteractiveDungeonObjectContainer;

    MapGenerator _mapGen;
    BSPTree _bspTree;
    bool _renderBSPGrid; 
    
    void Start()
    {
        _mapGen = new MapGenerator(TileContainer, InteractiveDungeonObjectContainer,
            Random.Range(100, 500), Random.Range(100, 500));
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
