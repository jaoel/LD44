using UnityEditor;
using UnityEngine;

public static class GizmoUtility
 {
    public static void DrawRectangle(RectInt rect, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3Int(rect.xMax, rect.y, 0));
        Gizmos.DrawLine(new Vector3(rect.xMax, rect.y, 0), new Vector3Int(rect.xMax, rect.yMax, 0));
        Gizmos.DrawLine(new Vector3(rect.x, rect.yMax, 0), new Vector3Int(rect.xMax, rect.yMax, 0));
        Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3Int(rect.x, rect.yMax, 0));
    }

    public static void DrawLine(Delaunay.Edge<MapNode> edge, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(new Vector3(edge.Point1.Position.x, edge.Point1.Position.y, 0),
            new Vector3(edge.Point2.Position.x, edge.Point2.Position.y, 0));
    }

    public static void DrawText(Vector2 position, Color color)
    {
        Gizmos.color = color;
        Handles.Label(new Vector3(position.x, position.y, 0.0f), position.x + " : " + position.y);
    }
}
