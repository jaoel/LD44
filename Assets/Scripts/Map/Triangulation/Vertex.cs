using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class Vertex<T>
    {
        public Vector2 Position { get; set; }
        public HashSet<Triangle<T>> AdjacentTriangles { get; }
        public T Data { get; set; }

        private Vertex()
        {
            AdjacentTriangles = new HashSet<Triangle<T>>();
        }

        public Vertex(Vector2 position)
            : this()
        {
            Position = position;
        }

        public Vertex(float x, float y)
           : this()
        {
            Position = new Vector2(x, y);
        }

        public Vertex(Vector2 position, T data)
            : this()
        {
            Position = position;
            Data = data;
        }

        public Vertex(float x, float y, T data)
          : this()
        {
            Position = new Vector2(x, y);
            Data = data;
        }
    }
}