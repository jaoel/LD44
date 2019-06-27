using UnityEngine;

public static class RectIntExtension
{
    public static BoundsInt ToBoundsInt(this RectInt a, int z = 0, int depth = 1)
    {
        return new BoundsInt(a.xMin, a.yMin, z, a.width, a.height, depth);
    }

    public static bool Overlaps(this RectInt a, RectInt b)
    {
        if (a.yMax <= b.y || a.y >= b.yMax)
        {
            return false;
        }

        if (a.xMax <= b.x || a.x >= b.xMax)
        {
            return false;
        }

        return true;
    }

    public static bool Intersects(this RectInt a, RectInt b, out RectInt area)
    {
        area = new RectInt();

        if (b.Overlaps(a))
        {
            int x1 = Mathf.Min(a.xMax, b.xMax);
            int x2 = Mathf.Max(a.xMin, b.xMin);
            int y1 = Mathf.Min(a.yMax, b.yMax);
            int y2 = Mathf.Max(a.yMin, b.yMin);
            area.x = Mathf.Min(x1, x2);
            area.y = Mathf.Min(y1, y2);
            area.width = Mathf.Max(0, x1 - x2);
            area.height = Mathf.Max(0, y1 - y2);

            return true;
        }

        return false;
    }

    public static bool Contains(this RectInt a, Vector2 point)
    {
        if (a.xMin <= point.x && a.xMax >= point.x && a.yMin <= point.y && a.yMax >= point.y)
        {
            return true;
        }
        return false;
    }

    public static bool Contains(this RectInt a, RectInt b)
    {
        if (a.xMin <= b.xMin && a.xMax >= b.xMax && a.yMin <= b.yMin && a.yMax >= b.yMax)
        {
            return true;
        }

        return false;
    }

    public static bool Contains(this RectInt a, BoundsInt b)
    {
        return Contains(a, b.ToRectInt());
    }

    public static int Area(this RectInt rect)
    {
        return rect.width * rect.height;
    }

    /// <summary>
    /// Expands a rect with the fiven amount in all directions
    /// </summary>
    public static RectInt Expand(this RectInt rect, int amount)
    {
        RectInt newRect = rect;
        newRect.x = rect.x - amount;
        newRect.y = rect.y - amount;
        newRect.width = rect.width + amount * 2;
        newRect.height = rect.height + amount * 2;
        return newRect;
    }
}
