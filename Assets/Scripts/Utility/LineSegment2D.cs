using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LineSegment2D
{
    private Vector2 _from;
    private Vector2 _to;
    private Vector2 _direction;
    private float _distSquared;

    public Vector2 From => _from;
    public Vector2 To => _to;

    public LineSegment2D(Vector2 from, Vector2 to)
    {
        _from = from;
        _to = to;

        _direction = _to - _from;
        _distSquared = _direction.sqrMagnitude;
    }

    public float DistanceToPoint(Vector2 point)
    {
        if (_distSquared == 0.0f)
        {
            return (point - _from).magnitude;
        }

        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(point - _from, _direction) / _distSquared));
        Vector2 projection = _from + t * _direction;

        return (point - projection).magnitude;
    }
}
