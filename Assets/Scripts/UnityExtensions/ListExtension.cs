using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
    public static List<T> Shuffle<T>(this List<T> self)
    {
        List<T> shuffled = new List<T>(self);
        int n = shuffled.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = shuffled[k];
            shuffled[k] = shuffled[n];
            shuffled[n] = value;
        }
        return shuffled;
    }
}
