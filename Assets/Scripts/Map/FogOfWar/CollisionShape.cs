using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CollisionShape
{
    public class Vertex
    {
        public Vector2 position;
        public Vector2 normal;
        public int[] neighbours;
        public bool corner = false;

        public bool PositionEquals(Vector2 other)
        {
            return Vector2.Distance(position, other) < 0.001f;
        }
    }

    public class Edge
    {
        public int startVertex;
        public int endVertex;

        public bool Equals(Edge other)
        {
            return startVertex == other.startVertex && endVertex == other.endVertex ||
                startVertex == other.endVertex && endVertex == other.startVertex;
        }
    }

    private List<Vertex> _verticesList = new List<Vertex>();
    private List<Edge> _edgesList = new List<Edge>();

    public Vertex[] Vertices { get; private set; } = new Vertex[0];
    public Edge[] Edges { get; private set; } = new Edge[0];


    private int AddPoint(Vector2 point)
    {
        int index = _verticesList.FindIndex(v => v.PositionEquals(point));
        if(index < 0)
        {
            _verticesList.Add(new Vertex() { position = point });
            index = _verticesList.Count - 1;
        }
        return index;
    }

    public void AddEdge(Vector2 start, Vector2 end)
    {
        int i0 = AddPoint(start);
        int i1 = AddPoint(end);

        Vertex v0 = _verticesList[i0];
        Vertex v1 = _verticesList[i1];

        List<int> n0 = new List<int>(v0.neighbours ?? new int[0]);
        List<int> n1 = new List<int>(v1.neighbours ?? new int[0]);

        n0.Add(i1);
        n1.Add(i0);

        v0.neighbours = n0.ToArray();
        v1.neighbours = n1.ToArray();

        _edgesList.Add(new Edge() { startVertex = i0, endVertex = i1 });
    }

    private bool IsParallel(Edge e0, Edge e1)
    {
        Vector2 v0 = _verticesList[e0.endVertex].position - _verticesList[e0.startVertex].position;
        Vector2 v1 = _verticesList[e1.endVertex].position - _verticesList[e1.startVertex].position;

        if (Mathf.Abs(Vector2.Dot(v0, v1)) > 0.9)
        {
            return true;
        }
        return false;
    }

    public void Bake()
    {
        // Find edge vertices
        Dictionary<int, int> cornerVerticeIndices = new Dictionary<int, int>();
        List<Vertex> cornerVertices = new List<Vertex>();
        for(int vertIndex = 0; vertIndex < _verticesList.Count; vertIndex++)
        {
            Vertex vert = _verticesList[vertIndex];
            vert.corner = false;
            List<Edge> connectedEdges = vert.neighbours.Select(neighbour => new Edge() { startVertex = vertIndex, endVertex = neighbour }).ToList();

            if (connectedEdges.Count == 2)
            {
                if (!IsParallel(connectedEdges[0], connectedEdges[1]))
                {
                    Vector2 v0 = _verticesList[connectedEdges[0].endVertex].position - _verticesList[connectedEdges[0].startVertex].position;
                    Vector2 v1 = _verticesList[connectedEdges[1].endVertex].position - _verticesList[connectedEdges[1].startVertex].position;
                    vert.corner = true;
                    vert.normal = (-v1 - v0).normalized;
                    cornerVertices.Add(vert);
                    cornerVerticeIndices[vertIndex] = cornerVertices.Count - 1;
                }
            }

            // Special case where the edge has more than 2 connected edges
            else if(connectedEdges.Count > 2)
            {
                vert.corner = true;
                cornerVertices.Add(vert);
                cornerVerticeIndices[vertIndex] = cornerVertices.Count - 1;
            }
        }

        // Find edges between corner verts
        List<Edge> cornerEdges = new List<Edge>();
        for (int vertIndex = 0; vertIndex < _verticesList.Count; vertIndex++)
        {
            Vertex vert = _verticesList[vertIndex];
            if (vert.corner)
            {
                foreach(int neighbourIndex in vert.neighbours) {
                    int previous = vertIndex;
                    int current = neighbourIndex;
                    while (_verticesList[current].corner == false)
                    {
                        foreach(int currentNeighbourIndex in _verticesList[current].neighbours) {
                            if (currentNeighbourIndex != previous)
                            {
                                previous = current;
                                current = currentNeighbourIndex;
                            }
                        }
                    }
                    Edge newEdge = new Edge() { startVertex = vertIndex, endVertex = current };
                    if (vertIndex != current && !cornerEdges.Exists(edge => edge.Equals(newEdge)))
                    {
                        cornerEdges.Add(newEdge);
                    }
                }
            }
        }

        // Remap edge indices to new corner verts list
        foreach (Edge edge in cornerEdges)
        {
            edge.startVertex = cornerVerticeIndices[edge.startVertex];
            edge.endVertex = cornerVerticeIndices[edge.endVertex];
        }


        // Remap vert neighbours indices to new corner verts list
        foreach (Vertex vertex in cornerVertices)
        {
            vertex.neighbours = new int[0];
        }
        foreach (Edge edge in cornerEdges)
        {
            Vertex v0 = cornerVertices[edge.startVertex];
            Vertex v1 = cornerVertices[edge.endVertex];

            List<int> n0 = new List<int>(v0.neighbours ?? new int[0]);
            List<int> n1 = new List<int>(v1.neighbours ?? new int[0]);

            n0.Add(edge.endVertex);
            n1.Add(edge.startVertex);

            v0.neighbours = n0.ToArray();
            v1.neighbours = n1.ToArray();
        }

        Vertices = cornerVertices.ToArray();
        Edges = cornerEdges.ToArray();

        _verticesList.Clear();
        _edgesList.Clear();
    }

    public void DebugDraw()
    {
        foreach (Vertex vertex in Vertices)
        {
            Vector2 pos = vertex.position;
            Debug.DrawLine(new Vector3(pos.x - 0.1f, pos.y - 0.1f, 0f), new Vector3(pos.x + 0.1f, pos.y + 0.1f, 0f), Color.red);
            Debug.DrawLine(new Vector3(pos.x + 0.1f, pos.y - 0.1f, 0f), new Vector3(pos.x - 0.1f, pos.y + 0.1f, 0f), Color.red);
            Debug.DrawLine(pos, pos + vertex.normal, Color.blue);
        }

        foreach (Edge edge in Edges)
        {
            Vector2 p0 = Vertices[edge.startVertex].position;
            Vector2 p1 = Vertices[edge.endVertex].position;
            Debug.DrawLine(p0, p1, Color.green);
        }
    }
}
