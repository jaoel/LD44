using System.Collections.Generic;
using UnityEngine;

public class Map
{
    public List<MapNode> _cells;

    public void DrawDebug()
    {
        DrawCells();
    }

    private void DrawCells()
    {
        if (_cells == null)
            return;

        _cells.ForEach(x =>
        {
            switch (x.Type)
            {
                case MapNodeType.Room:
                    DrawRectangle(x.Cell, Color.green);
                    break;
                case MapNodeType.Corridor:
                    DrawRectangle(x.Cell, Color.blue);
                    break;
                default:
                    break;
            }
        });
    }

    private void DrawRectangle(RectInt rect, Color color)
    {
        // Container
        Gizmos.color = color;
        // top
        Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3Int(rect.xMax, rect.y, 0));
        // right
        Gizmos.DrawLine(new Vector3(rect.xMax, rect.y, 0), new Vector3Int(rect.xMax, rect.yMax, 0));
        // bottom
        Gizmos.DrawLine(new Vector3(rect.x, rect.yMax, 0), new Vector3Int(rect.xMax, rect.yMax, 0));
        // left
        Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3Int(rect.x, rect.yMax, 0));
    }
}
