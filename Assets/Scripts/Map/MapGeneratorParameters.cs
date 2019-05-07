using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MapGeneratorParameters
{
    public int GenerationRadius { get; set; }

    public int MaxCellCount { get; set; }
    public int MinCellCount { get; set; }

    public int MaxCellSize { get; set; }
    public int MinCellSize { get; set; }

    public int MinRoomWidth { get; set; }
    public int MinRoomHeight { get; set; }
    public int MinCorridorWidth { get; set; }
    public int MaxCorridorWidth { get; set; }
}
