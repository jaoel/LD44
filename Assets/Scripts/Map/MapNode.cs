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

    public MapNode(int id, Vector2Int position, Vector2Int size)
    {
        Id = id;
        Cell = new RectInt(position, size);
    }
} 
