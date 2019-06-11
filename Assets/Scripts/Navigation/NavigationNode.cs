using System.Collections.Generic;
using UnityEngine;

public class NavNodeComparar : IComparer<NavNode>
{
    int IComparer<NavNode>.Compare(NavNode x, NavNode y)
    {
        return x.FScore.CompareTo(y.FScore);
    }
}

public class NavNode
{
    public int FScore
    {
        get { return GScore + HScore; }
    }

    public int GScore { get; set; }
    public int HScore { get; set; }

    public NavNode Parent { get; set; }
    public Vector2Int Data { get; set; }

    public NavNode(int hScore, int gScore, Vector2Int data)
    {
        HScore = hScore;
        GScore = gScore;
        Data = data;
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

        NavNode other = (NavNode)obj;

        return Data.Equals(other.Data);
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    public static bool operator ==(NavNode a, NavNode b)
    {
        if (object.ReferenceEquals(a, null))
        {
            return object.ReferenceEquals(b, null);
        }

        return a.Equals(b);
    }

    public static bool operator !=(NavNode a, NavNode b)
    {
        if (object.ReferenceEquals(a, null))
        {
            return !object.ReferenceEquals(b, null);
        }

        return !a.Equals(b);
    }
}

public class NavigationNode<T>
{
    public int index;
    public int FScore
    {
        get { return GScore + HScore; }
    }

    public int GScore { get; set; }
    public int HScore { get; set; }

    public NavigationNode<T> Parent { get; set; }
    public T Data { get; set; }

    public NavigationNode(int hScore, int gScore, T data)
    {
        HScore = hScore;
        GScore = gScore;
        Data =  data;
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

        NavigationNode<T> other = (NavigationNode<T>)obj;

        return Data.Equals(other.Data);
    }

    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    public static bool operator == (NavigationNode<T> a, NavigationNode<T> b)
    {
        if (object.ReferenceEquals(a, null))
        {
            return object.ReferenceEquals(b, null);
        }    

        return a.Equals(b);
    }

    public static bool operator != (NavigationNode<T> a, NavigationNode<T> b)
    {
        if (object.ReferenceEquals(a, null))
        {
            return !object.ReferenceEquals(b, null);
        }

        return !a.Equals(b);
    }
} 
