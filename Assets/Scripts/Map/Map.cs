using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Map
{
    public List<MapNode> Cells { get; set; }
    public List<Delaunay.Triangle<MapNode>> Triangles { get; set; }
    public List<Delaunay.Edge<MapNode>> DelaunayGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> GabrielGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> EMSTGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> CorridorGraph { get; set; }
    public int[,] CollisionMap { get; set; }
    public BoundsInt Bounds { get; set; }

    private bool _drawCells;
    private bool _drawDelaunay;
    private bool _drawGabriel;
    private bool _drawEMST;
    private bool _drawCorridors;

    public Map()
    {
        _drawCells = true;
        _drawDelaunay = false;
        _drawGabriel = false;
        _drawEMST = false;
        _drawCorridors = true;
    }

    public void DrawDebug()
    {
        DrawCells();
    }

    private void DrawCells()
    {
        if (_drawCells && Cells != null)
        {
            Cells.ForEach(x =>
            {
                switch (x.Type)
                {
                    case MapNodeType.Default:
                        GizmoUtility.DrawRectangle(x.Cell, Color.black);
                        break;
                    case MapNodeType.Room:
                        GizmoUtility.DrawRectangle(x.Cell, Color.green);   
                        break;
                    case MapNodeType.Corridor:
                        GizmoUtility.DrawRectangle(x.Cell, Color.blue);
                        break;
                    default:
                        break;
                }
            });
        }   

        if (_drawDelaunay && DelaunayGraph != null)
        {
            DelaunayGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.cyan);
            });
        }

        if (_drawGabriel && GabrielGraph != null)
        {
            GabrielGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.magenta);
            });
        }

        if (_drawEMST && EMSTGraph != null)
        {
            EMSTGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.cyan);
            });
        }

        if (_drawCorridors && CorridorGraph != null)
        {
            CorridorGraph.ForEach(x =>
            {
               GizmoUtility.DrawLine(x, Color.red);
            });
        }
    }

   
}
