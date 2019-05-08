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

    public BoundsInt Bounds;

    private bool _drawCells;
    private bool _drawDelaunay;
    private bool _drawGabriel;
    private bool _drawEMST;
    private bool _drawCorridors;

    public Map()
    {
        _drawCells = false;
        _drawDelaunay = false;
        _drawGabriel = false;
        _drawEMST = false;
        _drawCorridors = false;
    }

    public void DrawDebug()
    {
        DrawCells();
    }

    private void DrawCells()
    {
        if (_drawCells && _cells != null)
        {
            _cells.ForEach(x =>
            {
                switch (x.Type)
                {
                    case MapNodeType.Default:
                        GizmoUtility.DrawRectangle(x.Cell, Color.black);
                        break;
                    case MapNodeType.Room:
                        GizmoUtility.DrawRectangle(x.Cell, Color.green);   
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
