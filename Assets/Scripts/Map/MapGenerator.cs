using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public TileContainer tileContainer;
    public InteractiveDungeonObject interactiveObjectContainer;
    public EnemyContainer enemyContainer;
    public TrapContainer trapContainer;

    public Tilemap floors;
    public Tilemap walls;

    private MillerParkLCG _random;
    private Timer _timer;

    private Map _debugMap;

    private static MapGenerator _instance = null;
    public static MapGenerator Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            _instance = FindObjectOfType<MapGenerator>();
            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a MapGenerator");
            }

            _instance.Initialize();
            return _instance;
        }
    }

    private void Initialize()
    {
        _random = new MillerParkLCG();
        _timer = new Timer();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.O))
        {
            Debug.Log("Seed " + _random.GetStartingSeed() + " copied to clipboard");
            GUIUtility.systemCopyBuffer = _random.GetStartingSeed().ToString();
        }

        if (Input.GetKey(KeyCode.P))
        {
            PostProcessTiles(_debugMap, null);
        }
    }

    public Map GenerateMap(long seed, in MapGeneratorParameters parameters)
    {
        _timer.Start();
        Map result = new Map(floors, walls, _random);

        _random.SetSeed(seed);

        GenerateCells(ref result, parameters);
        SeparateCells(ref result, parameters);
        int roomCount = IdentifyRooms(ref result, parameters);

        if (roomCount < 3)
        {
            seed += 1;
            return GenerateMap(seed, parameters);
        }

        Triangulate(ref result, parameters);
        GenerateLayoutGraph(ref result, parameters);
        GenerateCorridorGraph(ref result, parameters);
        PaintRooms(result, parameters);
        PaintCorridors(ref result, parameters);
        PaintTiles(result, parameters);
        RemoveDeadRooms(ref result, parameters);
        FindChokepoints(ref result, parameters);

        _timer.Stop();
        _timer.Print("MapGenerator.GenerateMap");
        _debugMap = result;
        return result;
    }

    public void PopulateMap(ref Map map, ref Player player, in MapGeneratorParameters parameters)
    {
        Tuple<MapNode, MapNode> startAndGoal = map.GetRoomsFurthestApart(true);

        if (_random.NextFloat() < 0.5f)
        {
            startAndGoal = new Tuple<MapNode, MapNode>(startAndGoal.Item2, startAndGoal.Item1);
        }

        player.transform.position = map.GetRandomPositionInRoom(1, 1, startAndGoal.Item1).ToVector3();
        CameraManager.Instance.SetCameraPosition(player.transform.position);

        map.AddInteractiveObject(Instantiate(interactiveObjectContainer.Stairs,
            map.GetRandomPositionInRoom(2, 2, startAndGoal.Item2).ToVector3(), Quaternion.identity));

        GenerateDoors(ref map, startAndGoal.Item1, startAndGoal.Item2, parameters);
        PostProcessTiles(map, parameters);
        /*
        int trapCount = _random.Range(0, 10);
        for (int i = 0; i < trapCount; i++)
        {
            Vector3Int pos = map.GetPositionInMap(2, 2, false).ToVector3Int();
            while (Vector3.Distance(pos, player.transform.position) < 4
                || map.InteractiveObjects.Any(x => Vector3.Distance(x.transform.position, pos) < 4))
            {
                pos = map.GetPositionInMap(2, 2, false).ToVector3Int();
            }

            map.AddInteractiveObject(GameObject.Instantiate(trapContainer.GetRandomTrap(),
                new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity).gameObject);
        }

        int enemyCount = 10;
        int shootingZombieCount = 10;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = map.GetPositionInMap(1, 1, true, new List<MapNode>() { startAndGoal.Item1 }).ToVector3();

            while (Vector3.Distance(player.transform.position, spawnPos) < 10)
            {
                spawnPos = map.GetPositionInMap(1, 1, true, new List<MapNode>() { startAndGoal.Item1 }).ToVector3();
            }

            map.AddEnemy(GameObject.Instantiate(enemyContainer.basicZombie,
                new Vector3(spawnPos.x, spawnPos.y, 0.0f), Quaternion.identity));
            map.Enemies[map.Enemies.Count - 1].SetActive(false);
            map.Enemies[map.Enemies.Count - 1].GetComponent<Enemy>().maxSpeedMultiplier = _random.Range(0.9f, 1.2f);
        }

        for (int i = 0; i < shootingZombieCount; i++)
        {
            Vector3Int spawnPos = map.GetPositionInMap(1, 1, true, new List<MapNode>() { startAndGoal.Item1 }).ToVector3Int();
            while (Vector3.Distance(spawnPos, player.transform.position) < 10)
            {
                spawnPos = map.GetPositionInMap(1, 1, true, new List<MapNode>() { startAndGoal.Item1 }).ToVector3Int();
            }

            GameObject type = _random.Range(0.0f, 1.0f) < 0.5f ? enemyContainer.shootingZombie : enemyContainer.shotgunZombie;
            map.AddEnemy(GameObject.Instantiate(type, new Vector3(spawnPos.x, spawnPos.y, 0.0f), Quaternion.identity));
            map.Enemies[map.Enemies.Count - 1].SetActive(false);
        }
        */
    }

    private void GenerateCells(ref Map map, in MapGeneratorParameters parameters)
    {
        int cellCount = _random.Range(parameters.MinCellCount, parameters.MaxCellCount);
        map.Cells = new List<MapNode>(cellCount);

        int xMin = int.MaxValue;
        int xMax = int.MinValue;
        int yMin = int.MaxValue;
        int yMax = int.MinValue;

        for (int i = 0; i < cellCount; i++)
        {
            Vector2Int size = GenerateRandomSize(parameters);
            Vector2Int position = RandomPointInCircle(parameters.GenerationRadius);

            MapNode room = new MapNode(i, position, size);
            map.Cells.Add(new MapNode(i, position, size));
            map.Cells = map.Cells.OrderBy(x => x.Cell.x).ThenBy(x => x.Cell.y).ToList();

            if (room.Cell.xMin < xMin)
            {
                xMin = room.Cell.xMin;
            }
            if (room.Cell.xMax > xMax)
            {
                xMax = room.Cell.xMax;
            }
            if (room.Cell.yMin < yMin)
            {
                yMin = room.Cell.yMin;
            }
            if (room.Cell.yMax > yMax)
            {
                yMax = room.Cell.yMax;
            }
        }
        map.Bounds = new BoundsInt(xMin, yMin, 0, Mathf.Abs(xMax - xMin), Mathf.Abs(yMax - yMin), 0);
    }

    private void SeparateCells(ref Map map, in MapGeneratorParameters parameters)
    {
        bool regionsSeparated = false;
        int iterations = 0;

        int xMin = map.Bounds.xMin;
        int xMax = map.Bounds.xMax;
        int yMin = map.Bounds.yMin;
        int yMax = map.Bounds.yMax;

        while (!regionsSeparated && iterations < 3 * map.Cells.Count)
        {
            regionsSeparated = true;
            foreach (MapNode node in map.Cells)
            {
                Vector2 movement = Vector2.zero;
                int separationCount = 0;
                foreach (MapNode other in map.Cells)
                {
                    if (node == other)
                    {
                        continue;
                    }

                    RectInt overSizeNode = node.Cell;
                    overSizeNode.width += parameters.MinRoomDistance;
                    overSizeNode.height += parameters.MinRoomDistance;

                    if (!overSizeNode.Overlaps(other.Cell))
                    {
                        continue;
                    }

                    movement += other.Cell.center - overSizeNode.center;
                    separationCount++;
                }

                if (separationCount > 0)
                {
                    movement *= -1.0f;
                    movement = movement.normalized;
                    RectInt newRect = node.Cell;
                    newRect.position += new Vector2Int((int)Math.Round(movement.x), (int)Math.Round(movement.y));

                    if (!newRect.Equals(node.Cell))
                    {
                        node.Cell = newRect;
                        regionsSeparated = false;
                    }

                    if (newRect.xMin < xMin)
                    {
                        xMin = newRect.xMin;
                    }
                    if (newRect.xMax > xMax)
                    {
                        xMax = newRect.xMax;
                    }
                    if (newRect.yMin < yMin)
                    {
                        yMin = newRect.yMin;
                    }
                    if (newRect.yMax > yMax)
                    {
                        yMax = newRect.yMax;
                    }
                }
            }
            iterations++;
        }

        map.Bounds = new BoundsInt(xMin - 1, yMin - 1, 0, Mathf.Abs(xMax - xMin) + 1, Mathf.Abs(yMax - yMin) + 1, 0);
        map.CollisionMap = new int[map.Bounds.size.x, map.Bounds.size.y];
        
        if (!regionsSeparated)
        {
            //We should iterate over nodes overlapping and discard them
            Debug.Log("Unable to separate all nodes");
        }
    }

    private int IdentifyRooms(ref Map map, in MapGeneratorParameters parameters)
    {

        double roomThresholdX = 0;
        double roomThresholdY = 0;

        foreach (MapNode node in map.Cells)
        {
            roomThresholdX += node.Cell.width;
            roomThresholdY += node.Cell.height;
        }

        roomThresholdX /= map.Cells.Count;
        roomThresholdY /= map.Cells.Count;

        roomThresholdX *= parameters.RoomThresholdMultiplier;
        roomThresholdY *= parameters.RoomThresholdMultiplier;

        int roomCount = 0;
        foreach (MapNode node in map.Cells)
        {
            if(node.Cell.width > roomThresholdX && node.Cell.height > roomThresholdY)
            {
                node.Type = MapNodeType.Room;
                roomCount++;
            }
        }

        return roomCount;
    }

    private void Triangulate(ref Map map, in MapGeneratorParameters parameters)
    {
        Delaunay.BowerWatsonDelaunay triangulator = new Delaunay.BowerWatsonDelaunay();
        IEnumerable<Delaunay.Vertex<MapNode>> vertices = map.Cells.Where(x => x.Type == MapNodeType.Room)
            .Select(x => new Delaunay.Vertex<MapNode>(x.Cell.center, x));
        IEnumerable<Delaunay.Triangle<MapNode>> triangles = triangulator.Triangulate(vertices);
        map.Triangles = triangles.ToList();

        HashSet<Delaunay.Edge<MapNode>> delaunayEdges = triangulator.GetDelaunayEdges(triangles);
        HashSet<Delaunay.Edge<MapNode>> gabrielGraph = triangulator.GetGabrielGraph(delaunayEdges, vertices);
        HashSet<Delaunay.Edge<MapNode>> emst = triangulator.GetPrimEMST(gabrielGraph, vertices.Distinct());

        map.DelaunayGraph = delaunayEdges.ToList();
        map.GabrielGraph = gabrielGraph.ToList();
        map.EMSTGraph = emst.ToList();
    }

    private void GenerateLayoutGraph(ref Map map, in MapGeneratorParameters parameters)
    {
        List<Delaunay.Edge<MapNode>> result = new List<Delaunay.Edge<MapNode>>(map.EMSTGraph);
        List<Delaunay.Edge<MapNode>> allowedEdges = new List<Delaunay.Edge<MapNode>>();

        foreach(Delaunay.Edge<MapNode> edge in map.GabrielGraph)
        {
            if (!allowedEdges.Contains(edge) && !map.EMSTGraph.Contains(edge))
            {
                allowedEdges.Add(edge);
            }
        }

        int extraEdgeCount = (int)Math.Round(allowedEdges.Count * parameters.MazeFactor);
        for (int i = 0; i < extraEdgeCount; i++)
        {
            int edgeIndex = _random.Range(0, allowedEdges.Count);
            result.Add(allowedEdges[edgeIndex]);
            allowedEdges.RemoveAt(edgeIndex);
        }

        map.LayoutGraph = result;
    }

    private void PaintRooms(in Map map, in MapGeneratorParameters parameters)
    {
        foreach (MapNode node in map.Cells)
        {
            if (node.Type != MapNodeType.Room)
            {
                continue;
            }

            PaintRoom(node.Cell, true);
        }
    }

    private void PaintRoom(RectInt cell, bool paintWalls)
    {
        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = tileContainer.FloorTiles[0];
        }

        floors.SetTilesBlock(new BoundsInt(pos, size), tiles);

        if (paintWalls)
        {
            for (int i = 0; i < size.x * size.y; i++)
            {
                tiles[i] = null;

                int x = i % size.x + pos.x;
                int y = i / size.x + pos.y;

                if (x == pos.x && y == pos.y)
                {
                    tiles[i] = tileContainer.BottomLeft;
                }
                else if (x == pos.x && y == cell.yMax - 1)
                {
                    tiles[i] = tileContainer.TopLeft;
                }
                else if (x == cell.xMax - 1 && y == pos.y)
                {
                    tiles[i] = tileContainer.BottomRight;
                }
                else if (x == cell.xMax - 1 && y == cell.yMax - 1)
                {
                    tiles[i] = tileContainer.TopRight;
                }
                else if (y == cell.yMax - 1)
                {
                    tiles[i] = tileContainer.TopMiddle;
                }
                else if (y == pos.y)
                {
                    tiles[i] = tileContainer.BottomMiddle;
                }
                else if (x == cell.xMax - 1)
                {
                    tiles[i] = tileContainer.MiddleRight;
                }
                else if (x == pos.x)
                {
                    tiles[i] = tileContainer.MiddleLeft;
                }
            }

            walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
        }
    }

    private void PaintBox(RectInt cell)
    {
        Vector3Int pos = new Vector3Int(cell.xMin, cell.yMin, 0);
        Vector3Int size = new Vector3Int(cell.width, cell.height, 1);

        TileBase[] tiles = new TileBase[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = null;// tileContainer.FloorTiles[0];
        }

        floors.SetTilesBlock(new BoundsInt(pos, size), tiles);

        for (int i = 0; i < size.x * size.y; i++)
        {
            tiles[i] = null;

            int x = i % size.x + pos.x;
            int y = i / size.x + pos.y;

            if (x == pos.x && y == pos.y)
            {
                tiles[i] = tileContainer.BottomRightOuter;
            }
            else if (x == pos.x && y == cell.yMax - 1)
            {
                tiles[i] = tileContainer.TopLeftOuter;
            }
            else if (x == cell.xMax - 1 && y == pos.y)
            {
                tiles[i] = tileContainer.BottomLeftOuter;
            }
            else if (x == cell.xMax - 1 && y == cell.yMax - 1)
            {
                tiles[i] = tileContainer.TopRightOuter;
            }
            else if (y == cell.yMax - 1)
            {
                tiles[i] = tileContainer.BottomMiddle;
            }
            else if (y == pos.y)
            {
                tiles[i] = tileContainer.TopMiddle;
            }
            else if (x == cell.xMax - 1)
            {
                tiles[i] = tileContainer.MiddleLeft;
            }
            else if (x == pos.x)
            {
                tiles[i] = tileContainer.MiddleRight;
            }
        }

        walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
    }

    private void GenerateCorridorGraph(ref Map map, in MapGeneratorParameters parameters)
    {
        List<Delaunay.Edge<MapNode>> result = new List<Delaunay.Edge<MapNode>>();
        foreach (Delaunay.Edge<MapNode> edge in map.LayoutGraph)
        {
            MapNode a = null;
            MapNode b = null;

            foreach (MapNode node in map.Cells)
            {
                if (node.Type != MapNodeType.Room)
                {
                    continue;
                }

                if (a == null && node.Cell.Contains(edge.Point1.Position))
                {
                    a = node;
                }
                else if (b == null && node.Cell.Contains(edge.Point2.Position))
                {
                    b = node;
                }

                if (a != null && b != null)
                {
                    break;
                }
            }

            if (a == null || b == null)
            {
                continue;
            }

            Vector2 aCenter = a.Cell.center;
            Vector2 bCenter = b.Cell.center;
            Vector2 midpoint = (aCenter + bCenter) / 2;

            bool useX = false;
            if (midpoint.x >= a.Cell.xMin && midpoint.x <= a.Cell.xMax && midpoint.x >= b.Cell.xMin && midpoint.x <= b.Cell.xMax)
            {
                useX = true;
            }

            bool useY = false;
            if (midpoint.y >= a.Cell.yMin && midpoint.y <= a.Cell.yMax && midpoint.y >= b.Cell.yMin && midpoint.y <= b.Cell.yMax)
            {
                useY = true;
            }

            if (useX && useY)
            {
                if (_random.NextFloat() < 0.5f)
                {
                    useX = false;
                }
                else
                {
                    useY = false;
                }
            }

            if (useX)
            {
                // Hallway goes up/down

                if (midpoint.x < bCenter.x)
                {
                    midpoint.x -= 1;
                }
                if (midpoint.x > bCenter.x)
                {
                    midpoint.x += 1;
                }

                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(midpoint.x, aCenter.y),
                     new Delaunay.Vertex<MapNode>(midpoint.x, bCenter.y)));
                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(midpoint.x, bCenter.y),
                    new Delaunay.Vertex<MapNode>(bCenter.x, bCenter.y)));
            }
            else if (useY)
            {
                if (midpoint.y < bCenter.y)
                {
                    midpoint.y -= 1;
                }
                if (midpoint.y > bCenter.y)
                {
                    midpoint.x += 1;
                }

                // Hallway goes left/right
                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(aCenter.x, midpoint.y),
                      new Delaunay.Vertex<MapNode>(bCenter.x, midpoint.y)));
                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(bCenter.x, midpoint.y),
                   new Delaunay.Vertex<MapNode>(bCenter.x, bCenter.y)));
            }
            else
            {
                // Hallway has a corner
                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(aCenter.x, aCenter.y),
                        new Delaunay.Vertex<MapNode>(aCenter.x, bCenter.y)));
                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(aCenter.x, bCenter.y),
                    new Delaunay.Vertex<MapNode>(bCenter.x, bCenter.y)));
            }

            a.Corridors.Add(edge);
            b.Corridors.Add(edge);
        }

        map.CorridorGraph = result;
    }

    private void AddCorridorRooms(ref Map map, in MapGeneratorParameters parameters, BoundsInt corridorBounds)
    {
        foreach (MapNode room in map.Cells)
        {
            if (room.Type != MapNodeType.Default)
            {
                continue;
            }

            if (corridorBounds.Intersects(room.Cell, out RectInt area))
            {
                if (area.width > 1 && area.height > 1)
                {
                    room.Type = MapNodeType.Corridor;
                    PaintRoom(room.Cell, _random.NextFloat() < parameters.CorridorRoomConnectionFactor);
                }
            }
        }
    }

    private void PaintCorridors(ref Map map, in MapGeneratorParameters parameters)
    {
        Vector3Int size = Vector3Int.zero;
        int width = 0;
        for (int i = 0; i < map.CorridorGraph.Count; i++)
        {
            Delaunay.Edge<MapNode> corridor = map.CorridorGraph[i];
            int minX = (int)Mathf.Floor(Mathf.Min(corridor.Point1.Position.x, corridor.Point2.Position.x));
            int maxX = (int)Mathf.Floor(Mathf.Max(corridor.Point1.Position.x, corridor.Point2.Position.x));
            int minY = (int)Mathf.Floor(Mathf.Min(corridor.Point1.Position.y, corridor.Point2.Position.y));
            int maxY = (int)Mathf.Floor(Mathf.Max(corridor.Point1.Position.y, corridor.Point2.Position.y));

            Vector3Int pos = new Vector3Int(minX, minY, 0);
            size = new Vector3Int(Math.Abs(maxX - minX), Math.Abs(maxY - minY), 1);

            bool addOffset = false;
            if (i % 2 == 0)
            {
                width = _random.Range(parameters.MinCorridorWidth, parameters.MaxCorridorWidth);

                if (map.CorridorGraph[i + 1].Point2.Position.x < minX)
                {
                    addOffset = true;
                }
            }

            if (size.x <= 1)
            {
                size.x = width;
            }
            if (size.y <= 1)
            {
                size.y = width;
            }

            if (addOffset)
            {
                size.y += width;
            }

            TileBase[] tiles = new TileBase[size.x * size.y];
            TileBase[] wallTiles = new TileBase[size.x * size.y];
            for (int tileIndex = 0; tileIndex < size.x * size.y; tileIndex++)
            {
                tiles[tileIndex] = tileContainer.FloorTiles[0];
                wallTiles[tileIndex] = null;
            }

            BoundsInt bounds = new BoundsInt(pos, size);
            AddCorridorRooms(ref map, parameters, bounds);
            floors.SetTilesBlock(bounds, tiles);
            walls.SetTilesBlock(bounds, wallTiles);
        }
    }

    private void PaintTiles(in Map map, in MapGeneratorParameters parameters)
    {
        //_timer.Start();
        for(int i = 0; i < map.Bounds.size.x; i++)
        {
            for(int j = 0; j < map.Bounds.size.y; j++)
            {
                int x = map.Bounds.xMin + i;
                int y = map.Bounds.yMin + j;
                RemoveThinWalls(x, y);

                Tile tile = GetTileByNeighbours(x, y);
                if (tile == null)
                {
                    continue;
                }

                map.CollisionMap[i, j] = 1;
                walls.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        //_timer.Stop();
        //_timer.Print("MapGenerator.PaintTiles()");
    }

    private bool RemoveThinWalls(int x, int y)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);

        Tile middleRightWall = (Tile)walls.GetTile(pos + new Vector3Int(1, 0, 0));
        Tile middleLeftWall = (Tile)walls.GetTile(pos - new Vector3Int(1, 0, 0));
        Tile middleTopWall = (Tile)walls.GetTile(pos + new Vector3Int(0, 1, 0));
        Tile middleBottomWall = (Tile)walls.GetTile(pos - new Vector3Int(0, 1, 0));

        Tile middleLeftFloor = (Tile)floors.GetTile(pos - new Vector3Int(1, 0, 0));
        Tile middleRightFloor = (Tile)floors.GetTile(pos + new Vector3Int(1, 0, 0));
        Tile middleTopFloor = (Tile)floors.GetTile(pos + new Vector3Int(0, 1, 0));
        Tile middleBottomFloor = (Tile)floors.GetTile(pos - new Vector3Int(0, 1, 0));

        if (middleLeftFloor != null && middleRightFloor != null && middleRightWall == null && middleLeftWall == null)
        {
            walls.SetTile(pos, null);
            return true;
        }

        if (middleTopFloor != null && middleBottomFloor != null && middleTopWall == null && middleBottomWall == null)
        {
            walls.SetTile(pos, null);
            return true;
        }

        return false;
    }

    private void PostProcessTiles(in Map map, in MapGeneratorParameters parameters)
    {
        for (int x = map.Bounds.xMin; x < map.Bounds.xMax; x++)
        {
            for (int y = map.Bounds.yMin; y < map.Bounds.yMax; y++)
            {
                if (RemoveThinWalls(x, y))
                {
                    continue;
                }

                Vector3Int pos = new Vector3Int(x, y, 0);
                Tile currentWallTile = (Tile)walls.GetTile(pos);

                Tile result = null;

                BoundsInt blockBounds = new BoundsInt(x - 1, y - 1, 0, 3, 3, 1);
                TileBase[] wallBlock = walls.GetTilesBlock(blockBounds);
                TileBase[] floorBlock = floors.GetTilesBlock(blockBounds);

                Tile floorBottomLeft = (Tile)floorBlock[0];
                Tile floorBottomMid = (Tile)floorBlock[1];
                Tile floorBottomRight = (Tile)floorBlock[2];
                Tile floorMiddleLeft = (Tile)floorBlock[3];
                Tile floorMiddleRight = (Tile)floorBlock[5];
                Tile floorTopLeft = (Tile)floorBlock[6];
                Tile floorTopMid = (Tile)floorBlock[7];
                Tile floorTopRight = (Tile)floorBlock[8];
                
                Tile wallBottomLeft = (Tile)wallBlock[0];
                Tile wallBottomMid = (Tile)wallBlock[1];
                Tile wallBottomRight = (Tile)wallBlock[2];
                Tile wallMiddleLeft = (Tile)wallBlock[3];
                Tile wallMiddleRight = (Tile)wallBlock[5];
                Tile wallTopLeft = (Tile)wallBlock[6];
                Tile wallTopMid = (Tile)wallBlock[7];
                Tile wallTopRight = (Tile)wallBlock[8];

                if (currentWallTile != null)
                {
                    if (wallBlock.Where(tile => tile != null).Count() <= 1)
                    {
                        walls.SetTile(new Vector3Int(x, y, 0), null);
                        continue;
                    }

                    if (wallMiddleLeft == null && wallBottomLeft == null && wallBottomMid == null
                        && wallMiddleRight != null && wallTopMid != null)
                    {
                        if (floorBottomMid == null)
                        {
                            result = tileContainer.BottomLeft;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else
                        {
                            result = tileContainer.BottomRightOuter;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }

                    if (wallMiddleLeft != null && wallBottomMid == null && wallBottomRight == null 
                        && wallMiddleRight == null && wallTopMid != null)
                    {
                        if (floorBottomMid == null)
                        {
                            result = tileContainer.BottomRight;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else
                        {
                            if (wallMiddleLeft == tileContainer.TopLeftOuter)
                            {
                                result = tileContainer.BottomRight;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                            else
                            {
                                result = tileContainer.BottomLeftOuter;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                        }
                    }

                    if (wallMiddleLeft == null && wallBottomMid != null && wallMiddleRight != null && wallTopMid == null)
                    {
                        if (floorTopMid == null)
                        {
                            result = tileContainer.TopLeft;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else
                        {
                            result = tileContainer.TopLeftOuter;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }

                    if (wallMiddleLeft != null && wallBottomMid != null && wallMiddleRight == null)
                    {
                        if (wallTopMid == null)
                        {
                            if (floorTopMid == null)
                            {
                                result = tileContainer.TopRight;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                            else
                            {
                                result = tileContainer.TopRightOuter;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                        }
                        else
                        {
                            if (wallBottomMid == tileContainer.MiddleRight || wallBottomMid == tileContainer.BottomRightOuter)
                            {
                                result = tileContainer.TopRight;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                            else if ((wallMiddleLeft == tileContainer.BottomMiddle || wallMiddleLeft == tileContainer.TopLeftOuter)
                                && (wallTopMid == tileContainer.MiddleRight || wallTopMid == tileContainer.TopLeftOuter))
                            {
                                result = tileContainer.BottomRight;
                                walls.SetTile(new Vector3Int(x, y, 0), result);
                                continue;
                            }
                        }
                        
                    }

                    if (wallMiddleLeft != null && wallBottomMid != null && wallMiddleRight != null)
                    {
                        if (floorTopMid == null && wallMiddleLeft == tileContainer.TopMiddle)
                        {
                            result = tileContainer.TopRight;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else if (floorTopMid == null &&  wallMiddleLeft == tileContainer.MiddleRight)
                        {
                            result = tileContainer.TopLeft;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }

                    if (wallMiddleLeft != null && wallBottomMid == null && wallMiddleRight != null)
                    {
                        if (floorTopMid != null && floorTopRight == null && floorBottomMid == null)
                        {
                            result = tileContainer.BottomRight;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }

                    if (currentWallTile != tileContainer.MiddleLeft && currentWallTile != tileContainer.MiddleRight
                        && currentWallTile != tileContainer.TopMiddle && currentWallTile != tileContainer.BottomMiddle)
                    {
                        if (wallBottomMid != null && floorMiddleRight != null && floorMiddleLeft == null && wallMiddleRight == null)
                        {
                            result = tileContainer.MiddleLeft;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else if (wallMiddleLeft != null && floorBottomMid != null && wallBottomMid == null)
                        {
                            result = tileContainer.TopMiddle;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else if (wallMiddleLeft != null && floorTopMid != null && wallTopMid == null)
                        {
                            result = tileContainer.BottomMiddle;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                        else if (wallBottomMid != null && floorMiddleLeft != null && floorMiddleRight == null)
                        {
                            result = tileContainer.MiddleRight;
                            walls.SetTile(new Vector3Int(x, y, 0), result);
                            continue;
                        }
                    }
                }
            }
        }
    }

    private void RemoveDeadRooms(ref Map map, in MapGeneratorParameters parameters)
    {
        map.Cells.RemoveAll(x => x.Type == MapNodeType.Default || x.Type == MapNodeType.None);
    }

    private void FindChokepoints(ref Map map, in MapGeneratorParameters parameters)
    {
        foreach (MapNode room in map.Cells)
        {
            if (room.Type != MapNodeType.Room)
            {
                continue;
            }

            bool roomLockable = true;
            RectInt overSizeRoom = room.Cell;
            int offset = 1;
            for (int i = 0; i < offset; i++)
            {
                BoundsInt upper = new BoundsInt();
                upper.xMin = overSizeRoom.xMin;
                upper.xMax = overSizeRoom.xMax;
                upper.yMin = overSizeRoom.yMax - 1 - i;
                upper.yMax = overSizeRoom.yMax - i;
                upper.zMax = 1;

                List<BoundsInt> result = FindChokepoints(upper, true, out bool fullWall, out bool lockable);
                room.Chokepoints.AddRange(result);
                map.ChokePoints.AddRange(result);

                if (!lockable)
                {
                    roomLockable = false;
                }

                if (result.Count > 0 || fullWall)
                {
                    break;
                }
            }

            
            for (int i = 0; i < offset; i++)
            {
                BoundsInt lower = new BoundsInt();
                lower.xMin = overSizeRoom.xMin;
                lower.xMax = overSizeRoom.xMax;
                lower.yMin = overSizeRoom.yMin + i;
                lower.yMax = overSizeRoom.yMin + 1 + i;
                lower.zMax = 1;
            
                List<BoundsInt> result = FindChokepoints(lower, true, out bool fullWall, out bool lockable);
                room.Chokepoints.AddRange(result);
                map.ChokePoints.AddRange(result);

                if (!lockable)
                {
                    roomLockable = false;
                }

                if (result.Count > 0 || fullWall)
                {
                    break;
                }
            }
            
            for (int i = 0; i < offset; i++)
            {
                BoundsInt left = new BoundsInt();
                left.xMin = overSizeRoom.xMin + i;
                left.xMax = overSizeRoom.xMin + 1 + i;
                left.yMin = overSizeRoom.yMin;
                left.yMax = overSizeRoom.yMax;
                left.zMax = 1;
            
                List<BoundsInt> result = FindChokepoints(left, false, out bool fullWall, out bool lockable);
                room.Chokepoints.AddRange(result);
                map.ChokePoints.AddRange(result);

                if (!lockable)
                {
                    roomLockable = false;
                }

                if (result.Count > 0 || fullWall)
                {
                    break;
                }
            }
            
            for (int i = 0; i < offset; i++)
            {
                BoundsInt right = new BoundsInt();
                right.xMin = overSizeRoom.xMax - 1 - i;
                right.xMax = overSizeRoom.xMax - i;
                right.yMin = overSizeRoom.yMin;
                right.yMax = overSizeRoom.yMax;
                right.zMax = 1;
            
                List<BoundsInt> result = FindChokepoints(right, false, out bool fullWall, out bool lockable);
                room.Chokepoints.AddRange(result);
                map.ChokePoints.AddRange(result);

                if (!lockable)
                {
                    roomLockable = false;
                }

                if (result.Count > 0 || fullWall)
                {
                    break;
                }
            }
            room.Lockable = roomLockable;
        }
    }

    private List<BoundsInt> FindChokepoints(BoundsInt bounds, bool horizontal, out bool fullWall, out bool lockable)
    {
        fullWall = true;
        lockable = false;
        List<BoundsInt> result = new List<BoundsInt>();
        TileBase[] tempWalls = walls.GetTilesBlock(bounds);
        TileBase[] tempFloors = floors.GetTilesBlock(bounds);

        for (int i = 0; i < tempWalls.Length - 1; i++)
        {
            if (tempWalls[i] == null)
            {
                fullWall = false;
            }

            if (tempWalls[i] != null && tempWalls[i + 1] == null && tempFloors[i + 1] != null)
            {
                fullWall = false;
                BoundsInt chokepoint = bounds;
                for (int j = i + 2; j < tempWalls.Length; j++)
                {
                    if (tempWalls[j] != null)
                    {
                        if (horizontal)
                        {
                            chokepoint.xMin = bounds.xMin + i;
                            chokepoint.xMax = bounds.xMin + j + 1;
                        }
                        else
                        {
                            chokepoint.yMin = bounds.yMin + i;
                            chokepoint.yMax = bounds.yMin + j + 1;
                        }
                        result.Add(chokepoint);
                        i = j;
                        lockable = true;
                        break;
                    }
                    lockable = false;
                }
            }
        }

        if (fullWall)
        {
            lockable = true;
        }

        return result;
    }

    private void GenerateDoors(ref Map map, MapNode spawnRoom, in MapNode exitRoom, in MapGeneratorParameters parameters)
    {
        List<MapNode> rooms = new List<MapNode>(map.Cells.Where(x => x.Type == MapNodeType.Room));
        
        int lockedDoorCount = (int)Mathf.Round(rooms.Count * parameters.LockFactor);
        rooms.Remove(spawnRoom);
        rooms.Remove(exitRoom);

        LockRoom(map, spawnRoom, exitRoom, rooms, out MapNode keyRoom, true);
        for (int i = 0; i < lockedDoorCount; i++)
        {
            MapNode room = rooms[_random.Range(0, rooms.Count)];

            if (!room.Lockable)
            {
                continue;
            }

            LockRoom(map, spawnRoom, room, rooms, out keyRoom, false);
            rooms.Remove(keyRoom);

            if (rooms.Count == 0)
            {
                break;
            }
        }
    }

    private bool LockRoom(Map map, MapNode spawnRoom, MapNode target, List<MapNode> rooms, out MapNode keyRoom, bool goldKey)
    {
        keyRoom = null;
        List<Door> doors = new List<Door>();
        foreach (BoundsInt chokepoint in target.Chokepoints)
        {
            Door door = null;
            if (chokepoint.size.x > 1)
            {
                if (chokepoint.size.x == 3)
                {
                    door = Instantiate(interactiveObjectContainer.horizontalDoor, chokepoint.center, 
                        Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.xMin + 1, chokepoint.yMin, 1, 1);
                }
                else if (chokepoint.size.x == 4)
                {
                    door = Instantiate(interactiveObjectContainer.horizontalDoor, chokepoint.center, 
                        Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.xMin + 1, chokepoint.yMin, 2, 1);
                }
                else
                {
                   
                    door = Instantiate(interactiveObjectContainer.horizontalDoor, new Vector3(chokepoint.xMin + 2, chokepoint.center.y),
                        Quaternion.identity).GetComponent<Door>();
                    BuildHorizontalDoorWalls(map, target, chokepoint);
                    door.Bounds = new RectInt(chokepoint.xMin + 1, chokepoint.yMin, 2, 1);
                }
            }
            else if (chokepoint.size.y > 1)
            {
                if (chokepoint.size.y == 3)
                {
                    door = Instantiate(interactiveObjectContainer.verticalDoor, chokepoint.center, Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.x, chokepoint.yMin + 1, 1, 1);
                }
                else if (chokepoint.size.y == 4)
                {
                    door = Instantiate(interactiveObjectContainer.verticalDoor, chokepoint.center, Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.x, chokepoint.yMin + 1, 1, 2);
                }
                else
                {
                    door = Instantiate(interactiveObjectContainer.verticalDoor, new Vector3(chokepoint.center.x, chokepoint.yMin + 2),
                        Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.x, chokepoint.yMin + 1, 1, 2);
                    BuildVerticalDoorWalls(map, target, chokepoint);
                }
            }

            if (door != null)
            {
                doors.Add(door);
                map.UpdateCollisionMap(chokepoint.ToRectInt(), 1);
                map.UpdateCollisionMap(door.Bounds, 0);
                map.AddInteractiveObject(door.gameObject);
            }
        }

        if (doors.Count > 0)
        {
            for (int i = 0; i < doors.Count; i++)
            {
                for (int j = 0; j < doors.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    doors[i].Siblings.Add(doors[j]);
                }
            }

            Delaunay.Vertex<MapNode> lockedRoom = new Delaunay.Vertex<MapNode>(target.Cell.center, target);
            rooms.ForEach(x =>
            {
                x.Corridors.RemoveAll(corridor => corridor.ContainsVertex(lockedRoom));
            });

            rooms.RemoveAll(x => x.Equals(target));
            keyRoom = FindKeyRoom(spawnRoom, target, rooms);

            Key newKey = null;
            if (goldKey)
            {
                newKey = Instantiate(interactiveObjectContainer.goldKey, map.GetRandomPositionInRoom(1, 1, keyRoom).ToVector3(), 
                    Quaternion.identity).GetComponent<Key>();
            }
            else
            {
                newKey = Instantiate(interactiveObjectContainer.skeletonKey, map.GetRandomPositionInRoom(1, 1, keyRoom).ToVector3(), 
                    Quaternion.identity).GetComponent<Key>();
            }

            doors.ForEach(x =>
            {
                x.IsGoalDoor = goldKey;
            });
            
            map.AddInteractiveObject(newKey.gameObject);
            return true;
        }

        return false;
    }

    private void BuildVerticalDoorWalls(Map map, MapNode target, BoundsInt chokepoint)
    {
        BoundsInt bounds = chokepoint;
        bounds.yMin += 3;
        BoundsInt drawArea = new BoundsInt(bounds.position, bounds.size);
        bounds.yMax -= 1;

        //Chokepoint is on the left edge
        if (chokepoint.center.x < target.Cell.center.x)
        {
            BoundsInt limit = bounds;
            limit.x -= 2;

            if (floors.GetTilesBlock(limit).Any(x => x != null) && !walls.GetTilesBlock(limit).Any(x => x != null))
            {
                drawArea.xMin -= 1;
                PaintBox(drawArea.ToRectInt());
            }
            else
            {
                drawArea.xMax += 1;
                PaintBox(drawArea.ToRectInt());
            }

            map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
        }
        //chokepoint is on the right edge
        else
        {
            BoundsInt limit = bounds;
            limit.x += 2;

            if (floors.GetTilesBlock(limit).Any(x => x != null) && !walls.GetTilesBlock(limit).Any(x => x != null))
            {
                drawArea.xMax += 1;
                PaintBox(drawArea.ToRectInt());
            }
            else
            {
                drawArea.xMin -= 1;
                PaintBox(drawArea.ToRectInt());
            }

            map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
        }
    }

    private void BuildHorizontalDoorWalls(Map map, MapNode target, BoundsInt chokepoint)
    {
        BoundsInt bounds = new BoundsInt(chokepoint.position, chokepoint.size);
        bounds.xMin += 3;

        BoundsInt drawArea = new BoundsInt(bounds.position, bounds.size);

        bounds.xMax -= 1;

        Tile[] tileBlock = new Tile[drawArea.size.x * drawArea.size.y];

        //Chokepoint is on the lower edge
        if (chokepoint.center.y < target.Cell.center.y)
        {
            BoundsInt limit = bounds;
            limit.y -= 2;

            if (floors.GetTilesBlock(limit).Any(x => x != null) && !walls.GetTilesBlock(limit).Any(x => x != null))
            {
                drawArea.yMin -= 1;
                PaintBox(drawArea.ToRectInt());
                map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
            }
            else
            {
                drawArea.yMax += 1;
                PaintBox(drawArea.ToRectInt());
                map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
            }
        }
        //chokepoint is on the upper edge
        else
        {
            BoundsInt limit = bounds;
            limit.y += 2;

            if (floors.GetTilesBlock(limit).Any(x => x != null) && !walls.GetTilesBlock(limit).Any(x => x != null))
            {
                drawArea.yMax += 1;
                PaintBox(drawArea.ToRectInt());
                map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
            }
            else
            {
                drawArea.yMin -= 1;
                PaintBox(drawArea.ToRectInt());
                map.UpdateCollisionMap(drawArea.ToRectInt(), 1);
            }
        }
    }

    private MapNode FindKeyRoom(MapNode spawnRoom, MapNode target, List<MapNode> rooms)
    {
        List<Tuple<int, float>> distances = new List<Tuple<int, float>>();

        rooms.ForEach(x =>
        {
            float distToSpawn = (x.Cell.center - spawnRoom.Cell.center).sqrMagnitude;
            float distToTarget = (x.Cell.center - target.Cell.center).sqrMagnitude;

            distances.Add(new Tuple<int, float>(x.Id, Mathf.Min(distToSpawn, distToTarget)));
        });

        distances = distances.OrderByDescending(x => x.Item2).ToList();

        MapNode result = null;
        while(result == null)
        {
            if (distances.Count == 0)
            {
                result = spawnRoom;
                break;
            }

            MapNode candidate = rooms.Single(x => x.Id == distances[0].Item1);
            List<MapNode> path = NavigationManager.Instance.AStar(spawnRoom, candidate, out float distance);
            distances.RemoveAt(0);

            if (path != null)
            {
                result = candidate;
            }
        }

        return result;
    }

    private Vector2Int GenerateRandomSize(in MapGeneratorParameters parameters)
    {
        int width = _random.Range(parameters.MinCellSize, parameters.MaxCellSize);
        int height = _random.Range(parameters.MinCellSize, parameters.MaxCellSize);

        return new Vector2Int(width, height);
    }

    private Vector2Int RandomPointInCircle(float radius)
    {
        double r = radius * Math.Sqrt(_random.NextFloat());
        double theta = _random.NextFloat() * 2 * Math.PI;

        int x = (int)Math.Round(r * Math.Cos(theta));
        int y = (int)Math.Round(r * Math.Sin(theta));

        return new Vector2Int(x, y);
    }

    private Tile GetTileByNeighbours(int x, int y, bool recursive = false, bool ignoreCurrent = false)
    {
        if (x == -12 && y == -43)
        {
            //return tileContainer.FloorTiles[3];
            int foo = 0;
        }

        TileBase currentFloorTile = floors.GetTile(new Vector3Int(x, y, 0));
        Tile currentWallTile = (Tile)walls.GetTile(new Vector3Int(x, y, 0));

        if (currentFloorTile == null)
        {
            return null;
        }

        if (currentWallTile != null && !ignoreCurrent)
        {
            return currentWallTile;
        }

        BoundsInt blockBounds = new BoundsInt(x - 1, y - 1, 0, 3, 3, 1);
        TileBase[] wallBlock = walls.GetTilesBlock(blockBounds);
        TileBase[] floorBlock = floors.GetTilesBlock(blockBounds);

        Tile floorBottomLeft    = (Tile)floorBlock[0];
        Tile floorBottomMiddle  = (Tile)floorBlock[1];
        Tile floorBottomRight   = (Tile)floorBlock[2];
        Tile floorMiddleLeft    = (Tile)floorBlock[3];
        Tile floorMiddleRight   = (Tile)floorBlock[5];
        Tile floorTopLeft       = (Tile)floorBlock[6];
        Tile floorTopMiddle     = (Tile)floorBlock[7];
        Tile floorTopRight      = (Tile)floorBlock[8];

        Tile wallBottomLeft     = (Tile)wallBlock[0];
        Tile wallBottomMiddle   = (Tile)wallBlock[1];
        Tile wallBottomRight    = (Tile)wallBlock[2];
        Tile wallMiddleLeft     = (Tile)wallBlock[3];
        Tile wallMiddleRight    = (Tile)wallBlock[5];
        Tile wallTopLeft        = (Tile)wallBlock[6];
        Tile wallTopMiddle      = (Tile)wallBlock[7];
        Tile wallTopRight       = (Tile)wallBlock[8];

        if (floorTopMiddle != null && floorBottomMiddle != null)
        {
            if (floorMiddleLeft == null)
            {
                return tileContainer.MiddleLeft;
            }

            if (floorMiddleRight == null)
            {
                return tileContainer.MiddleRight;

            }
        }

        if (floorMiddleRight != null)
        {
            if (floorBottomMiddle == null && floorTopMiddle != null)
            {
                return tileContainer.BottomMiddle;
            }

            if (floorTopMiddle == null && floorBottomMiddle != null)
            {
                return tileContainer.TopMiddle;
            }
        }

        if (wallMiddleLeft != null)
        {
            if (wallMiddleLeft == tileContainer.BottomMiddle || wallMiddleLeft == tileContainer.TopLeftOuter)
            {
                if (wallBottomMiddle == tileContainer.MiddleLeft)
                {
                    return tileContainer.TopRightOuter;
                }

                if (floorMiddleRight == null && floorTopRight != null)
                {
                    return tileContainer.BottomRight;
                }
            }

            if (wallBottomMiddle != null && floorTopMiddle == null && floorMiddleRight == null)
            {
                return tileContainer.TopRight;
            }

            if (floorTopLeft == null && floorTopMiddle == null && floorTopRight == null && floorMiddleRight == null 
                && floorBottomMiddle != null && wallBottomLeft == null)
            {
                return tileContainer.FloorTiles[3];
            }

            if (floorBottomLeft == null && wallTopMiddle == null && wallTopRight == null && wallBottomMiddle != null /* && wallTopLeft != null*/)
            {
                return tileContainer.TopRightOuter;
            }
        }         
        
        if (wallBottomMiddle != null)
        {
            if (wallBottomMiddle == tileContainer.MiddleRight)
            {
                if (wallTopLeft != null && floorMiddleRight == null)
                {
                    return tileContainer.TopRight;
                }
            }

            if (/*wallMiddleRight == null &&*/ floorBottomRight == null && wallMiddleLeft == null && wallBottomLeft == null)
            {
                return tileContainer.TopLeftOuter;
            }
        }

        if (wallTopMiddle == tileContainer.MiddleRight && floorMiddleRight == null && floorBottomMiddle == null)
        {
            return tileContainer.FloorTiles[3];
        }

        if (wallMiddleLeft == null)
        {
            if (wallBottomMiddle == null && floorTopRight == null)
            {
                if (wallBottomRight != null)
                {
                    return tileContainer.BottomRightOuter;
                }

                if (wallBottomLeft == null && wallBottomRight == null && wallTopMiddle == null)
                {
                    return tileContainer.BottomRightOuter;
                }

                if (wallBottomLeft == null && wallBottomRight == null && wallTopMiddle != null)
                {
                    return tileContainer.BottomRightOuter;
                }
            }
        }

        if (recursive)
        {
            return null;
        }

        if (wallMiddleLeft == null)
        {
            if (GetTileByNeighbours(x + 1, y, true) != null /*&& wallBottomLeft != null*/ && floorBottomRight == null)
            {
                return tileContainer.TopLeftOuter;
            }

            if (GetTileByNeighbours(x + 1, y, true) != null && (wallBottomMiddle == tileContainer.MiddleRight 
                || (wallBottomMiddle == tileContainer.BottomRight && floorBottomRight == null)))
            {
                return tileContainer.TopLeftOuter;
            }

            if (wallBottomMiddle == null && wallTopRight != tileContainer.TopMiddle && GetTileByNeighbours(x - 1, y + 1) == null 
                && GetTileByNeighbours(x + 1, y, true) != null && GetTileByNeighbours(x, y + 1, true) != null && floorTopRight == null)
            {
                return tileContainer.BottomRightOuter;
            }
        }

        if (wallMiddleLeft != null)
        {
            if (wallBottomMiddle == null)
            {
                if ((GetTileByNeighbours(x, y + 1, true) == tileContainer.MiddleLeft || GetTileByNeighbours(x, y + 1, true) == tileContainer.MiddleRight))
                {
                    if (floorBottomMiddle == null)
                    {
                        return tileContainer.BottomRight;
                    }
                    else
                    {
                        return tileContainer.BottomLeftOuter;
                    }
                }

                if (floorTopRight == null && wallBottomRight != null && GetTileByNeighbours(x + 1, y, true) != null)
                {
                    return tileContainer.FloorTiles[3];
                }

                if (floorTopLeft == null && wallMiddleRight == null && GetTileByNeighbours(x, y + 1, true) != null)
                {
                    return tileContainer.BottomLeftOuter;
                }
            }

            if ((wallMiddleLeft == tileContainer.BottomMiddle || wallMiddleLeft == tileContainer.BottomRight) 
                && wallBottomMiddle == tileContainer.BottomLeft && GetTileByNeighbours(x + 1, y, true) == null 
                && GetTileByNeighbours(x, y + 1, true) == null)
            {
                return tileContainer.TopRightOuter;
            }
        }

        return null;
    }

    private Tile GetCornerTileByNeighbours(int x, int y)
    {
        TileBase currentFloorTile = floors.GetTile(new Vector3Int(x, y, 0));
        Tile currentWallTile = (Tile)walls.GetTile(new Vector3Int(x, y, 0));

        if (currentFloorTile == null)
        {
            return null;
        }

        if (currentWallTile != null)
        {
            return currentWallTile;
        }

        return null;
    }
}
