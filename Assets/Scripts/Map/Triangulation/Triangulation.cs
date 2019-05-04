using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Triangulation
{
    private List<Vertex> _vertices;
    private List<Edge> _edges;

    public List<Vertex> Vertices => _vertices;
    public List<Edge> Edges => _edges;

    public Triangulation()
    {
        _vertices = new List<Vertex>();
        _edges = new List<Edge>();
    }

    public void AddVertex(Vertex node)
    {
        _vertices.Add(node);
    }

    public void AddVertices(List<MapNode> nodes)
    {
        nodes.ForEach(x =>
        {
            AddVertex(new Vertex(x));
        });
    }

    public void AddVertices(List<Vertex> nodes)
    {
        _vertices.AddRange(nodes);
    }

    public void AddEdge(Edge edge)
    {
        _edges.Add(edge);
    }

    public void AddEdges(List<Edge> edges)
    {
        _edges.AddRange(edges);
    }

    public Vertex GetLowestVertex()
    {
        return _vertices.Aggregate((min, next) => min.Position.y < next.Position.y ? min : next);
    }
}  
