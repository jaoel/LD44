using UnityEngine;

public static class RectExtension
{
    public static bool Overlaps(this Rect a, Rect b)
    {
        if (a.x < b.x + b.width && a.x + a.width > b.x && a.y < b.y + b.height && a.y + a.height > b.y)
        {
            return true;
        }
        return false;
    }

    public static bool Intersects(this Rect a, Rect b, out Rect area)
    {
        area = new Rect();

        if (b.Overlaps(a))
        {
            float x1 = Mathf.Min(a.xMax, b.xMax);
            float x2 = Mathf.Max(a.xMin, b.xMin);
            float y1 = Mathf.Min(a.yMax, b.yMax);
            float y2 = Mathf.Max(a.yMin, b.yMin);
            area.x = Mathf.Min(x1, x2);
            area.y = Mathf.Min(y1, y2);
            area.width = Mathf.Max(0, x1 - x2);
            area.height = Mathf.Max(0, y1 - y2);

            return true;
        }

        return false;
    }

    public static bool Contains(this Rect a, Vector2 point)
    {
        if (a.xMin <= point.x && a.xMax >= point.x && a.yMin <= point.y && a.yMax >= point.y)
        {
            return true;
        }
        return false;
    }

    public static float Area(this Rect rect)
    {
        return rect.width * rect.height;
    }

    /// <summary>
    /// Expands a rect with the fiven amount in all directions
    /// </summary>
    public static Rect Expand(this Rect rect, float amount)
    {
        Rect newRect = rect;
        newRect.x = rect.x - amount;
        newRect.y = rect.y - amount;
        newRect.width = rect.width + amount * 2f;
        newRect.height = rect.height + amount * 2f;
        return newRect;
    }
}
