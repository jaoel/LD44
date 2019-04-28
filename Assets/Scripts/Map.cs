using System;
using System.Collections.Generic;
using System.Linq;                 
using UnityEngine;
using UnityEngine.Tilemaps;


public class Map
{
    public int[,] CollisionMap
    {
        get
        {
            return _collisionMap;
        }
    }

    Tilemap _walls;
    Tilemap _floor;
    int _width;
    int _height;

    BSPTree _bspTree;
    List<BSPTree> _leafNodes;
    int[,] _collisionMap;

    public Map(BSPTree bspTree, Tilemap walls, Tilemap floor, int width, int height, int[,] collisionMap)
    {
        _bspTree = bspTree;
        _walls = walls;
        _floor = floor;
        _width = width;
        _height = height;

        _collisionMap = collisionMap;
        NavigationManager.collisionMap = _collisionMap;

        _leafNodes = new List<BSPTree>();
        _bspTree.GetAllLeafNodes(ref _leafNodes);
    }

    public void MovePlayerToSpawn(Player player)
    {
        Vector3Int playerSpawnPos = GetOpenPositionInRoom(2, 2);
        player.transform.position = playerSpawnPos + new Vector3(0.5f, 0.5f, -1.0f);
    }

    public Vector3Int GetOpenPositionInRoom(int widthInTiles, int heightInTiles)
    {
        List<BSPTree> eligibleRooms = _leafNodes.Where(node => node.Room.width > widthInTiles 
            && node.Room.height > heightInTiles).ToList();

        if (eligibleRooms.Count == 0)
            return Vector3Int.zero;

        BSPTree room = eligibleRooms[UnityEngine.Random.Range(0, eligibleRooms.Count)];

        int halfWidth = (int)Math.Ceiling(widthInTiles / 2.0f) + 1;
        int halfHeight = (int)Math.Ceiling(heightInTiles / 2.0f + 1);

        int x = UnityEngine.Random.Range(room.Room.x + halfWidth, room.Room.xMax - halfWidth);
        int y = UnityEngine.Random.Range(room.Room.y + halfHeight, room.Room.yMax - halfHeight);

        return new Vector3Int(x, y, -1);
    } 

    public void DrawPath(List<Vector2Int> path)
    {
        int index = 0;
        path.ForEach(pos =>
        {
            Vector3Int position = new Vector3Int(pos.x, pos.y, 0);
            _floor.SetTileFlags(position, TileFlags.None);

            Color color = new Color(100 / 255.0f, 149 / 255.0f, 237 / 255.0f);
            if (index == 0)
                color = Color.red;

            _floor.SetColor(position, color);
            index++;
        });
    }
}
