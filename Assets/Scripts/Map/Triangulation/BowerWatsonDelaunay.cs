using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class BowerWatsonDelaunay<T>
    {
        private Timer _timer;

        public BowerWatsonDelaunay()
        {
            _timer = new Timer();
        }

        public IEnumerable<Triangle<T>> Triangulate(IEnumerable<Vertex<T>> vertices)
        {
            _timer.Start();

            Triangle<T> supraTriangle = GetSupraTriangle(vertices);
            HashSet<Triangle<T>> triangulation = new HashSet<Triangle<T>>() { supraTriangle };

            foreach (Vertex<T> vertex in vertices)
            {
                ISet<Triangle<T>> badTriangles = FindBadTriangles(vertex, triangulation);
                List<Edge<T>> polygon = FindHoleBoundaries(badTriangles);

                foreach (Triangle<T> triangle in badTriangles)
                {
                    foreach (Vertex<T> badTriangleVertex in triangle.Vertices)
                    {
                        badTriangleVertex.AdjacentTriangles.Remove(triangle);
                    }
                }

                int trianglesRemoved = triangulation.RemoveWhere(o => badTriangles.Contains(o));

                foreach (Edge<T> edge in polygon)
                {
                    var triangle = new Triangle<T>(vertex, edge.Point1, edge.Point2);
                    triangulation.Add(triangle);
                }
            }

            triangulation.RemoveWhere(x => x.Vertices.Any(v => supraTriangle.Vertices.Contains(v)));

            _timer.Stop();
            _timer.Print("BowerWatsonDelaunay.Triangulate");

            return triangulation;
        }

        public HashSet<Edge<T>> GetDelaunayEdges(IEnumerable<Triangle<T>> triangles)
        {
            HashSet<Edge<T>> result = new HashSet<Edge<T>>();
            foreach (Triangle<T> triangle in triangles)
            {
                foreach(Edge<T> edge in triangle.Edges)
                {
                    result.Add(edge);
                }
            }

            return result;
        }

        public HashSet<Edge<T>> GetGabrielGraph(in HashSet<Edge<T>> delaunayEdges, in IEnumerable<Vertex<T>> vertices)
        {
            _timer.Start();

            HashSet<Edge<T>> result = new HashSet<Edge<T>>(delaunayEdges);
            List<Edge<T>> removalList = new List<Edge<T>>();

            foreach (Edge<T> edge in result)
            {   
                foreach (Vertex<T> vertex in vertices)
                {
                    if (edge.CircumCircleContainsPoint(vertex))
                    {
                        removalList.Add(edge);
                        break;
                    }
                }
            }

            result.RemoveWhere(x => removalList.Contains(x));

            _timer.Stop();
            _timer.Print("BowerWatsonDelaunay.GetGabrielGraph");

            return result;
        }

        public HashSet<Edge<T>> GetPrimEMST(in HashSet<Edge<T>> gabrielGraph, in IEnumerable<Vertex<T>> vertices)
        {
            _timer.Start();

            HashSet<Edge<T>> emst = new HashSet<Edge<T>>();
            List<Vertex<T>> queue = new List<Vertex<T>>(vertices);
            List<Vertex<T>> tree = new List<Vertex<T>>();

            tree.Add(queue[0]);
            queue.RemoveAt(0);

            while(queue.Count > 0)
            {                                                  
                List<Edge<T>> candidates = new List<Edge<T>>();
                foreach(Vertex<T> vertex in tree)
                {
                    candidates.AddRange(gabrielGraph.Where(edge => edge.ContainsVertex(vertex) && (!tree.Contains(edge.Point1) || !tree.Contains(edge.Point2))));
                }

                Edge<T> finalCandidate = candidates.Aggregate((min, x) => min.DistanceSquared < x.DistanceSquared ? min : x);
                emst.Add(finalCandidate);

                if (tree.Contains(finalCandidate.Point1))
                    tree.Add(finalCandidate.Point2);
                else
                    tree.Add(finalCandidate.Point1);

                queue.Remove(tree.Last());
            }

            _timer.Stop();
            _timer.Print("BowerWatsonDelaunay.GetPrimEMST");

            return emst;
        }

        private Triangle<T> GetSupraTriangle(IEnumerable<Vertex<T>> vertices)
        {
            float xMax = float.MinValue;
            float xMin = float.MaxValue;
            float yMax = float.MinValue;
            float yMin = float.MaxValue;

            foreach(Vertex<T> vertex in vertices)
            {
                if (vertex.Position.x > xMax)
                    xMax = vertex.Position.x;

                if (vertex.Position.x < xMin)
                    xMin = vertex.Position.x;

                if (vertex.Position.y > yMax)
                    yMax = vertex.Position.y;

                if (vertex.Position.y < yMin)
                    yMin = vertex.Position.y;
            }

            float dx = xMax - xMin;
            float dy = yMax - yMin;
            float dMax = Math.Max(dx, dy);
            float midX = (xMin + xMax) / 2.0f;
            float midY = (yMin + yMax) / 2.0f;

            Vertex<T> v1 = new Vertex<T>(midX - 20 * dMax, midY - dMax);
            Vertex<T> v2 = new Vertex<T>(midX, midY + 20 * dMax);
            Vertex<T> v3 = new Vertex<T>(midX + 20 * dMax, midY - dMax);

            return new Triangle<T>(v1, v2, v3);
        }

        private List<Edge<T>> FindHoleBoundaries(ISet<Triangle<T>> badTriangles)
        {
            List<Edge<T>> edges = new List<Edge<T>>();
            foreach (Triangle<T> triangle in badTriangles)
            {
                edges.Add(new Edge<T>(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge<T>(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge<T>(triangle.Vertices[2], triangle.Vertices[0]));
            }
            IEnumerable<IGrouping<Edge<T>, Edge<T>>> grouped = edges.GroupBy(x => x);
            IEnumerable<Edge<T>> boundaryEdges = edges.GroupBy(x => x).Where(x => x.Count() == 1).Select(x => x.First());
            return boundaryEdges.ToList();
        }

        private ISet<Triangle<T>> FindBadTriangles(Vertex<T> point, HashSet<Triangle<T>> triangles)
        {
            IEnumerable<Triangle<T>> badTriangles = triangles.Where(x => x.IsPointInsideCircumcircle(point));
            return new HashSet<Triangle<T>>(badTriangles);
        }
    }
}
