using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class BSPMapGenerator
    {
        private Tilemap _floor;
        private Tilemap _walls;
        private readonly TileContainer _tileContainer;
        private readonly InteractiveDungeonObject _interactiveObjectsContainer;
        private readonly ItemContainer _itemContainer;
        private readonly EnemyContainer _enemyContainer;
        private readonly TrapContainer _trapContainer;

        private int _width;
        private int _height;
       
        public BSPMapGenerator(TileContainer tileContainer, InteractiveDungeonObject interactiveObjects,
            ItemContainer itemContainer, EnemyContainer enemyContainer, TrapContainer trapContainer)
        {
            _floor = GameObject.Find("Floor").GetComponent<Tilemap>();
            _walls = GameObject.Find("Walls").GetComponent<Tilemap>();
            _tileContainer = tileContainer;
            _interactiveObjectsContainer = interactiveObjects;
            _itemContainer = itemContainer;
            _enemyContainer = enemyContainer;
            _enemyContainer = enemyContainer;
            _trapContainer = trapContainer;
        }
           
        public void DrawDebug()
        {
            //if (_root != null)
            //    BSPTree.DebugDrawBspNode(_root);
        }   
                                    
        public BSPMap GenerateDungeon(int subdivisions, int width, int height, int currentLevel, Player player)
        {
            _width = width;
            _height = height;

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

            List<GameObject> interactiveObjects = new List<GameObject>();
            List<GameObject> enemies = new List<GameObject>();

            BSPMap map = new BSPMap(root, _walls, _floor, _width, _height, collisionMap);
            AddStairs(map, interactiveObjects);
            map.MovePlayerToSpawn(player);

            PopulateMap(map, player, currentLevel, interactiveObjects, enemies);
            enemies.ForEach(x => x.SetActive(true));
            map.SetInteractiveObjects(interactiveObjects, enemies);

            return map;
        }

        private void AddStairs(BSPMap map, List<GameObject> interactiveObjects)
        {
            //Stairs to next level
            Vector3Int stairsPosition = map.GetOpenPositionInRoom(2, 2);
            interactiveObjects.Add(GameObject.Instantiate(_interactiveObjectsContainer.Stairs,
                new Vector3(stairsPosition.x, stairsPosition.y, 0.0f), Quaternion.identity));

            map.stairs = interactiveObjects[0];
        }

        void PopulateMap(BSPMap map, Player player, int currentLevel, List<GameObject> interactiveObjects,
            List<GameObject> enemies)
        {
            int trapCount = Random.Range(0, 10);
            for(int i = 0; i < trapCount; i++)
            {
                Vector3Int pos = map.GetOpenPositionInRoom(2, 2);
                while(Vector3.Distance(pos, player.transform.position) < 4 
                    || interactiveObjects.Any(x => Vector3.Distance(x.transform.position, pos) < 4))
                {
                    pos = map.GetOpenPositionInRoom(2, 2);
                }

                interactiveObjects.Add(GameObject.Instantiate(_trapContainer.GetRandomTrap(),
                    new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity).gameObject);
            }

            int enemyCount = 0;
            int shootingZombieCount = 0;
            if (currentLevel <= 5)
            {
                enemyCount = (int)(5 + Math.Pow(1.5, 1.5 * currentLevel));
                shootingZombieCount = (currentLevel - 1) * 2;
            }
            else
            {
                enemyCount = (int)(15 + 10 * Math.Log(currentLevel));
                shootingZombieCount = enemyCount;
            }

            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPos = map.GetOpenPositionInRoom(1, 1);

                while(Vector3.Distance(player.transform.position, spawnPos) < 10)
                {
                    spawnPos = map.GetOpenPositionInRoom(1, 1);
                }

                enemies.Add(GameObject.Instantiate(_enemyContainer.basicZombie, 
                    new Vector3(spawnPos.x, spawnPos.y, 0.0f), Quaternion.identity));
                enemies[enemies.Count - 1].SetActive(false);
                enemies[enemies.Count - 1].GetComponent<Enemy>().maxSpeedMultiplier = Random.Range(0.9f, 1.2f);
            }

            for (int i = 0; i < shootingZombieCount; i++)
            {
                Vector3Int spawnPos = map.GetOpenPositionInRoom(1, 1);
                while (Vector3.Distance(spawnPos, player.transform.position) < 10)
                {
                    spawnPos = map.GetOpenPositionInRoom(2, 2);
                }

                GameObject type = Random.Range(0.0f, 1.0f) < 0.5f ? _enemyContainer.shootingZombie : _enemyContainer.shotgunZombie;
                enemies.Add(GameObject.Instantiate(type, new Vector3(spawnPos.x, spawnPos.y, 0.0f), Quaternion.identity));
                enemies[enemies.Count - 1].SetActive(false);
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

                if (newContainers[0].width * 0.3f <= 2.0f || newContainers[0].height * 0.3f <= 2.0f)
                    return node;
                if (newContainers[1].width * 0.3f <= 2.0f || newContainers[1].height * 0.3f <= 2.0f)
                    return node;

                node.Left = Split(node, level - 1, newContainers[0]);
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
                var randomX = Random.Range(0, node.Grid.width / 4);
                var randomY = Random.Range(0, node.Grid.height / 4);
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
                                                  
                RectInt leftContainer = left.Grid;
                RectInt rightContainer = right.Grid;

                Vector2 leftCenter = leftContainer.center;
                Vector2 rightCenter = rightContainer.center;

                int corridorWidth = Random.Range(4, 6);
                Vector2 direction = (rightCenter - leftCenter).normalized;

                while (Vector2.Distance(leftCenter, rightCenter) > 1)
                {
                    if (direction.Equals(Vector2.right))
                    {
                        for (int i = 0; i < corridorWidth; i++)
                        {
                            Vector3Int coords = new Vector3Int((int)leftCenter.x, (int)leftCenter.y + i, 0);
                            _floor.SetTile(coords, _tileContainer.FloorTiles[GetFloorTileIndex()]);
                            if (coords.y < collisionMap.GetLength(1))
                                collisionMap[coords.x, coords.y] = 0;
                        }
                    }
                    else if (direction.Equals(Vector2.up))
                    {
                        for (int i = 0; i < corridorWidth; i++)
                        {
                            Vector3Int coords = new Vector3Int((int)leftCenter.x + i, (int)leftCenter.y, 0);
                            _floor.SetTile(coords, _tileContainer.FloorTiles[GetFloorTileIndex()]);
                            if (coords.x < collisionMap.GetLength(0))
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
