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

    public static bool Overlaps(this BoundsInt a, Bounds b)
    {
        if (a.x < b.min.x + b.size.x && a.x + a.size.x > b.min.x && a.y < b.min.y + b.size.y && a.y + a.size.y > b.min.y)
        {
            return true;
        }
        return false;
    }

    public static bool Contains(this BoundsInt a, RectInt b)
    {
        if (a.xMin <= b.xMin && a.xMax >= b.xMax && a.yMin <= b.yMin && a.yMax >= b.yMax)
        {
            return true;
        }

        return false;
    }

    public static bool Intersects(this BoundsInt a, RectInt b, out RectInt area)
    {
        area = new RectInt();
        BoundsInt bb = new BoundsInt(b.xMin, b.yMin, 0, b.size.x, b.size.y, 0);
        if (bb.Overlaps(a))
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
