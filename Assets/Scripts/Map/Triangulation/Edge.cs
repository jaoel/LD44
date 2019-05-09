using UnityEngine;

namespace Delaunay
{
    public class Edge<T>
    {
        private Vertex<T> _point1;
        private Vertex<T> _point2;

        private Vector2 _edgeVector;
        private float _distanceSquared;
        private Vector2 _circumCircleCenter;
        private float _radius;


        public Vertex<T> Point1 => _point1;
        public Vertex<T> Point2 => _point2;
        public float DistanceSquared => _distanceSquared;

        public Edge(Vertex<T> point1, Vertex<T> point2)
        {
            _point1 = point1;
            _point2 = point2;

            _edgeVector = _point2.Position - point1.Position;
            _distanceSquared = _edgeVector.sqrMagnitude;

            _circumCircleCenter = (_point1.Position + _point2.Position) / 2.0f;
            _radius = _edgeVector.magnitude / 2.0f;
        }

        public bool CircumCircleContainsPoint(Vertex<T> vertex)
        {
            if (vertex.Position == _point1.Position || vertex.Position == _point2.Position)
            {
                return false;
            }

            if ((vertex.Position - _circumCircleCenter).magnitude < _radius)
            {
                return true;
            }

            return false;
        }

        public bool ContainsVertex(Vertex<T> vertex)
        {
            if (_point1.Position == vertex.Position || _point2.Position == vertex.Position)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            Edge<T> edge = obj as Edge<T>;

            bool samePoints = Point1.Position == edge.Point1.Position && Point2.Position == edge.Point2.Position;
            bool samePointsReversed = Point1.Position == edge.Point2.Position && Point2.Position == edge.Point1.Position;
            return samePoints || samePointsReversed;
        }

        public override int GetHashCode()
        {
            int hCode = (int)_point1.Position.x ^ (int)_point1.Position.y ^ (int)_point2.Position.x ^ (int)_point2.Position.y;
            return hCode.GetHashCode();
        }
    }
}    