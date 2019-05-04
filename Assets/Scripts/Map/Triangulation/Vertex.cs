using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Vertex
{
    private MapNode _node;
    private double _angle;

    public RectInt Cell => _node.Cell;
    public Vector2 Position => _node.Cell.center;
    public double Angle => _angle;
    public MapNode Node => _node;

    public Vertex()
    {

    }

    public Vertex(MapNode node)
    {
        _node = node;
    }

    public void SetNode(MapNode node)
    {
        _node = node;
    }

    public void SetAngle(double angle)
    {
        _angle = angle;
    }
}
