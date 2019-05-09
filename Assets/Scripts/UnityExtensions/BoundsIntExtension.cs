using UnityEngine;

public static class BoundsIntExtension
{
    public static bool Overlaps(this BoundsInt a, BoundsInt b)
    {
        if (a.x < b.x + b.size.x && a.x + a.size.x > b.x && a.y < b.y + b.size.y && a.y + a.size.y > b.y)
        {
            return true;
        }
        return false;
    }

    public static bool Overlaps(this BoundsInt a, RectInt b)
    {
        if (a.x < b.x + b.size.x && a.x + a.size.x > b.x && a.y < b.y + b.size.y && a.y + a.size.y > b.y)
        {
            return true;
        }
        return false;
    }

    public static bool Contains(this BoundsInt a, BoundsInt point)
    {
        if (a.xMin <= point.x && a.xMax >= point.x && a.yMin <= point.y && a.yMax >= point.y)
            return true;

        return false;
    }
    public static int Area(this BoundsInt rect)
    {
        return rect.size.x * rect.size.y;
    }

    public static RectInt ToRectInt(this BoundsInt a)
    {
        return new RectInt(a.xMin, a.yMin, a.size.x, a.size.y);
    }
}
