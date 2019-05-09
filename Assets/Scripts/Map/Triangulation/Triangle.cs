using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Delaunay
{
    public class Triangle<T>
    {
        private Vector2 _circumCenter;
        private float _radiusSquared;

        public Vertex<T>[] Vertices { get; }
        public Edge<T>[] Edges { get; }

        public IEnumerable<Triangle<T>> TrianglesWithSharedEdge
        {
            get
            {
                HashSet<Triangle<T>> neighbors = new HashSet<Triangle<T>>();
                foreach (Vertex<T> vertex in Vertices)
                {
                    IEnumerable<Triangle<T>> trianglesWithSharedEdge = vertex.AdjacentTriangles.Where(x =>
                    {
                        return x != this && SharesEdgeWith(x);
                    });
                    neighbors.UnionWith(trianglesWithSharedEdge);
                }
                return neighbors;
            }
        }

        public Triangle(Vertex<T> a, Vertex<T> b, Vertex<T> c)
        {
            Vertices = new Vertex<T>[3];
            Edges = new Edge<T>[3];

            if (!IsCounterClockwise(a.Position, b.Position, c.Position))
            {
                Vertices[0] = a;
                Vertices[1] = b;
                Vertices[2] = c;
            }
            else
            {
                Vertices[0] = a;
                Vertices[1] = b;
                Vertices[2] = c;
            }

            Edges[0] = new Edge<T>(Vertices[0], Vertices[1]);
            Edges[1] = new Edge<T>(Vertices[1], Vertices[2]);
            Edges[2] = new Edge<T>(Vertices[2], Vertices[0]);

            Vertices[0].AdjacentTriangles.Add(this);
            Vertices[1].AdjacentTriangles.Add(this);
            Vertices[2].AdjacentTriangles.Add(this);
            UpdateCircumcircle();
        }

        private void UpdateCircumcircle()
        {
            Vertex<T> p0 = Vertices[0];
            Vertex<T> p1 = Vertices[1];
            Vertex<T> p2 = Vertices[2];
            float dA = p0.Position.x * p0.Position.x + p0.Position.y * p0.Position.y;
            float dB = p1.Position.x * p1.Position.x + p1.Position.y * p1.Position.y;
            float dC = p2.Position.x * p2.Position.x + p2.Position.y * p2.Position.y;

            float aux1 = (dA * (p2.Position.y - p1.Position.y) + dB * (p0.Position.y - p2.Position.y) + dC * (p1.Position.y - p0.Position.y));
            float aux2 = -(dA * (p2.Position.x - p1.Position.x) + dB * (p0.Position.x - p2.Position.x) + dC * (p1.Position.x - p0.Position.x));
            float div = (2 * (p0.Position.x * (p2.Position.y - p1.Position.y) + p1.Position.x * (p0.Position.y - p2.Position.y) + p2.Position.x * (p1.Position.y - p0.Position.y)));

            if (div == 0)
            {
                throw new System.Exception();
            }

            Vector2 center = new Vector2(aux1 / div, aux2 / div);
            _circumCenter = center;
            _radiusSquared = (_circumCenter - p0.Position).sqrMagnitude;
        }

        private bool IsCounterClockwise(Vector2 a, Vector2 b, Vector2 c)
        {
            float result = (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
            return result > 0;
        }

        public bool SharesEdgeWith(Triangle<T> triangle)
        {
            int sharedVertexCount = Vertices.Where(x => triangle.Vertices.Contains(x)).Count();
            return sharedVertexCount == 2;
        }

        public bool IsPointInsideCircumcircle(Vertex<T> vertex)
        {
            float distanceSquared = (vertex.Position - _circumCenter).sqrMagnitude;
            return distanceSquared < _radiusSquared;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Triangle<T> otherTriangle = (Triangle<T>)obj;
            Vertex<T>[] otherVertices = otherTriangle.Vertices;
            foreach (Vertex<T> vertex in Vertices)
            {
                if (!otherVertices.Any(x => x.Position == vertex.Position))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return 59827589 + EqualityComparer<Vertex<T>[]>.Default.GetHashCode(Vertices);
        }
    }
}
