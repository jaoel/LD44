using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.Tilemaps;           

namespace Assets.Scripts
{
    public class MapGenerator
    {
        private Tilemap _floor;
        private Tilemap _walls;
        private BSPTree _root;
        private readonly TileContainer _tileContainer;

        public MapGenerator(TileContainer tileContainer)
        {
            _floor = GameObject.Find("Floor").GetComponent<Tilemap>();
            _walls = GameObject.Find("Walls").GetComponent<Tilemap>();

            _tileContainer = tileContainer;
            _root = GenerateDungeon(500, 500);

            FillTilemap(_root);
            PaintTilemap();
        }

        public void DrawDebug()
        {
            if (_root != null)
                BSPTree.DebugDrawBspNode(_root);
        }

        public BSPTree GenerateDungeon(int width, int height)
        {
            BSPTree root = Split(null, 10, new RectInt(0, 0, width, height));
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
                    _floor.SetTile(new Vector3Int(x, y, 0), _tileContainer.FloorTiles[0]);
                }
            }
        }

        public BSPTree Split(BSPTree parent, int level, RectInt grid)
        {
            BSPTree node = new BSPTree(parent, grid);

            if (level > 0)
            {
                RectInt[] newContainers = SplitContainer(grid);

                if (newContainers[0].width * 0.5f > 5.0f && newContainers[0].height * 0.5f > 5.0f)
                    node.Left = Split(node, level - 1, newContainers[0]);

                if (newContainers[1].width * 0.5f > 5.0f && newContainers[1].height * 0.5f > 5.0f)
                    node.Right = Split(node, level - 1, newContainers[1]);
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
            int minRoomDelta = Random.Range(0, 5);
            if (node.IsLeaf)
            {
                var randomX = Random.Range(minRoomDelta, node.Grid.width / 4);
                var randomY = Random.Range(minRoomDelta, node.Grid.height / 4);
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
                BSPTree left = node.Left;
                if (left == null)
                {
                    BSPTree parent = node.Parent;

                    while (parent.Left == null)
                    {
                        parent = parent.Parent;
                    }
                    left = parent.Left;
                }

                BSPTree right = node.Right;
                if (right == null)
                {
                    BSPTree parent = node.Parent;

                    while (parent.Right == null)
                    {
                        parent = parent.Parent;
                    }

                    right = parent.Right;
                }

                RectInt leftContainer = left.Grid;
                RectInt rightContainer = right.Grid;
                Vector2 leftCenter = leftContainer.center;
                Vector2 rightCenter = rightContainer.center;
                Vector2 direction = (rightCenter - leftCenter).normalized;
                int corridorWidth = Random.Range(3, 5);
                while (Vector2.Distance(leftCenter, rightCenter) > 1)
                {
                    if (direction.Equals(Vector2.right))
                    {
                        for (int i = 0; i < corridorWidth; i++)
                        {
                            _floor.SetTile(new Vector3Int((int)leftCenter.x, (int)leftCenter.y + i, 0), _tileContainer.FloorTiles[0]);
                        }
                    }
                    else if (direction.Equals(Vector2.up))
                    {
                        for (int i = 0; i < corridorWidth; i++)
                        {
                            _floor.SetTile(new Vector3Int((int)leftCenter.x + i, (int)leftCenter.y, 0), _tileContainer.FloorTiles[0]);
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

        private void FillTilemap(BSPTree node)
        {
            if (node.IsLeaf)
            {
                for (int x = node.Room.x; x < node.Room.xMax; x++)
                {
                    for (int y = node.Room.y; y < node.Room.yMax; y++)
                    {     
                        _floor.SetTile(new Vector3Int(x, y, 0), _tileContainer.FloorTiles[0]);  
                    }  
                }
            }
            else
            {
                if (node.Left != null)
                    FillTilemap(node.Left);
                if (node.Right != null)
                    FillTilemap(node.Right);
            }
        }

        private void PaintTilemap()
        { 
            for(int x = 0; x < 500; x++)
            {
                for(int y = 0; y < 500; y++)
                {
                    Tile tile = GetTileByNeighbours(x, y);
                    if (tile != null)
                        _walls.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        private Tile GetTileByNeighbours(int x, int y)
        {
            TileBase currentTile = _floor.GetTile(new Vector3Int(x, y, 0));

            if (currentTile == null)
                return null;

            TileBase bottomLeft = _floor.GetTile(new Vector3Int(x - 1, y - 1, 0));
            TileBase bottomMiddle = _floor.GetTile(new Vector3Int(x, y - 1, 0));
            TileBase bottomRight = _floor.GetTile(new Vector3Int(x + 1, y - 1, 0));

            TileBase middleLeft = _floor.GetTile(new Vector3Int(x - 1, y, 0));
            TileBase middleRight = _floor.GetTile(new Vector3Int(x + 1, y, 0));

            TileBase topLeft = _floor.GetTile(new Vector3Int(x - 1, y + 1, 0));
            TileBase topMiddle = _floor.GetTile(new Vector3Int(x, y + 1, 0));
            TileBase topRight = _floor.GetTile(new Vector3Int(x + 1, y + 1, 0));

            //left
            if (middleLeft == null && topMiddle == null)
                return _tileContainer.TopLeft;
            if (middleLeft == null && topMiddle != null && bottomMiddle != null)
                return _tileContainer.MiddleLeft;
            if (middleLeft == null && bottomMiddle == null && topMiddle != null)
                return _tileContainer.BottomLeft;

            //middle
            if (middleLeft != null && topMiddle == null && middleRight != null)
                return _tileContainer.TopMiddle;

            if (middleLeft != null && bottomMiddle == null && middleRight != null)
                return _tileContainer.BottomMiddle;
            
            // right
            if (middleLeft != null && topMiddle == null && middleRight == null)
                return _tileContainer.TopRight;
            if (topMiddle != null && bottomMiddle != null && middleRight == null)
                return _tileContainer.MiddleRight;
            if (topMiddle != null && bottomMiddle == null && middleRight == null)
                return _tileContainer.BottomRight;

            return null;
        }
    }
}
