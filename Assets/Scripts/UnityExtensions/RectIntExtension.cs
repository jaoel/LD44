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
    public static int Area(this RectInt rect)
    {
        return rect.width * rect.height;
    }
}
