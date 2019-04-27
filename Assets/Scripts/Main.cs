using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    MapGenerator _mapGen;
    BSPTree _bspTree;

    public List<Tile> Tiles;
                     
    void Start()
    {
        _mapGen = new MapGenerator(Tiles);
    }

    private void OnDrawGizmos()
    {
        if (_mapGen != null)
            _mapGen.DrawDebug();
    }

    void Update()
    {
        
    }
}
