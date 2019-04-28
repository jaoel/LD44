using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class MapGenerator
    {
        private Tilemap _floor;
        private Tilemap _walls;
        private readonly TileContainer _tileContainer;
        private readonly InteractiveDungeonObject _interactiveObjectsContainer;
        private readonly ItemContainer _itemContainer;
        private readonly EnemyContainer _enemyContainer;

        private int _width;
        private int _height;
        List<GameObject> _interactiveObjects;
        List<GameObject> _enemies;

        public MapGenerator(TileContainer tileContainer, InteractiveDungeonObject interactiveObjects,
            ItemContainer itemContainer, EnemyContainer enemyContainer)
        {
            _interactiveObjects = new List<GameObject>();
            _enemies = new List<GameObject>();

            _floor = GameObject.Find("Floor").GetComponent<Tilemap>();
            _walls = GameObject.Find("Walls").GetComponent<Tilemap>();
            _tileContainer = tileContainer;
            _interactiveObjectsContainer = interactiveObjects;
            _itemContainer = itemContainer;
            _enemyContainer = enemyContainer;
        }

        public void DrawDebug()
        {
            //if (_root != null)
            //    BSPTree.DebugDrawBspNode(_root);
        }

        public Map GenerateDungeon(int subdivisions, int width, int height)
        {
            _width = width;
            _height = height;

            _floor.ClearAllTiles();
            _walls.ClearAllTiles();
            DestroyAllInteractiveObjects();

            int[,] collisionMap = new int[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    collisionMap[x, y] = -1;
                }
            }

            BSPTree root = Split(null, subdivisions, new RectInt(0, 0, width, height));
            GenerateRooms(root);
            GenerateCorridors(root, collisionMap);
            FillTilemap(root, collisionMap);
            PaintTilemap(collisionMap);

            Map map = new Map(root, _walls, _floor, _width, _height, collisionMap);
            PopulateMap(map);

            return map;
        }

        public void DestroyAllInteractiveObjects()
        {
            _interactiveObjects.ForEach(x =>
            {
                GameObject.Destroy(x);
            });
            _interactiveObjects.Clear();

            _enemies.ForEach(x =>
            {
                GameObject.Destroy(x);
            });
            _enemies.Clear();
        }

        void PopulateMap(Map map)
        {
            //Stairs to next level
            Vector3Int stairsPosition = map.GetOpenPositionInRoom(2, 2);
            _interactiveObjects.Add(GameObject.Instantiate(_interactiveObjectsContainer.Stairs,
                new Vector3(stairsPosition.x - 0.5f, stairsPosition.y - 0.5f, -1.0f), Quaternion.identity));

            Vector3Int powerupPos = map.GetOpenPositionInRoom(2, 2);
            _interactiveObjects.Add(GameObject.Instantiate(_itemContainer.Shotgun,
                new Vector3(powerupPos.x - 0.5f, powerupPos.y - 0.5f, -1.0f), Quaternion.identity)); 

            for (int i = 0; i < 20; i++)
            {
                Vector3Int spawnPos = map.GetOpenPositionInRoom(2, 2);
                _enemies.Add(GameObject.Instantiate(_enemyContainer.basicZombie, 
                    new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.5f, -1), Quaternion.identity));
            }
        }

        void GenerateFloor(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
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

        private void GenerateCorridors(BSPTree node, int[,] collisionMap)
        {
            if (node.IsInternal)
            {
                BSPTree left = node.Left;
                BSPTree right = node.Right;

                if (node.Left == null)
                    left = node.GetSibling();
                else if (node.Right == null)
                    right = node.GetSibling();

                RectInt leftContainer = left.Grid;
                RectInt rightContainer = right.Grid;
                Vector2 leftCenter = leftContainer.center;
                Vector2 rightCenter = rightContainer.center;
                Vector2 direction = (rightCenter - leftCenter).normalized;
                int corridorWidth = Random.Range(4, 6);
                while (Vector2.Distance(leftCenter, rightCenter) > 1)
                {
                    if (direction.Equals(Vector2.right))
                    {
                        for (int i = 0; i < corridorWidth; i++)
                        {
                            Vector3Int coords = new Vector3Int((int)leftCenter.x, (int)leftCenter.y + i, 0);
                            _floor.SetTile(coords, _tileContainer.FloorTiles[GetFloorTileIndex()]);
                            collisionMap[coords.x, coords.y] = 0;
                        }
                    }
                    else if (direction.Equals(Vector2.up))
                    {
                        for (int i = 0; i < corridorWidth; i++)
                        {
                            Vector3Int coords = new Vector3Int((int)leftCenter.x + i, (int)leftCenter.y, 0);
                            _floor.SetTile(coords, _tileContainer.FloorTiles[GetFloorTileIndex()]);
                            collisionMap[coords.x, coords.y] = 0;
                        }
                    }
                    leftCenter.x += direction.x;
                    leftCenter.y += direction.y;
                }

                if (node.Left != null)
                {
                    GenerateCorridors(node.Left, collisionMap);
                }
                if (node.Right != null)
                {
                    GenerateCorridors(node.Right, collisionMap);
                }
            }
        }

        private void FillTilemap(BSPTree node, int[,] collisionMap)
        {
            if (node.IsLeaf)
            {
                for (int x = node.Room.x; x < node.Room.xMax; x++)
                {
                    for (int y = node.Room.y; y < node.Room.yMax; y++)
                    { 
                        _floor.SetTile(new Vector3Int(x, y, 0), _tileContainer.FloorTiles[GetFloorTileIndex()]);
                        collisionMap[x, y] = 0;
                    }
                }
            }
            else
            {
                if (node.Left != null)
                    FillTilemap(node.Left, collisionMap);
                if (node.Right != null)
                    FillTilemap(node.Right, collisionMap);
            }
        }

        private void PaintTilemap(int[,] collisionMap)
        { 
            for(int x = 0; x < collisionMap.GetLength(0); x++)
            {
                for(int y = 0; y < collisionMap.GetLength(1); y++)
                {
                    Tile tile = GetTileByNeighbours(x, y);
                    if (tile != null)
                    {
                        _walls.SetTile(new Vector3Int(x, y, 0), tile);
                        collisionMap[x, y] = 1;
                    }
                }
            }
        }

        private int GetFloorTileIndex()
        {
            bool offTile = Random.Range(0.0f, 1.0f) < 0.01f;
            int tileIndex = 0;
            if (offTile)
                tileIndex = Random.Range(1, 4);

            return tileIndex;
        }

        private Tile GetTileByNeighbours(int x, int y)
        {
            TileBase currentTile = _floor.GetTile(new Vector3Int(x, y, 0));

            if (currentTile == null)
                return null;

            Tile bottomLeft = _floor.GetTile<Tile>(new Vector3Int(x - 1, y - 1, 0));
            Tile bottomMiddle = _floor.GetTile<Tile>(new Vector3Int(x, y - 1, 0));
            Tile bottomRight = _floor.GetTile<Tile>(new Vector3Int(x + 1, y - 1, 0));

            Tile middleLeft = _floor.GetTile<Tile>(new Vector3Int(x - 1, y, 0));
            Tile middleRight = _floor.GetTile<Tile>(new Vector3Int(x + 1, y, 0));

            Tile topLeft = _floor.GetTile<Tile>(new Vector3Int(x - 1, y + 1, 0));
            Tile topMiddle = _floor.GetTile<Tile>(new Vector3Int(x, y + 1, 0));
            Tile topRight = _floor.GetTile<Tile>(new Vector3Int(x + 1, y + 1, 0));

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

            Tile wallMiddleLeft = _walls.GetTile<Tile>(new Vector3Int(x - 1, y, 0));
            Tile wallMiddleRight = _walls.GetTile<Tile>(new Vector3Int(x + 1, y, 0));
            Tile wallTopMiddle = _walls.GetTile<Tile>(new Vector3Int(x, y + 1, 0));

            if (bottomMiddle != null && bottomLeft == null && wallMiddleLeft != null)
                return _tileContainer.TopRightOuter;

            if (bottomMiddle != null && bottomRight == null && wallMiddleRight == null)
                return _tileContainer.TopLeftOuter;

            if (topRight == null && wallMiddleLeft == null)
                return _tileContainer.BottomRightOuter;

            if (topLeft == null && topMiddle != null && wallMiddleLeft != null)
                return _tileContainer.BottomLeftOuter;

            return null;
        }  
    }
}
