using UnityEngine;

public static class VectorExtension
{
    public static Vector3 ToVector3(this Vector2Int a, float z = 0.0f)
    {
        return new Vector3(a.x, a.y, z);
    }

    public static Vector3 ToVector3(this Vector2 a, float z = 0.0f)
    {
        return new Vector3(a.x, a.y, z);
    }

    public static Vector3Int ToVector3Int(this Vector2Int a, int z = 0)
    {
        return new Vector3Int(a.x, a.y, z);
    }

    public static Vector3Int ToVector3Int(this Vector3 a)
    {
        return new Vector3Int(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(a.z));
    }

    public static Vector2 ToVector2(this Vector2Int a)
    {
        return new Vector2(a.x, a.y);
    }

    public static Vector2 ToVector2(this Vector3 a)
    {
        return new Vector2(a.x, a.y);
    }

    public static Vector2Int ToVector2Int(this Vector2 a)
    {
        return new Vector2Int(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y));
    }

}

