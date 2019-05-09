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

    private static MapGenerator instance = null;
    public static MapGenerator Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            instance = FindObjectOfType<MapGenerator>();
            if (instance == null || instance.Equals(null))
            {
                Debug.LogError("The scene needs a MapGenerator");
            }

            instance.Initialize();
            return instance;
        }
    }

    private void Initialize()
    {
        _random = new MillerParkLCG();
        _timer = new Timer();
    }

    public Map GenerateMap(long seed, in MapGeneratorParameters parameters)
    {
        _timer.Start();
        Map result = new Map(floors, walls, _random);

        _random.SetSeed(seed);

        GenerateCells(ref result, parameters);
        SeparateCells(ref result, parameters);
        IdentifyRooms(ref result, parameters);
        Triangulate(ref result, parameters);
        GenerateLayoutGraph(ref result, parameters);
        GenerateCorridorGraph(ref result, parameters);
        PaintRooms(result, parameters);
        PaintCorridors(ref result, parameters);
        PaintTiles(result, parameters);
        PostProcessTiles(result, parameters);
        RemoveDeadRooms(ref result, parameters);

        _timer.Stop();
        _timer.Print("MapGenerator.GenerateMap");

        return result;
    }

    public void PopulateMap(ref Map map, ref Player player, in MapGeneratorParameters parameters)
    {
        player.transform.position = map.GetPositionInMap(1, 1, false, out MapNode playerSpawnRoom).ToVector3();
        CameraManager.Instance.SetCameraPosition(player.transform.position);

        map.AddInteractiveObject(Instantiate(interactiveObjectContainer.Stairs, 
            map.GetPositionInMap(2, 2, false, new List<MapNode>() { playerSpawnRoom }).ToVector3(), Quaternion.identity));

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
            Vector3 spawnPos = map.GetPositionInMap(1, 1, true).ToVector3();

            while (Vector3.Distance(player.transform.position, spawnPos) < 10)
            {
                spawnPos = map.GetPositionInMap(1, 1, true).ToVector3();
            }

            map.AddEnemy(GameObject.Instantiate(enemyContainer.basicZombie,
                new Vector3(spawnPos.x, spawnPos.y, 0.0f), Quaternion.identity));
            map.Enemies[map.Enemies.Count - 1].SetActive(false);
            map.Enemies[map.Enemies.Count - 1].GetComponent<Enemy>().maxSpeedMultiplier = _random.Range(0.9f, 1.2f);
        }

        for (int i = 0; i < shootingZombieCount; i++)
        {
            Vector3Int spawnPos = map.GetPositionInMap(1, 1, true).ToVector3Int();
            while (Vector3.Distance(spawnPos, player.transform.position) < 10)
            {
                spawnPos = map.GetPositionInMap(1, 1, true).ToVector3Int();
            }

            GameObject type = _random.Range(0.0f, 1.0f) < 0.5f ? enemyContainer.shootingZombie : enemyContainer.shotgunZombie;
            map.AddEnemy(GameObject.Instantiate(type, new Vector3(spawnPos.x, spawnPos.y, 0.0f), Quaternion.identity));
            map.Enemies[map.Enemies.Count - 1].SetActive(false);
        }
    }

    private void GenerateCells(ref Map map, in MapGeneratorParameters parameters)
    {
        int cellCount = _random.Range(parameters.MinCellCount, parameters.MaxCellCount);
        map.Cells = new List<MapNode>(cellCount);
      
        for (int i = 0; i < cellCount; i++)
        {
            Vector2Int size = GenerateRandomSize(parameters);
            Vector2Int position = RandomPointInCircle(parameters.GenerationRadius);

            map.Cells.Add(new MapNode(i, position, size));
            map.Cells = map.Cells.OrderBy(x => x.Cell.x).ThenBy(x => x.Cell.y).ToList();
        }

    }

    private void SeparateCells(ref Map map, in MapGeneratorParameters parameters)
    {
        bool regionsSeparated = false;
        int iterations = 0;

        int xMin = int.MaxValue;
        int xMax = int.MinValue;
        int yMin = int.MaxValue;
        int yMax = int.MinValue;

        while (!regionsSeparated && iterations < 2 * map.Cells.Count)
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

                    if (!node.Cell.Overlaps(other.Cell))
                    {
                        continue;
                    }

                    movement += other.Cell.center - node.Cell.center;
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

                    if (node.Cell.xMin < xMin)
                        xMin = node.Cell.xMin;
                    if (node.Cell.xMax > xMax)
                        xMax = node.Cell.xMax;
                    if (node.Cell.yMin < yMin)
                        yMin = node.Cell.yMin;
                    if (node.Cell.yMax > yMax)
                        yMax = node.Cell.yMax;
                }
            }
            iterations++;
        }

        map.Bounds = new BoundsInt(xMin, yMin, 0, Mathf.Abs(xMax - xMin), Mathf.Abs(yMax - yMin), 0);
        map.CollisionMap = new int[map.Bounds.size.x, map.Bounds.size.y];

        if (!regionsSeparated)
        {
            //We should iterate over nodes overlapping and discard them
            Debug.Log("Unable to separate all nodes");
        }
    }

    private void IdentifyRooms(ref Map map, in MapGeneratorParameters parameters)
    {
        foreach (MapNode node in map.Cells)
        {
            if ((node.Cell.width >= parameters.MinRoomWidth && node.Cell.height >= parameters.MinRoomHeight)
                || (node.Cell.height >= parameters.MinRoomWidth && node.Cell.width >= parameters.MinRoomHeight))
            {
                node.Type = MapNodeType.Room;
            }
        }
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
        HashSet<Delaunay.Edge<MapNode>> emst = triangulator.GetPrimEMST(gabrielGraph, vertices);

        map.DelaunayGraph = delaunayEdges.ToList();
        map.GabrielGraph = gabrielGraph.ToList();
        map.EMSTGraph = emst.ToList();
    }

    private void GenerateLayoutGraph(ref Map map, in MapGeneratorParameters parameters)
    {

    }

    private void PaintRooms(in Map map, in MapGeneratorParameters parameters)
    {
        foreach (MapNode node in map.Cells)
        {
            if (node.Type != MapNodeType.Room)
                continue;

            PaintRoom(node, true);
        }
    }

    private void PaintRoom(MapNode node, bool paintWalls)
    {
        Vector3Int pos = new Vector3Int(node.Cell.xMin, node.Cell.yMin, 0);
        Vector3Int size = new Vector3Int(node.Cell.width, node.Cell.height, 1);

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
                    tiles[i] = tileContainer.BottomLeft;
                else if (x == pos.x && y == node.Cell.yMax - 1)
                    tiles[i] = tileContainer.TopLeft;
                else if (x == node.Cell.xMax - 1 && y == pos.y)
                    tiles[i] = tileContainer.BottomRight;
                else if (x == node.Cell.xMax - 1 && y == node.Cell.yMax - 1)
                    tiles[i] = tileContainer.TopRight;
                else if (y == node.Cell.yMax - 1)
                    tiles[i] = tileContainer.TopMiddle;
                else if (y == pos.y)
                    tiles[i] = tileContainer.BottomMiddle;
                else if (x == node.Cell.xMax - 1)
                    tiles[i] = tileContainer.MiddleRight;
                else if (x == pos.x)
                    tiles[i] = tileContainer.MiddleLeft;
            }

            walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
        }
    }

    private void GenerateCorridorGraph(ref Map map, in MapGeneratorParameters parameters)
    {
        List<Delaunay.Edge<MapNode>> result = new List<Delaunay.Edge<MapNode>>();
        foreach (Delaunay.Edge<MapNode> edge in map.EMSTGraph)
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
                    midpoint.x -= 1;
                if (midpoint.x > bCenter.x)
                    midpoint.x += 1;

                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(midpoint.x, aCenter.y),
                     new Delaunay.Vertex<MapNode>(midpoint.x, bCenter.y)));
                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(midpoint.x, bCenter.y),
                    new Delaunay.Vertex<MapNode>(bCenter.x, bCenter.y)));
            }
            else if (useY)
            {
                if (midpoint.y < bCenter.y)
                    midpoint.y -= 1;
                if (midpoint.y > bCenter.y)
                    midpoint.x += 1;

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
        }

        map.CorridorGraph = result;
    }

    private void AddCorridorRooms(ref Map map, in MapGeneratorParameters parameters, BoundsInt corridorBounds)
    {
        foreach (MapNode room in map.Cells)
        {
            if (room.Type != MapNodeType.Default)
                continue;

            if (corridorBounds.Overlaps(room.Cell))
            {
                room.Type = MapNodeType.Corridor;
                PaintRoom(room, false);
                break;
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
                    addOffset = true;
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
                size.y += width;

            TileBase[] tiles = new TileBase[size.x * size.y];
            for(int tileIndex = 0; tileIndex < size.x * size.y; tileIndex++)
            {
                tiles[tileIndex] = tileContainer.FloorTiles[0];
            }

            BoundsInt bounds = new BoundsInt(pos, size);
            AddCorridorRooms(ref map, parameters, bounds);
            floors.SetTilesBlock(bounds, tiles);
            walls.SetTilesBlock(bounds, new TileBase[size.x * size.y]);
        }
    }

    private void PaintTiles(in Map map, in MapGeneratorParameters parameters)
    {
        int colX = 0;
        for(int x = map.Bounds.xMin; x < map.Bounds.xMax; x++)
        {
            int colY = 0;
            for(int y = map.Bounds.yMin; y < map.Bounds.yMax; y++)
            {
                Tile tile = GetTileByNeighbours(x, y);
                if (tile == null)
                    continue;

                map.CollisionMap[colX, colY] = 1;
                walls.SetTile(new Vector3Int(x, y, 0), tile);
                colY++;
            }
            colX++;
        }
    }

    private void PostProcessTiles(in Map map, in MapGeneratorParameters parameters)
    {
        for (int x = map.Bounds.xMin; x < map.Bounds.xMax; x++)
        {
            for (int y = map.Bounds.yMin; y < map.Bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                Tile currentTile = (Tile)walls.GetTile(pos);

                if (currentTile == null)
                    continue;

                Tile result = null;               
                Tile middleRight = (Tile)walls.GetTile(pos + new Vector3Int(1, 0, 0));
                Tile middleLeft = (Tile)walls.GetTile(pos - new Vector3Int(1, 0, 0));
                Tile middleTop = (Tile)walls.GetTile(pos + new Vector3Int(0, 1, 0));
                Tile middleBottom = (Tile)walls.GetTile(pos - new Vector3Int(0, 1, 0));


                if (currentTile == tileContainer.MiddleRight && middleRight == tileContainer.MiddleLeft && middleTop == null)
                    result = tileContainer.TopLeftOuter;

                if (currentTile == tileContainer.MiddleRight && middleRight == tileContainer.MiddleLeft && middleBottom == null)
                    result = tileContainer.BottomRightOuter;

                if (currentTile == tileContainer.MiddleLeft && middleLeft != tileContainer.MiddleRight && middleTop == null)
                    result = tileContainer.TopRightOuter;

                if (currentTile == tileContainer.MiddleLeft && middleLeft == tileContainer.BottomRightOuter && middleBottom == null)
                    result = tileContainer.BottomLeftOuter;

                if (currentTile == tileContainer.TopMiddle && middleTop == tileContainer.BottomMiddle && middleRight == null)
                    result = tileContainer.BottomLeftOuter;

                if (currentTile == tileContainer.BottomMiddle && middleBottom == tileContainer.BottomLeftOuter && middleRight == null)
                    result = tileContainer.TopRightOuter;

                if (currentTile == tileContainer.TopMiddle && middleTop == tileContainer.BottomMiddle && middleLeft == null)
                    result = tileContainer.BottomRightOuter;

                if (currentTile == tileContainer.BottomMiddle && middleBottom == tileContainer.BottomRightOuter && middleLeft == null)
                    result = tileContainer.TopLeftOuter;

                if (result != null)
                    walls.SetTile(new Vector3Int(x, y, 0), result);
            }
        }
    }

    private void RemoveDeadRooms(ref Map map, in MapGeneratorParameters parameters)
    {
        map.Cells.RemoveAll(x => x.Type == MapNodeType.Default || x.Type == MapNodeType.None);
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

    private Tile GetTileByNeighbours(int x, int y)
    {
        TileBase currentFloorTile = floors.GetTile(new Vector3Int(x, y, 0));
        Tile currentWallTile = (Tile)walls.GetTile(new Vector3Int(x, y, 0));

        if (currentFloorTile == null)
            return null;

        if (currentWallTile != null)
            return currentWallTile;

        Tile bottomLeft = floors.GetTile<Tile>(new Vector3Int(x - 1, y - 1, 0));
        Tile bottomMiddle = floors.GetTile<Tile>(new Vector3Int(x, y - 1, 0));
        Tile bottomRight = floors.GetTile<Tile>(new Vector3Int(x + 1, y - 1, 0));

        Tile middleLeft = floors.GetTile<Tile>(new Vector3Int(x - 1, y, 0));
        Tile middleRight = floors.GetTile<Tile>(new Vector3Int(x + 1, y, 0));

        Tile topLeft = floors.GetTile<Tile>(new Vector3Int(x - 1, y + 1, 0));
        Tile topMiddle = floors.GetTile<Tile>(new Vector3Int(x, y + 1, 0));
        Tile topRight = floors.GetTile<Tile>(new Vector3Int(x + 1, y + 1, 0));

        Tile wallMiddleLeft = walls.GetTile<Tile>(new Vector3Int(x - 1, y, 0));
        Tile wallMiddleRight = walls.GetTile<Tile>(new Vector3Int(x + 1, y, 0));
        Tile wallTopMiddle = walls.GetTile<Tile>(new Vector3Int(x, y + 1, 0));

        //left
        if (middleLeft == null && topMiddle == null)
            return tileContainer.TopLeft;
        if (middleLeft == null && topMiddle != null && bottomMiddle != null)
            return tileContainer.MiddleLeft;
        if (middleLeft == null && bottomMiddle == null && topMiddle != null)
            return tileContainer.BottomLeft;

        //middle
        if (middleLeft != null && topMiddle == null && middleRight != null)
            return tileContainer.TopMiddle;
        if (middleLeft != null && bottomMiddle == null && middleRight != null)
            return tileContainer.BottomMiddle;

        // right
        if (middleLeft != null && topMiddle == null && middleRight == null)
            return tileContainer.TopRight;
        if (topMiddle != null && bottomMiddle != null && middleRight == null)
            return tileContainer.MiddleRight;
        if (topMiddle != null && bottomMiddle == null && middleRight == null)
            return tileContainer.BottomRight;

        if (bottomMiddle != null && bottomLeft == null && wallMiddleLeft != null && wallMiddleRight == null)
            return tileContainer.TopRightOuter;
        if (bottomMiddle != null && bottomRight == null && wallTopMiddle == null)
            return tileContainer.TopLeftOuter;
        if (topRight == null && wallMiddleLeft == null)
            return tileContainer.BottomRightOuter;
        if (topLeft == null && topMiddle != null && wallMiddleLeft != null)
            return tileContainer.BottomLeftOuter;

        return null;
    }
}
