using System.Collections.Generic;
using UnityEngine;

public class NavigationNode
{
    public int FScore
    {
        get { return GScore + HScore; }
    }

    public int GScore { get; set; }
    public int HScore { get; set; }

    public NavigationNode Parent { get; set; }
    public Vector2Int Position { get; set; }

    public NavigationNode(int x, int y)
    {                       
        Position = new Vector2Int(x, y);
    }

    public NavigationNode(int hScore, int gScore, int x, int y)
    {
        HScore = hScore;
        GScore = gScore;
        Position = new Vector2Int(x, y);
    }

    public NavigationNode(int hScore, int gScore, Vector2Int pos)
    {
        HScore = hScore;
        GScore = gScore;
        Position =  pos;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (this.GetType() != obj.GetType())
        {
            return false;
        }

        return Position == ((NavigationNode)obj).Position;
    }

    public override int GetHashCode()
    {
        var hashCode = -372316642;
        hashCode = hashCode * -1521134295 + GScore.GetHashCode();
        hashCode = hashCode * -1521134295 + HScore.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<NavigationNode>.Default.GetHashCode(Parent);
        hashCode = hashCode * -1521134295 + EqualityComparer<Vector2Int>.Default.GetHashCode(Position);
        return hashCode;
    }

    public static bool operator == (NavigationNode a, NavigationNode b)
    {
        if (object.ReferenceEquals(a, null))
        {
            return object.ReferenceEquals(b, null);
        }    

        return a.Equals(b);
    }

    public static bool operator != (NavigationNode a, NavigationNode b)
    {
        if (object.ReferenceEquals(a, null))
        {
            return !object.ReferenceEquals(b, null);
        }

        return !a.Equals(b);
    }
} 
