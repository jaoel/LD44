using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class DelaunayTriangulation
{
    public static Triangulation Triangulate(List<MapNode> vertices)
    {
        if (vertices.Count == 2)
        {
            Triangulation tri = new Triangulation();
            tri.AddVertices(vertices);
            tri.AddEdge(new Edge(vertices[0], vertices[1]));

            return tri;
        }

        if (vertices.Count == 3)
        {
            Triangulation tri = new Triangulation();
            tri.AddVertices(vertices);
            tri.AddEdge(new Edge(vertices[0], vertices[1]));
            tri.AddEdge(new Edge(vertices[1], vertices[2]));
            tri.AddEdge(new Edge(vertices[2], vertices[0]));

            return tri;
        }

        Tuple<List<MapNode>, List<MapNode>> splitList = Split(vertices);

        Triangulation left = Triangulate(splitList.Item1);
        Triangulation right = Triangulate(splitList.Item2);

        Triangulation result = new Triangulation();
        Edge baseEdge = GetBaseEdge(left, right);

        AddLREdge(ref result, left, right, baseEdge);

        result.AddVertices(left.Vertices);
        result.AddVertices(right.Vertices);
        result.AddEdges(left.Edges);
        result.AddEdges(right.Edges);

        result.AddEdge(baseEdge);

        return result;
    }

    private static void AddLREdge(ref Triangulation current, Triangulation left, Triangulation right, Edge baseEdge)
    {
        left.Vertices.ForEach(x =>
        {
            x.SetAngle(FindAngle(baseEdge, x, true));
        });

        right.Vertices.ForEach(x =>
        {
            x.SetAngle(FindAngle(baseEdge, x, false));
        });

        List<Vertex> sortedLeft = left.Vertices.OrderBy(x => x.Angle).ToList();
        List<Vertex> sortedRight = right.Vertices.OrderBy(x => x.Angle).ToList();

        sortedLeft.RemoveAll(x => x.Angle >= 180.0f || baseEdge.ContainsVertex(x));
        sortedRight.RemoveAll(x => x.Angle >= 180.0f || baseEdge.ContainsVertex(x));

        Vertex finalLeftCandidate = FindPotentialCandidate(left, sortedLeft, baseEdge, true);
        Vertex finalRightCandidate = FindPotentialCandidate(right, sortedRight, baseEdge, false);

        if (finalLeftCandidate != null && finalRightCandidate != null)
        {

        }
        else if (finalLeftCandidate != null && finalRightCandidate == null)
        {

        }
        else if (finalLeftCandidate == null && finalRightCandidate == null)
        {

        }

        /*
        left.Vertices.ForEach(x =>
        {
            x.SetAngle(FindAngle(baseEdge, x, true));  
        });

        right.Vertices.ForEach(x =>
        {
            x.SetAngle(FindAngle(baseEdge, x, false)); 
        });

        List<Vertex> sortedLeft = left.Vertices.OrderBy(x => x.Angle).ToList();
        List<Vertex> sortedRight = right.Vertices.OrderBy(x => x.Angle).ToList();

        sortedLeft.RemoveAll(x => x.Angle >= 180.0f * Mathf.Deg2Rad);
        sortedRight.RemoveAll(x => x.Angle >= 180.0f * Mathf.Deg2Rad);  
        
        Vertex finalLeftCandidate = FindPotentialCandidate(left, sortedLeft, baseEdge, true);
        Vertex finalRightCandidate = FindPotentialCandidate(right, sortedRight, baseEdge, false);

        if (current.Edges.Count > 100)
        {
            Debug.Log("looping");
            return;
        }

        if (finalLeftCandidate != null && finalRightCandidate != null)
        {
            Circle circle = new Circle(baseEdge.Origin, baseEdge.Target, finalLeftCandidate);
            if (!circle.PointInCircle(finalRightCandidate.Position))
            {
                current.AddEdge(new Edge(finalLeftCandidate, baseEdge.Target));
                AddLREdge(ref current, left, right, baseEdge);
            }
            else
            {
                current.AddEdge(new Edge(finalRightCandidate, baseEdge.Origin));
                AddLREdge(ref current, left, right, baseEdge);
            }
        }
        else if (finalLeftCandidate != null && finalRightCandidate == null)
        {
            current.AddEdge(new Edge(finalLeftCandidate, baseEdge.Target));
            AddLREdge(ref current, left, right, current.Edges.Last());
        }
        else if (finalLeftCandidate == null && finalRightCandidate != null)
        {
            current.AddEdge(new Edge(finalRightCandidate, baseEdge.Origin));
            AddLREdge(ref current, left, right, current.Edges.Last());
        }  
        */
    }

    public static Vertex FindPotentialCandidate(Triangulation triangulation, List<Vertex> vertices, Edge edge, bool left)
    {
        if (vertices.Count == 0)
            return null;

        if (vertices.Count == 1)
            return vertices[0];

        Vertex candidate = vertices[0];
        Vertex nextCandidate = vertices[1];

        Circle circumCircle = new Circle(edge.Origin, edge.Target, candidate);
        if (!circumCircle.PointInCircle(nextCandidate.Position))
        {
            return candidate;
        }
        else
        {
            vertices.RemoveAt(0);

            if (left)
            {
                Debug.Log("Delete LL edges");
            }
            else
            {
                Debug.Log("Delete LR edges");
            }

            return FindPotentialCandidate(triangulation, vertices, edge, left);
        }   
    }

    public static double FindAngle(Edge baseEdge, Vertex candidate, bool left)
    {
        Vector2 baseVector = Vector2.zero;
        Vector2 toCandidate = Vector2.zero;
        double angle = 0.0;
        if (left)
        {
            baseVector = (baseEdge.Target.Position - baseEdge.Origin.Position).normalized;
            toCandidate = (candidate.Position - baseEdge.Origin.Position).normalized;
            angle = Vector2.SignedAngle(baseVector, toCandidate);
        }
        else
        {
            baseVector = (baseEdge.Origin.Position - baseEdge.Target.Position).normalized;
            toCandidate = (candidate.Position - baseEdge.Target.Position).normalized;
            angle = Vector2.SignedAngle(toCandidate, baseVector);
        }

        if (Math.Sign(angle) == -1)
            angle += 360.0;

        return angle;

        /*
        var p1 = new Vector2();
        var p0 = new Vector2();

        if (!left)
        {
            p0.Set(baseEdge.Target.Position.x, baseEdge.Target.Position.y);
            p1.Set(baseEdge.Origin.Position.x, baseEdge.Origin.Position.y);
        }
        else
        {
            p0.Set(baseEdge.Origin.Position.x, baseEdge.Origin.Position.y);
            p1.Set(baseEdge.Target.Position.x, baseEdge.Target.Position.y);
        }
        var p2 = new Vector2(candidate.Position.x, candidate.Position.y);

        var v1 = new Vector2(p1.x - p0.x, p1.y - p0.y);
        var v2 = new Vector2(p2.x - p0.x, p2.y - p0.y);

        var dot = v1.x * v2.x + v1.y * v2.y;
        var det = v1.x * v2.y + v1.y * v2.x;
        return Math.Atan2(det, dot);
        */

        return 0.0;
    }

    private static Edge GetBaseEdge(Triangulation left, Triangulation right)
    {
        Vertex lowestLeft = left.GetLowestVertex();
        Vertex lowestRight = right.GetLowestVertex();

        return new Edge(lowestLeft, lowestRight);
    }
    
    private static Edge CreateEdge(MapNode origin, MapNode target)
    {
        return new Edge(origin, target);
    } 

    private static Tuple<List<MapNode>, List<MapNode>> Split(in List<MapNode> nodes)
    {
        List<MapNode> first = nodes.Take(nodes.Count / 2).ToList();
        List<MapNode> second = nodes.Skip(nodes.Count / 2).ToList();

        return new Tuple<List<MapNode>, List<MapNode>>(first, second);
    } 
} 
