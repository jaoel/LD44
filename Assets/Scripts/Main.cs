using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public TileContainer TileContainer;

    MapGenerator _mapGen;
    BSPTree _bspTree;
    bool _renderBSPGrid; 
    
    void Start()
    {
        _mapGen = new MapGenerator(TileContainer);
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
