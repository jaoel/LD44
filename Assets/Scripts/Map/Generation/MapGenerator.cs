﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public TileContainer wallContainer;
    public TileContainer pitContainer;
    public InteractiveDungeonObject interactiveObjectContainer;
    public TrapContainer trapContainer;
    public SpawnableContainer spawnKeyframes;

    public Tilemap floors;
    public Tilemap walls;
    public Tilemap pits;

    private MillerParkLCG _random;
    private Timer _timer;

    private Map _debugMap;
    private MapPainter _mapPainter;
    private MapPopulator _mapPopulator;

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

            return _instance;
        }
    }

    private void Awake()
    {

    }

    public void Initialize()
    {
        floors = GameObject.Find("Floor")?.GetComponent<Tilemap>();
        walls = GameObject.Find("Walls")?.GetComponent<Tilemap>();
        pits = GameObject.Find("Pits")?.GetComponent<Tilemap>();

        _random = new MillerParkLCG();
        _timer = new Timer();
        _mapPainter = new MapPainter();
        _mapPainter.Initialize(wallContainer, pitContainer, floors, walls, pits);

        _mapPopulator = new MapPopulator();
        _mapPopulator.Initialize(_random, interactiveObjectContainer, spawnKeyframes, trapContainer, _mapPainter);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.O))
        {
            Debug.Log("Seed " + GetCurrentSeed() + " copied to clipboard");
            GUIUtility.systemCopyBuffer = GetCurrentSeed().ToString();
        }
    }

    public long GetCurrentSeed()
    {
        return _random.GetStartingSeed();
    }

    public Map GenerateMap(long seed, in MapGeneratorParameters parameters, int level)
    {
        Debug.Log("Generating new map");
        Debug.Log(seed);

        _timer.Start();
        Map result = new Map(floors, walls, pits, _random);

        _random.SetSeed(seed);
                                                                                                                                                                                                             
        GenerateCells(ref result, parameters);
        SeparateCells(ref result, parameters);

        if (IdentifyRooms(ref result, parameters) < 3)
        {
            seed += 1;
            return GenerateMap(seed, parameters, level);
        }

        Triangulate(ref result, parameters);
        GenerateLayoutGraph(ref result, parameters);
        GenerateCorridorGraph(ref result, parameters);
        PaintRooms(result, parameters);
        PaintCorridors(ref result, parameters);
        _mapPainter.PaintTiles(result, parameters);
        RemoveDeadRooms(ref result, parameters);

        if(FindChokepoints(ref result, parameters) < 2)
        {
            seed += 1;
            return GenerateMap(seed, parameters, level);
        }

        GeneratePools(ref result, parameters);

        _timer.Stop();
        _timer.Print("MapGenerator.GenerateMap");
        _debugMap = result;
        return result;
    }

    public void PopulateMap(ref Map map, ref Player player, in MapGeneratorParameters generationParameters, int level)
    {
        _mapPopulator.PopulateMap(ref map, ref player, generationParameters, level);
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

        while (!regionsSeparated && iterations < 2 * map.Cells.Count)
        {
            regionsSeparated = true;
            for (int i = 0; i < map.Cells.Count; i++)
            {
                Vector2 movement = Vector2.zero;
                int separationCount = 0;
                for (int j = 0; j < map.Cells.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    RectInt overSizeNode = map.Cells[i].Cell;

                    if (parameters.MinRoomDistance > 0)
                    {
                        overSizeNode.position -= new Vector2Int(parameters.MinRoomDistance, parameters.MinRoomDistance);
                        overSizeNode.width += parameters.MinRoomDistance * 2;
                        overSizeNode.height += parameters.MinRoomDistance * 2;
                    }

                    if (!overSizeNode.Intersects(map.Cells[j].Cell, out RectInt area))
                    {
                        continue;
                    }

                    movement += map.Cells[j].Cell.center - overSizeNode.center;
                    separationCount++;
                }

                if (separationCount > 0)
                {
                    movement *= -1.0f;
                    movement = movement.normalized;
                    RectInt newRect = map.Cells[i].Cell;
                    newRect.position += new Vector2Int(Mathf.RoundToInt(movement.x), Mathf.RoundToInt(movement.y)) * (1 + parameters.MinRoomDistance);

                    if (newRect.position != map.Cells[i].Cell.position || movement.magnitude > 0.0f)
                    {
                        map.Cells[i].Cell = newRect;
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
            for(int i = 0; i < map.Cells.Count; i++)
            {
                if (map.Cells[i].Type == MapNodeType.None)
                {
                    continue;
                }

                for(int j = 0; j < map.Cells.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    if (map.Cells[j].Type == MapNodeType.None)
                    {
                        continue;
                    }

                    if (map.Cells[i].Cell.Overlaps(map.Cells[j].Cell))
                    {
                        map.Cells[j].Type = MapNodeType.None;
                    }
                }
            }
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
            //These are already discarded
            if (node.Type == MapNodeType.None)
            {
                continue;
            }

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
            .Select(x => new Delaunay.Vertex<MapNode>(x.Cell.center, x)).Distinct();
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

        foreach (Delaunay.Edge<MapNode> edge in map.GabrielGraph)
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

        AddCorridorsToNodes(map);
    }

    private static void AddCorridorsToNodes(Map map)
    {
        foreach (MapNode mapNode in map.Cells)
        {
            if (mapNode.Type == MapNodeType.Room)
            {
                mapNode.Corridors.Clear();
                List<Delaunay.Edge<MapNode>> edges = map.LayoutGraph.Where(y => y.ContainsVertex(new Delaunay.Vertex<MapNode>(mapNode.Cell.center, mapNode))).ToList();
                mapNode.Corridors.AddRange(edges);
            }
        }
    }

    private void PaintRooms(in Map map, in MapGeneratorParameters parameters)
    {
        foreach (MapNode node in map.Cells)
        {
            if (node.Type != MapNodeType.Room)
            {
                continue;
            }

            _mapPainter.PaintRoom(node.Cell, true);
        }
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
                if (area.width > 2 && area.height > 2)
                {
                    room.Type = MapNodeType.Corridor;
                    _mapPainter.PaintRoom(room.Cell, _random.NextFloat() < parameters.CorridorRoomConnectionFactor);
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
                tiles[tileIndex] = wallContainer.FloorTiles[0];
                wallTiles[tileIndex] = null;
            }

            BoundsInt bounds = new BoundsInt(pos, size);
            AddCorridorRooms(ref map, parameters, bounds);
            floors.SetTilesBlock(bounds, tiles);
            walls.SetTilesBlock(bounds, wallTiles);
        }
    }
   
    private void RemoveDeadRooms(ref Map map, in MapGeneratorParameters parameters)
    {
        map.Cells.RemoveAll(x => x.Type == MapNodeType.Default || x.Type == MapNodeType.None);
    }

    private int FindChokepoints(ref Map map, in MapGeneratorParameters parameters)
    {
        int lockableRoomCount = 0;
        foreach (MapNode room in map.Cells)
        {
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
            if (room.Lockable)
            {
                lockableRoomCount++;
            }
        }

        return lockableRoomCount;
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

    private void GeneratePools(ref Map map, in MapGeneratorParameters parameters)
    {
        pits.SetTile(walls.cellBounds.min, null);
        pits.SetTile(walls.cellBounds.max - new Vector3Int(1, 1, 1), null);

        List<BoundsInt> chokepoints = new List<BoundsInt>(map.ChokePoints);

        for (int i = 0; i < map.ChokePoints.Count; i++)
        {
            chokepoints[i] = new BoundsInt(map.ChokePoints[i].position - new Vector3Int(1, 1, 0), map.ChokePoints[i].size + new Vector3Int(2, 2, 0));
        }

        for(int i = 0; i < map.Cells.Count; i++)
        {
            if (map.Cells[i].Cell.width < 6 || map.Cells[i].Cell.height < 6)
            {
                continue;
            }

            //roll if pool should be generated
            if (_random.Range(0.0f, 1.0f) < parameters.PitFrequency)
            {
                continue;
            }

            //get position of initial rectangle
            Vector2 originPos = map.Cells[i].Cell.min + new Vector2Int(_random.Range(2, map.Cells[i].Cell.width - 4), _random.Range(2, map.Cells[i].Cell.height - 4));

            //save rectangle to list
            List<BoundsInt> rectangles = new List<BoundsInt>();
            rectangles.Add(new BoundsInt(originPos.ToVector3().ToVector3Int(), new Vector3Int(2, 2, 1)));

            int left = 0;
            int down = 1;
            int up = 2;
            int right = 3;

            List<Tuple<Vector2Int, int>> edges = new List<Tuple<Vector2Int, int>>();
            if (rectangles[rectangles.Count - 1].xMin - 3 > map.Cells[i].Cell.xMin)
            {
                edges.Add(new Tuple<Vector2Int, int>(new Vector2Int(rectangles[rectangles.Count - 1].xMin, rectangles[rectangles.Count - 1].yMin), left));
            }

            if (rectangles[rectangles.Count - 1].yMin - 3 > map.Cells[i].Cell.yMin)
            {
                edges.Add(new Tuple<Vector2Int, int>(new Vector2Int(rectangles[rectangles.Count - 1].xMin, rectangles[rectangles.Count - 1].yMin), down));
            }

            if (rectangles[rectangles.Count - 1].xMax + 3 < map.Cells[i].Cell.xMax)
            {
                edges.Add(new Tuple<Vector2Int, int>(new Vector2Int(rectangles[rectangles.Count - 1].xMax, rectangles[rectangles.Count - 1].yMin), right));
            }

            if (rectangles[rectangles.Count - 1].yMax + 3 > map.Cells[i].Cell.yMax)
            {
                edges.Add(new Tuple<Vector2Int, int>(new Vector2Int(rectangles[rectangles.Count - 1].xMin, rectangles[rectangles.Count - 1].yMax), up));
            }

            float sizeFactor = map.Cells[i].Type == MapNodeType.Room ? 0.95f : 0.5f;

            int minSize = 4;
            while (_random.Range(0.0f, 1.0f) < sizeFactor || rectangles.Count < minSize)
            {
                if (edges.Count == 0)
                {
                    break;
                }

                Tuple<Vector2Int, int> edge = edges[_random.Range(0, edges.Count)];
                BoundsInt newRect = new BoundsInt();
                if (edge.Item2 == left)
                {
                    newRect = new BoundsInt(edge.Item1.x - 2, edge.Item1.y, 0, 2, 2, 1);
                }
                else if (edge.Item2 == down)
                {
                    newRect = new BoundsInt(edge.Item1.x, edge.Item1.y - 2, 0, 2, 2, 1);
                }
                else if(edge.Item2 == up)
                {
                    newRect = new BoundsInt(edge.Item1.x, edge.Item1.y, 0, 2, 2, 1);
                }
                else if(edge.Item2 == right)
                {
                    newRect = new BoundsInt(edge.Item1.x, edge.Item1.y, 0, 2, 2, 1);
                }
              
                edges.Remove(edge);

                if (rectangles.Any(x => x.Overlaps(newRect)) || chokepoints.Any(x => x.Overlaps(newRect)))
                {
                    continue;
                }

                if (walls.GetTilesBlock(newRect).Any(x => x != null))
                {
                    continue;
                }

                rectangles.Add(newRect);

                if (rectangles[rectangles.Count - 1].xMin - 3 > map.Cells[i].Cell.xMin)
                {
                    edges.Add(new Tuple<Vector2Int, int>(new Vector2Int(rectangles[rectangles.Count - 1].xMin, rectangles[rectangles.Count - 1].yMin), left));
                }

                if (rectangles[rectangles.Count - 1].yMin - 3 > map.Cells[i].Cell.yMin)
                {
                    edges.Add(new Tuple<Vector2Int, int>(new Vector2Int(rectangles[rectangles.Count - 1].xMin, rectangles[rectangles.Count - 1].yMin), down));
                }

                if (rectangles[rectangles.Count - 1].xMax + 3 < map.Cells[i].Cell.xMax)
                {
                    edges.Add(new Tuple<Vector2Int, int>(new Vector2Int(rectangles[rectangles.Count - 1].xMax, rectangles[rectangles.Count - 1].yMin), right));
                }

                if (rectangles[rectangles.Count - 1].yMax + 3 > map.Cells[i].Cell.yMax)
                {
                    edges.Add(new Tuple<Vector2Int, int>(new Vector2Int(rectangles[rectangles.Count - 1].xMin, rectangles[rectangles.Count - 1].yMax), up));
                }
            }

            foreach(BoundsInt bounds in rectangles)
            {
                _mapPainter.PaintPit(bounds.ToRectInt(), false);
                map.UpdateCollisionMap(bounds.ToRectInt(), 2, false);
            }
        }
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
}
