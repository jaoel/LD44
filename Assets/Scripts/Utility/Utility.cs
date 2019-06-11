using UnityEngine;

public static class Utility
{
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    public static float ConvertRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
    {
        double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
        return (float)(newStart + ((value - originalStart) * scale));
    }

    public static Color RGBAColor(int r, int g, int b, float a)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a);
    }

    public static Vector2 RandomPointInCircle(float radius)
    {
        float r = radius * Mathf.Sqrt(Random.Range(0.0f, 1.0f));
        float theta = Random.Range(0.0f, 1.0f) * 2 * Mathf.PI;

        float x = Mathf.Round(r * Mathf.Cos(theta));
        float y = Mathf.Round(r * Mathf.Sin(theta));

        return new Vector2(x, y);
    }

    public static Vector2 RandomPointOnCircleEdge(float radius)
    {
        return Random.insideUnitCircle.normalized * radius;
    }

    public static int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
} 
