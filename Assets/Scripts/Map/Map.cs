using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Map
{
    public List<MapNode> _cells;
    public List<Delaunay.Triangle<MapNode>> Triangles;
    //public Triangulation _triangulation;

    //public List<QuadEdge<int>> _edges;

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
                    case MapNodeType.Room:
                        DrawRectangle(x.Cell, Color.green);
                        DrawText(x.Cell.center, Color.cyan);
                        break;
                    case MapNodeType.Corridor:
                        DrawRectangle(x.Cell, Color.blue);
                        break;
                    default:
                        break;
                }
            });
        }   

        if (Triangles != null)
        {
            Triangles.ForEach(x =>
            {
                Gizmos.color = Color.cyan;

                Gizmos.DrawLine(new Vector3(x.Vertices[0].Position.x, x.Vertices[0].Position.y, 0), new Vector3(x.Vertices[1].Position.x, x.Vertices[1].Position.y, 0));
                Gizmos.DrawLine(new Vector3(x.Vertices[1].Position.x, x.Vertices[1].Position.y, 0), new Vector3(x.Vertices[2].Position.x, x.Vertices[2].Position.y, 0));
                Gizmos.DrawLine(new Vector3(x.Vertices[2].Position.x, x.Vertices[2].Position.y, 0), new Vector3(x.Vertices[0].Position.x, x.Vertices[0].Position.y, 0));
            });
        }

        //if (_triangulation != null)
        //{
            //_triangulation.Edges.ForEach(x =>
            //{
            //    DrawLine(x, new Color(165 / 255.0f, 55 / 255.0f, 253 / 255.0f));
            //});
            //
            //DrawLine(_triangulation.Edges.First(), Color.blue);
            //DrawLine(_triangulation.Edges[1], Color.magenta);
        //}

        //if (_edges != null)
        //{
        //    _edges.ForEach(x =>
        //    {
        //        DrawLine(x, new Color(165 / 255.0f, 55 / 255.0f, 253 / 255.0f));
        //    });
        //}
    }

    private void DrawRectangle(RectInt rect, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3Int(rect.xMax, rect.y, 0));
        Gizmos.DrawLine(new Vector3(rect.xMax, rect.y, 0), new Vector3Int(rect.xMax, rect.yMax, 0));
        Gizmos.DrawLine(new Vector3(rect.x, rect.yMax, 0), new Vector3Int(rect.xMax, rect.yMax, 0));
        Gizmos.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3Int(rect.x, rect.yMax, 0));
    }

    //private void DrawLine(Edge edge, Color color)
    //{
    //    Gizmos.color = color;
    //    Gizmos.DrawLine(new Vector3(edge.Origin.Cell.center.x, edge.Origin.Cell.center.y, 0), new Vector3(edge.Target.Cell.center.x, edge.Target.Cell.center.y, 0));
    //
    //}

    //private void DrawLine(QuadEdge<int> edge, Color color)
    //{
    //    Gizmos.color = color;
    //    Gizmos.DrawLine(new Vector3(edge.Origin.x, edge.Origin.y, 0), new Vector3(edge.Destination.x, edge.Destination.y, 0));
    //
    //}

    private void DrawText(Vector2 position, Color color)
    {
        Gizmos.color = color;
        Handles.Label(new Vector3(position.x, position.y, 0.0f), position.x + " : " + position.y);
    }
}
