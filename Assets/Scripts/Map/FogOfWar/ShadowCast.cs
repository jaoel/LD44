using UnityEngine;

public static class ShadowCast
{
    public static void CalculateVisibility(Vector3 position, BoundsInt bounds, TileType[,] tiles, float[,] visibility)
    {
        Vector3Int boundsPosition = new Vector3Int(bounds.size.x / 2, bounds.size.y / 2, 0);
        Debug.Log(boundsPosition);

        if (boundsPosition.x >= 0 && boundsPosition.x < visibility.GetLength(0) && boundsPosition.y >= 0 && boundsPosition.y < visibility.GetLength(1))
        {
            visibility[boundsPosition.x, boundsPosition.y] = 1f;
        }

        //for (int y = 0; y < visibility.GetLength(1); y++)
        //{
        //    if(y < 0 || y >= tiles.GetLength(1))
        //    {
        //        continue;
        //    }

        //    for (int x = 0; x < visibility.GetLength(0); x++)
        //    {
        //        if (x < 0 || x >= tiles.GetLength(0))
        //        {
        //            continue;
        //        }

        //        visibility[x, y] = (y * visibility.GetLength(0) + x) % 2 == 0 ? 1f : 0f;
        //    }
        //}
    }
}
