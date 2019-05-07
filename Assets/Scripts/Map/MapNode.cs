using System.Collections.Generic;
using UnityEngine;

public enum MapNodeType
{
    Default,
    Room,   
    Corridor,
    None
}

public class MapNode
{
    public int Id { get; set; }
    public RectInt Cell { get; set; }
    public MapNodeType Type { get; set; }
    public List<Vector2Int> EntryPoints { get; set; }

    public bool HasEntryPoint => EntryPoints.Count > 0;

    public MapNode(int id, Vector2Int position, Vector2Int size, bool useDefaultEntryPoint = true)
    {
        Id = id;
        Cell = new RectInt(position, size);
        EntryPoints = new List<Vector2Int>();
    }

    public Vector2Int GetClosestEntryPoint(Vector2 position)
    {
        return GetClosestEntryPoint(new Vector2Int((int)position.x, (int)position.y));
    }

    public Vector2Int GetClosestEntryPoint(Vector2Int position)
    {
        Vector2Int result = Vector2Int.zero;
        float minDist = float.PositiveInfinity;
        foreach(Vector2Int entryPoint in EntryPoints)
        {
            float sqrDist = Vector2.SqrMagnitude(entryPoint - position);
            if (sqrDist < minDist)
            {
                result = entryPoint;
                minDist = sqrDist;
            }
        }

        return result;
    }
} 
