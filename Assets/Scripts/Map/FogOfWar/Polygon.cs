using System.Collections.Generic;
using UnityEngine;

public class Polygon2D
{
    private List<Vector2> _vertices = new List<Vector2>();
    public List<Vector2> Vertices { get { return _vertices; } set { _vertices = value; } }

    public void DebugDraw()
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            Vector2 p0 = _vertices[i];
            Vector2 p1 = _vertices[(i + 1) % _vertices.Count];
            Debug.DrawLine(p0, p1, Color.magenta);
        }
    }

    public float SignedClosestDistance(Vector2 point)
    {
        float dist = float.MaxValue;
        int intersectionCount = 0;
        for(int i = 0; i < _vertices.Count - 1; i++)
        {
            Vector2 p0 = _vertices[i];
            Vector2 p1 = _vertices[i + 1];
            dist = Mathf.Min(dist, DistanceToLineSegment(ref p0, ref p1, ref point));
            if(IntersectsPointRay(ref p0, ref p1, ref point))
            {
                intersectionCount++;
            }
        }
        return dist * (intersectionCount % 2 == 0 ? 1f : -1f);
    }

    Vector2 lineVector;
    Vector2 proj;
    Vector2 pointMinusP0;
    Vector2 p1Minusp0;
    float lengthSqr;
    float t;
    public float DistanceToLineSegment(ref Vector2 p0, ref Vector2 p1, ref Vector2 point)
    {
        p1Minusp0.x = p1.x - p0.x;
        p1Minusp0.y = p1.y - p0.y;
        lineVector = p1Minusp0;
        lengthSqr = lineVector.sqrMagnitude;

        // p0 == p1
        if(lengthSqr == 0f)
        {
            return Vector2.Distance(p0, point);
        }

        pointMinusP0.x = point.x - p0.x;
        pointMinusP0.y = point.y - p0.y;
        t = (pointMinusP0.x * p1Minusp0.x + pointMinusP0.y * p1Minusp0.y) / lengthSqr;
        if (t < 0f)
        {
            t = 0f;
        }
        else if (t > 1f)
        {
            t = 1f;
        }
        proj.x = p0.x + t * p1Minusp0.x;
        proj.y = p0.y + t * p1Minusp0.y;
        return Vector2.Distance(point, proj);
    }

    private bool IntersectsPointRay(ref Vector2 p0, ref Vector2 p1, ref Vector2 point)
    {
        return ((p0.y > point.y) != (p1.y > point.y)) && (point.x < (p1.x - p0.x) * (point.y - p0.y) / (p1.y - p0.y) + p0.x);
    }
}
