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
    public List<Delaunay.Edge<MapNode>> Corridors { get; set; }
    public List<BoundsInt> Chokepoints { get; set; }
    public bool Lockable { get; set; }
    public bool Locked { get; set; }
    public bool ContainsStairs { get; set; }
    public List<GameObject> Keys { get; set; }
    public List<GameObject> Enemies { get; set; }
    public float SeclusionFactor { get; set; }
    public bool HasEntryPoint => EntryPoints.Count > 0;

    public MapNode(int id, Vector2Int position, Vector2Int size, bool useDefaultEntryPoint = true)
    {
        Id = id;
        Cell = new RectInt(position, size);
        EntryPoints = new List<Vector2Int>();
        Corridors = new List<Delaunay.Edge<MapNode>>();
        Chokepoints = new List<BoundsInt>();
        Keys = new List<GameObject>();
        Enemies = new List<GameObject>();
    }

    public Vector2Int GetClosestEntryPoint(Vector2 position)
    {
        return GetClosestEntryPoint(new Vector2Int((int)position.x, (int)position.y));
    }

    public Vector2Int GetClosestEntryPoint(Vector2Int position)
    {
        Vector2Int result = Vector2Int.zero;
        float minDist = float.PositiveInfinity;
        foreach (Vector2Int entryPoint in EntryPoints)
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

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (this.GetType() != obj.GetType())
        {
            return false;
        }

        MapNode other = (MapNode)obj;

        if (other.Id == Id)
        {
            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
