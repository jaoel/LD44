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
} 
