﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;


[Serializable]
public class TileProbabilityList : ProbabilityList<Tile>
{

}

[CreateAssetMenu]
public class TileContainer : ScriptableObject
{
    public TileProbabilityList FloorTiles;
    public Tile TopRight;
    public Tile TopMiddle;
    public Tile TopLeft;
    public Tile MiddleLeft;
    public Tile MiddleRight;
    public Tile BottomRight;
    public Tile BottomMiddle;
    public Tile BottomLeft;

    public Tile TopRightOuter;
    public Tile TopLeftOuter;
    public Tile BottomRightOuter;
    public Tile BottomLeftOuter;
}
