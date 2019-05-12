using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class BowerWatsonDelaunay
    {
        public BowerWatsonDelaunay()
        {
        }

        public IEnumerable<Triangle<MapNode>> Triangulate(IEnumerable<Vertex<MapNode>> vertices)
        {
            Triangle<MapNode> supraTriangle = GetSupraTriangle(vertices);
            HashSet<Triangle<MapNode>> triangulation = new HashSet<Triangle<MapNode>>() { supraTriangle };

            foreach (Vertex<MapNode> vertex in vertices)
            {
                ISet<Triangle<MapNode>> badTriangles = FindBadTriangles(vertex, triangulation);
                List<Edge<MapNode>> polygon = FindHoleBoundaries(badTriangles);

                foreach (Triangle<MapNode> triangle in badTriangles)
                {
                    foreach (Vertex<MapNode> badTriangleVertex in triangle.Vertices)
                    {
                        badTriangleVertex.AdjacentTriangles.Remove(triangle);
                    }
                }

                int trianglesRemoved = triangulation.RemoveWhere(o => badTriangles.Contains(o));

                foreach (Edge<MapNode> edge in polygon)
                {
                    var triangle = new Triangle<MapNode>(vertex, edge.Point1, edge.Point2);
                    triangulation.Add(triangle);
                }
            }

            triangulation.RemoveWhere(x => x.Vertices.Any(v => supraTriangle.Vertices.Contains(v)));
            return triangulation;
        }

        public HashSet<Edge<MapNode>> GetDelaunayEdges(IEnumerable<Triangle<MapNode>> triangles)
        {
            HashSet<Edge<MapNode>> result = new HashSet<Edge<MapNode>>();
            foreach (Triangle<MapNode> triangle in triangles)
            {
                foreach (Edge<MapNode> edge in triangle.Edges)
                {
                    result.Add(edge);
                }
            }

            return result;
        }

        public HashSet<Edge<MapNode>> GetGabrielGraph(in HashSet<Edge<MapNode>> delaunayEdges, in IEnumerable<Vertex<MapNode>> vertices)
        {
            HashSet<Edge<MapNode>> result = new HashSet<Edge<MapNode>>(delaunayEdges);
            List<Edge<MapNode>> removalList = new List<Edge<MapNode>>();

            foreach (Edge<MapNode> edge in result)
            {
                foreach (Vertex<MapNode> vertex in vertices)
                {
                    if (edge.CircumCircleContainsPoint(vertex))
                    {
                        removalList.Add(edge);
                        break;
                    }
                }
            }

            result.RemoveWhere(x => removalList.Contains(x));
            return result;
        }

        public HashSet<Edge<MapNode>> GetPrimEMST(in HashSet<Edge<MapNode>> gabrielGraph, in IEnumerable<Vertex<MapNode>> vertices)
        {
            List<Edge<MapNode>> emst = new List<Edge<MapNode>>();
            List<Vertex<MapNode>> emstVertices = new List<Vertex<MapNode>>();
            List<Vertex<MapNode>> graphVertices = vertices.ToList();
            emstVertices.Add(graphVertices.ElementAt(0));
            while (emstVertices.Count != graphVertices.Count)
            {
                foreach (Vertex<MapNode> vertex in graphVertices)
                {
                    if (emstVertices.Contains(vertex))
                    {
                        continue;
                    }

                    Edge<MapNode> shortestEdge = null;
                    float shortestDist = float.PositiveInfinity;

                    foreach (Edge<MapNode> edge in gabrielGraph)
                    {
                        if (emst.Contains(edge))
                        {
                            continue;
                        }

                        if ((edge.Point1.Equals(vertex) && emstVertices.Contains(edge.Point2)) || (edge.Point2.Equals(vertex) && emstVertices.Contains(edge.Point1)))
                        {
                            float distSquared = edge.DistanceSquared;
                            if (distSquared < shortestDist)
                            {
                                shortestDist = distSquared;
                                shortestEdge = edge;
                            }
                        }
                    }

                    if (float.IsInfinity(shortestDist))
                    {
                        continue;
                    }

                    emst.Add(shortestEdge);
                    emstVertices.Add(vertex);
                }
            }
            return new HashSet<Edge<MapNode>>(emst);
        }

        private Triangle<MapNode> GetSupraTriangle(IEnumerable<Vertex<MapNode>> vertices)
        {
            float xMax = float.MinValue;
            float xMin = float.MaxValue;
            float yMax = float.MinValue;
            float yMin = float.MaxValue;

            foreach (Vertex<MapNode> vertex in vertices)
            {
                if (vertex.Position.x > xMax)
                {
                    xMax = vertex.Position.x;
                }

                if (vertex.Position.x < xMin)
                {
                    xMin = vertex.Position.x;
                }

                if (vertex.Position.y > yMax)
                {
                    yMax = vertex.Position.y;
                }

                if (vertex.Position.y < yMin)
                {
                    yMin = vertex.Position.y;
                }
            }

            float dx = xMax - xMin;
            float dy = yMax - yMin;
            float dMax = Math.Max(dx, dy);
            float midX = (xMin + xMax) / 2.0f;
            float midY = (yMin + yMax) / 2.0f;

            Vertex<MapNode> v1 = new Vertex<MapNode>(midX - 20 * dMax, midY - dMax);
            Vertex<MapNode> v2 = new Vertex<MapNode>(midX, midY + 20 * dMax);
            Vertex<MapNode> v3 = new Vertex<MapNode>(midX + 20 * dMax, midY - dMax);

            return new Triangle<MapNode>(v1, v2, v3);
        }

        private List<Edge<MapNode>> FindHoleBoundaries(ISet<Triangle<MapNode>> badTriangles)
        {
            List<Edge<MapNode>> edges = new List<Edge<MapNode>>();
            foreach (Triangle<MapNode> triangle in badTriangles)
            {
                edges.Add(new Edge<MapNode>(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge<MapNode>(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge<MapNode>(triangle.Vertices[2], triangle.Vertices[0]));
            }
            IEnumerable<IGrouping<Edge<MapNode>, Edge<MapNode>>> grouped = edges.GroupBy(x => x);
            IEnumerable<Edge<MapNode>> boundaryEdges = edges.GroupBy(x => x).Where(x => x.Count() == 1).Select(x => x.First());
            return boundaryEdges.ToList();
        }

        private ISet<Triangle<MapNode>> FindBadTriangles(Vertex<MapNode> point, HashSet<Triangle<MapNode>> triangles)
        {
            IEnumerable<Triangle<MapNode>> badTriangles = triangles.Where(x => x.IsPointInsideCircumcircle(point));
            return new HashSet<Triangle<MapNode>>(badTriangles);
        }
    }
}
