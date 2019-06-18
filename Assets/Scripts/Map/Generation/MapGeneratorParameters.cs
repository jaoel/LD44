using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MapGeneratorParameters
{
    public int GenerationRadius { get; set; }

    public int MaxCellCount { get; set; }
    public int MinCellCount { get; set; }

    public int MaxCellSize { get; set; }
    public int MinCellSize { get; set; }

    public float RoomThresholdMultiplier { get; set; }
    public float CorridorRoomConnectionFactor { get; set; }
    public float MazeFactor { get; set; }
    public int MinCorridorWidth { get; set; }
    public int MaxCorridorWidth { get; set; }

    public int MinRoomDistance { get; set; }
    public float LockFactor { get; set; }
    public float PitFrequency { get; set; }
}
