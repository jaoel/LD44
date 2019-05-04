using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Edge
{
    private Vertex _origin;
    private Vertex _target;

    public Vertex Origin => _origin;
    public Vertex Target => _target;

    public Edge(MapNode origin, MapNode target)
    {
        _origin = new Vertex(origin);
        _target = new Vertex(target);
    }

    public Edge(Vertex origin, Vertex target)
    {
        _origin = origin;
        _target = target;
    } 
    
    public bool ContainsVertex(Vertex vertex)
    {
        return _origin.Node.Id == vertex.Node.Id || _target.Node.Id == vertex.Node.Id;
    }  
}  
