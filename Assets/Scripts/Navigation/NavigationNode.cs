using System.Collections.Generic;
using UnityEngine;

public class NavigationNode<T>
{
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
