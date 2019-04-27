using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.Tilemaps;           

namespace Assets.Scripts
{
    public class MapGenerator
    {
        private Tilemap _floor;
        private List<Tile> _tiles;
        private BSPTree _root;

        public MapGenerator(List<Tile> tiles)
        {
            _floor = GameObject.Find("Floor").GetComponent<Tilemap>();
            _tiles = tiles;
            _root = GenerateDungeon(100, 100);

            UpdateTilemap(_root);   
        }

        public void DrawDebug()
        {
            BSPTree.DebugDrawBspNode(_root);
        }

        public BSPTree GenerateDungeon(int width, int height)
        {
            BSPTree root = Split(5, new RectInt(0, 0, width, height));
            GenerateRooms(root);
            GenerateCorridors(root);
            return root;
        }

        void GenerateFloor(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    _floor.SetTile(new Vector3Int(x, y, 0), _tiles[0]);
                }
            }
        }

        public BSPTree Split(int level, RectInt grid)
        {
            BSPTree node = new BSPTree(grid);

            if (level > 0)
            {
                RectInt[] newContainers = SplitContainer(grid);

                node.Left = Split(level - 1, newContainers[0]);
                node.Right = Split(level - 1, newContainers[1]);
            }

            return node;
        }

        private RectInt[] SplitContainer(RectInt grid)
        {
            RectInt[] result = new RectInt[2];

            if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f)
            {
                result[0] = new RectInt(grid.x, grid.y, grid.width, (int)Random.Range(grid.height * 0.3f, grid.height * 0.5f));
                result[1] = new RectInt(grid.x, grid.y + result[0].height, grid.width, grid.height - result[0].height);
            }
            else
            {
                result[0] = new RectInt(grid.x, grid.y, (int)Random.Range(grid.width * 0.3f, grid.width * 0.5f), grid.height);
                result[1] = new RectInt(grid.x + result[0].width, grid.y, grid.width - result[0].width, grid.height);
            }

            return result;
        }

        private void GenerateRooms(BSPTree node)
        {
            if (node.IsLeaf)
            {
                var randomX = Random.Range(5, node.Grid.width / 4);
                var randomY = Random.Range(5, node.Grid.height / 4);
                int roomX = node.Grid.x + randomX;
                int roomY = node.Grid.y + randomY;
                int roomW = node.Grid.width - (int)(randomX * Random.Range(1f, 2f));
                int roomH = node.Grid.height - (int)(randomY * Random.Range(1f, 2f));
                node.Room = new RectInt(roomX, roomY, roomW, roomH);
            }
            else
            {
                if (node.Left != null)
                {
                    GenerateRooms(node.Left);
                }

                if (node.Right != null)
                {
                    GenerateRooms(node.Right);
                }
            }
        }

        private void GenerateCorridors(BSPTree node)
        {
            if (node.IsInternal)
            {
                RectInt leftContainer = node.Left.Grid;
                RectInt rightContainer = node.Right.Grid;
                Vector2 leftCenter = leftContainer.center;
                Vector2 rightCenter = rightContainer.center;
                Vector2 direction = (rightCenter - leftCenter).normalized;
                while (Vector2.Distance(leftCenter, rightCenter) > 1)
                {
                    if (direction.Equals(Vector2.right))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            _floor.SetTile(new Vector3Int((int)leftCenter.x, (int)leftCenter.y + i, 0), _tiles[1]);
                        }
                    }
                    else if (direction.Equals(Vector2.up))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            _floor.SetTile(new Vector3Int((int)leftCenter.x + i, (int)leftCenter.y, 0), _tiles[1]);
                        }
                    }
                    leftCenter.x += direction.x;
                    leftCenter.y += direction.y;
                }

                if (node.Left != null)
                {
                    GenerateCorridors(node.Left);
                }
                if (node.Right != null)
                {
                    GenerateCorridors(node.Right);
                }
            }    
        }

        private void UpdateTilemap(BSPTree node)
        {
            if (node.IsLeaf)
            {
                for (int x = node.Room.x; x < node.Room.xMax; x++)
                {
                    for (int y = node.Room.y; y < node.Room.yMax; y++)
                    {
                        _floor.SetTile(new Vector3Int(x, y, 0), _tiles[0]);
                    }  
                }
            }
            else
            {
                if (node.Left != null)
                    UpdateTilemap(node.Left);
                if (node.Right != null)
                    UpdateTilemap(node.Right);
            }
        }
    }
}
