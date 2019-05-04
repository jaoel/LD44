using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Circle
{
    private Vector2 _center;
    private double _radius;

    public Vector2 Center => _center;
    public double Radius => _radius;

    public Circle(Vector2 center, double radius)
    {
        _center = center;
        _radius = radius;
    }

    public Circle(Vertex v1, Vertex v2, Vertex v3)
    {
        float x1 = v1.Position.x;
        float x2 = v2.Position.x;
        float x3 = v3.Position.x;
        float y1 = v1.Position.y;
        float y2 = v2.Position.y;
        float y3 = v3.Position.y;

        Vector2 midPt1 = new Vector2((x1 + x2) / 2, (y1 + y2) / 2);
        Vector2 midPt2 = new Vector2((x1 + x3) / 2, (y1 + y3) / 2);

        float k1 = -(x2 - x1) / (y2 - y1);
        float k2 = -(x3 - x1) / (y3 - y1);

        float centerX = (midPt2.y - midPt1.y - k2 * midPt2.x + k1 * midPt1.x) / (k1 - k2);
        float centerY = midPt1.y + k1 * (midPt2.y - midPt1.y - k2 * midPt2.x + k2 * midPt1.x) / (k1 - k2);

        _center = new Vector2(centerX, centerY);
        _radius = Math.Sqrt((centerX - x1) * (centerX - x1) + (centerY - y1) * (centerY - y1));
    }

    public bool PointInCircle(Vector2 point)
    {
        var distance = Math.Sqrt((point.x - _center.x) * (point.x -_center.x) + (point.y - _center.y) * (point.y - _center.y));
        return distance <= _radius;
    }
}  
