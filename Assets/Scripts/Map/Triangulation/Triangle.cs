using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Delaunay
{
    public class Triangle<T>
    {
        private Vertex<T>[] _vertices;
        private Edge<T>[] _edges;

        private Vector2 _circumCenter;
        private float _radiusSquared;

        public Vertex<T>[] Vertices => _vertices;
        public Edge<T>[] Edges => _edges;

        public IEnumerable<Triangle<T>> TrianglesWithSharedEdge
        {
            get
            {
                HashSet<Triangle<T>> neighbors = new HashSet<Triangle<T>>();
                foreach (Vertex<T> vertex in _vertices)
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
            _vertices = new Vertex<T>[3];
            _edges = new Edge<T>[3];

            if (!IsCounterClockwise(a.Position, b.Position, c.Position))
            {
                _vertices[0] = a;
                _vertices[1] = b;
                _vertices[2] = c;
            }
            else
            {
                _vertices[0] = a;
                _vertices[1] = b;
                _vertices[2] = c;
            }

            _edges[0] = new Edge<T>(_vertices[0], _vertices[1]);
            _edges[1] = new Edge<T>(_vertices[1], _vertices[2]);
            _edges[2] = new Edge<T>(_vertices[2], _vertices[0]);

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
            int sharedVertexCount = _vertices.Where(x => triangle.Vertices.Contains(x)).Count();
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
                return false;

            Triangle<T> otherTriangle = (Triangle<T>)obj;
            Vertex<T>[] otherVertices = otherTriangle.Vertices;
            foreach(Vertex<T> vertex in _vertices)
            {
                if (!otherVertices.Any(x => x.Position == vertex.Position))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return 59827589 + EqualityComparer<Vertex<T>[]>.Default.GetHashCode(_vertices);
        }
    }
}
