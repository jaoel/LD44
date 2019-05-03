using UnityEngine;

public static class RectIntExtension
{
    public static bool Overlaps(this RectInt a, RectInt b)
    {
        if (a.x < b.x + b.width && a.x + a.width > b.x && a.y < b.y + b.height && a.y + a.height > b.y)
        {
            return true;
        }
        return false;
    }

    public static int Area(this RectInt rect)
    {
        return rect.width * rect.height;
    }
}  
