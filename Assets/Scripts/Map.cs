using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public class Map
    {
        Tilemap _walls;
        Tilemap _floor;
        int _width;
        int _height;

        BSPTree _bspTree;
        public Map(BSPTree bspTree, Tilemap walls, Tilemap floor, int width, int height)
        {
            _bspTree = bspTree;
            _walls = walls;
            _floor = floor;
            _width = width;
            _height = height;
        }

        public void MovePlayerToSpawn(Player player)
        {
            Vector3Int playerSpawnPos = GetOpenPositionInMap();
            player.transform.position = playerSpawnPos + new Vector3(0.5f, 0.5f, -1.0f);
        }

        public Vector3Int GetOpenPositionInMap()
        {
            int x = UnityEngine.Random.Range(0, _width);
            int y = UnityEngine.Random.Range(0, _height);
            while (true)
            {
                x = UnityEngine.Random.Range(0, _width);
                y = UnityEngine.Random.Range(0, _height);

                Tile floor = _floor.GetTile<Tile>(new Vector3Int(x, y, 0));
                Tile wall = _walls.GetTile<Tile>(new Vector3Int(x, y, -1));

                if (floor != null && wall == null)
                {
                    break;
                }
            }

            return new Vector3Int(x, y, 0);
        }
    }
}
