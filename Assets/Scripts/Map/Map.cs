using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Map
{
    public List<MapNode> _cells;
    public List<Delaunay.Triangle<MapNode>> Triangles;
    public List<Delaunay.Edge<MapNode>> DelaunayGraph;
    public List<Delaunay.Edge<MapNode>> GabrielGraph;
    public List<Delaunay.Edge<MapNode>> EMSTGraph;
    public List<Delaunay.Edge<MapNode>> CorridorGraph;

    public void DrawDebug()
    {
        DrawCells();
    }

    private void DrawCells()
    {
        if (_cells != null)
        {
            _cells.ForEach(x =>
            {
                switch (x.Type)
                {
                    case MapNodeType.Default:
                        DrawRectangle(x.Cell, Color.black);
                        break;
                    case MapNodeType.Room:
                        DrawRectangle(x.Cell, Color.green);   
                        break;
                    default:
                        break;
                }
            });
        }   

        if (DelaunayGraph != null)
        {
            DelaunayGraph.ForEach(x =>
            {
                //DrawLine(x, Color.cyan);
            });
        }

        if (GabrielGraph != null)
        {
            GabrielGraph.ForEach(x =>
            {
                //DrawLine(x, Color.magenta);
            });
        }

        if (EMSTGraph != null)
        {
            EMSTGraph.ForEach(x =>
            {
                //DrawLine(x, Color.cyan);
            });
        }

        if (CorridorGraph != null)
        {
            CorridorGraph.ForEach(x =>
            {
                DrawLine(x, Color.red);
            });
        }
    }

    private void DrawRectangle(RectInt rect, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3Int(rect.xMax, rect.y, 0));
        Gizmos.DrawLine(new Vector3(rect.xMax, rect.y, 0), new Vector3Int(rect.xMax, rect.yMax, 0));
        Gizmos.DrawLine(new Vector3(rect.x, rect.yMax, 0), new Vector3Int(rect.xMax, rect.yMax, 0));
        Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3Int(rect.x, rect.yMax, 0));
    }

    private void DrawLine(Delaunay.Edge<MapNode> edge, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(new Vector3(edge.Point1.Position.x, edge.Point1.Position.y, 0), 
            new Vector3(edge.Point2.Position.x, edge.Point2.Position.y, 0)); 
    }

    private void DrawText(Vector2 position, Color color)
    {
        Gizmos.color = color;
        Handles.Label(new Vector3(position.x, position.y, 0.0f), position.x + " : " + position.y);
    }
}
