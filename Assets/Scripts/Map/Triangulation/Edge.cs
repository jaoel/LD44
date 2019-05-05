namespace Delaunay
{
    public class Edge<T>
    {
        private Vertex<T> _point1;
        private Vertex<T> _point2;

        public Vertex<T> Point1 => _point1;
        public Vertex<T> Point2 => _point2;

        public Edge(Vertex<T> point1, Vertex<T> point2)
        {
            _point1 = point1;
            _point2 = point2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;

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