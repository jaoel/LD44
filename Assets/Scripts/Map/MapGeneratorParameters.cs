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

    /*
    public int MaxRoomCount { get; set; }
    public int MinRoomCount { get; set; }

    public int MaxLargeRoomCount { get; set; }
    public int MinLargeRoomCount { get; set; }

    public int MaxMediumRoomCount { get; set; }
    public int MinMediumRoomCount { get; set; }

    public int MaxSmallRoomCount { get; set; }
    public int MinSmallRoomCount { get; set; }
    */
    //public int SmallRoomMinArea { get; set; }

    /*
    public int MediumRoomMinArea { get; set; }
    public int LargeRoomMinArea { get; set; }
    */
}
