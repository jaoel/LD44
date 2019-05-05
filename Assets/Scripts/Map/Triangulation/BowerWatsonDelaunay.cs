using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class BowerWatsonDelaunay<T>
    {                                               
        public IEnumerable<Triangle<T>> Triangulate(IEnumerable<Vertex<T>> vertices)
        {
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
            return triangulation;
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
