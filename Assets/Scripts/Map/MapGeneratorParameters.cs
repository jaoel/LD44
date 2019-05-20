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
}

public class MapPopulationParameters
{
    public SortedDictionary<int, float> ZombieDensity { get; set; } = new SortedDictionary<int, float>();
    public SortedDictionary<int, float> SkeletonDensity { get; set; } = new SortedDictionary<int, float>();
    public SortedDictionary<int, float> SkeleramboDensity { get; set; } = new SortedDictionary<int, float>();

    public float GetDensity(SortedDictionary<int, float> densityScale, int level)
    {
        KeyValuePair<int, float> lowestAbove = densityScale.Where(x => x.Key >= level).FirstOrDefault();
        KeyValuePair<int, float> highestBelow = densityScale.OrderByDescending(x => x.Key).Where(x => x.Key < level).FirstOrDefault();

        float scaledLevel = Utility.ConvertRange(highestBelow.Key, lowestAbove.Key, 0.0f, 1.0f, level);
        float result = Mathf.Lerp(highestBelow.Value, lowestAbove.Value, scaledLevel);
        return result;
    }
}
