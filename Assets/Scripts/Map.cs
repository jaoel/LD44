using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Map
    {
        BSPTree _bspTree;
        public Map(BSPTree bspTree)
        {
            _bspTree = bspTree;
        }

        public void MovePlayerToSpawn(Player player)
        {

        }

        void PopulateMap()
        {
            //Stairs to next level
            Vector3Int stairsPosition = GetOpenPositionInMap();
            GameObject.Instantiate(_interactiveObjectsContainer.Stairs,
                new Vector3(stairsPosition.x - 0.5f, stairsPosition.y - 0.5f, -1.0f), Quaternion.identity);
        }

        public Vector3Int GetOpenPositionInMap()
        {
            int x = Random.Range(0, _width);
            int y = Random.Range(0, _height);
            while (true)
            {
                x = Random.Range(0, _width);
                y = Random.Range(0, _height);

                Tile floor = _floor.GetTile<Tile>(new Vector3Int(x, y, 0));
                Tile wall = _walls.GetTile<Tile>(new Vector3Int(x, y, 0));

                if (floor != null && wall == null)
                {
                    break;
                }
            }

            return new Vector3Int(x, y, 0);
        }
    }
}
