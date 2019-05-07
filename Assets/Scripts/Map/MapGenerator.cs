using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public Tilemap Floor;
    public Tilemap Walls;
    public TileContainer TileContainer;

    private MillerParkLCG _random;
    private int _seed;
    private Map _currentMap;
    private void Awake()
    {
        _random = new MillerParkLCG();
        _seed = 1;
    }

    private void Update()
    {
        MapGeneratorParameters parameters = new MapGeneratorParameters();
        parameters.GenerationRadius = 100;

        parameters.MinCellSize = 3;
        parameters.MaxCellSize = 30;

        parameters.MinCellCount = 100;
        parameters.MaxCellCount = 150;

        parameters.MinRoomWidth = 7;
        parameters.MinRoomHeight = 7;

        parameters.MinCorridorWidth = 3;
        parameters.MaxCorridorWidth = 5;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            _seed++;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Floor.ClearAllTiles();

            _currentMap = GenerateMap(_seed, parameters);
            SeparateCells(ref _currentMap, parameters);
            IdentifyRooms(ref _currentMap, parameters);
            Triangulate(ref _currentMap, parameters);
            GenerateLayoutGraph(ref _currentMap, parameters);
            GenerateCorridorGraph(ref _currentMap, parameters);
            PaintRoomFloors(_currentMap, parameters);
            PaintCorridors(ref _currentMap, parameters);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Floor.ClearAllTiles();
            Walls.ClearAllTiles();

            _currentMap = GenerateMap(DateTime.Now.Ticks, parameters);
            SeparateCells(ref _currentMap, parameters);
            IdentifyRooms(ref _currentMap, parameters);
            Triangulate(ref _currentMap, parameters);
            GenerateLayoutGraph(ref _currentMap, parameters);
            GenerateCorridorGraph(ref _currentMap, parameters);
            PaintRoomFloors(_currentMap, parameters);
            PaintCorridors(ref _currentMap, parameters);
        }
    }

    private void OnDrawGizmos()
    {
        if (_currentMap != null)
        {
            _currentMap.DrawDebug();
        }
    }

    public Map GenerateMap(long seed, in MapGeneratorParameters parameters)
    {
        Map result = new Map();

        _random.SetSeed(seed);
        GenerateCells(ref result, parameters);
        //SeparateCells(ref nodes, parameters);
        //IdentifyRooms(ref nodes, parameters);
        //Triangulate(ref nodes);
        //AddCorridorEdges(ref result);

        return result;
    }

    private void GenerateCells(ref Map map, in MapGeneratorParameters parameters)
    {
        int cellCount = _random.Range(parameters.MinCellCount, parameters.MaxCellCount);
        map._cells = new List<MapNode>(cellCount);

        for (int i = 0; i < cellCount; i++)
        {
            Vector2Int size = GenerateRandomSize(parameters);
            Vector2Int position = RandomPointInCircle(parameters.GenerationRadius);

            map._cells.Add(new MapNode(i, position, size));
            map._cells = map._cells.OrderBy(x => x.Cell.x).ThenBy(x => x.Cell.y).ToList();
        }
    }

    private void SeparateCells(ref Map map, in MapGeneratorParameters parameters)
    {
        bool regionsSeparated = false;
        int iterations = 0;

        while (!regionsSeparated && iterations < 2 * map._cells.Count)
        {
            regionsSeparated = true;
            foreach (MapNode node in map._cells)
            {
                Vector2 movement = Vector2.zero;
                int separationCount = 0;
                foreach (MapNode other in map._cells)
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
                }
            }
            iterations++;
        }

        if (!regionsSeparated)
        {
            //We should iterate over nodes overlapping and discard them
            Debug.Log("Unable to separate all nodes");
        }
    }

    private void IdentifyRooms(ref Map map, in MapGeneratorParameters parameters)
    {
        foreach (MapNode node in map._cells)
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
        IEnumerable<Delaunay.Vertex<MapNode>> vertices = map._cells.Where(x => x.Type == MapNodeType.Room)
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

    private void PaintRoomFloors(in Map map, in MapGeneratorParameters parameters)
    {
        foreach (MapNode node in map._cells)
        {
            if (node.Type != MapNodeType.Room)
                continue;

            Vector3Int pos = new Vector3Int(node.Cell.xMin, node.Cell.yMin, 0);
            Vector3Int size = new Vector3Int(node.Cell.width, node.Cell.height, 1);

            TileBase[] tiles = new TileBase[size.x * size.y];
            for (int i = 0; i < size.x * size.y; i++)
            {
                tiles[i] = TileContainer.FloorTiles[0];
            }

            Floor.SetTilesBlock(new BoundsInt(pos, size), tiles);

            for (int i = 0; i < size.x * size.y; i++)
            {
                tiles[i] = null;

                int x = i % size.x + pos.x;
                int y = i / size.x + pos.y;

                if (x == pos.x && y == pos.y)
                    tiles[i] = TileContainer.BottomLeft;
                else if (x == pos.x && y == node.Cell.yMax - 1)
                    tiles[i] = TileContainer.TopLeft;
                else if (x == node.Cell.xMax - 1 && y == pos.y)
                    tiles[i] = TileContainer.BottomRight;
                else if (x == node.Cell.xMax - 1 && y == node.Cell.yMax - 1)
                    tiles[i] = TileContainer.TopRight;
                else if (y == node.Cell.yMax - 1)
                    tiles[i] = TileContainer.TopMiddle;
                else if (y == pos.y)
                    tiles[i] = TileContainer.BottomMiddle;
                else if (x == node.Cell.xMax - 1)
                    tiles[i] = TileContainer.MiddleRight;
                else if (x == pos.x)
                    tiles[i] = TileContainer.MiddleLeft;
            }

            Walls.SetTilesBlock(new BoundsInt(pos, size), tiles);
        }
    }

    private void GenerateCorridorGraph(ref Map map, in MapGeneratorParameters parameters)
    {
        List<Delaunay.Edge<MapNode>> result = new List<Delaunay.Edge<MapNode>>();
        foreach (Delaunay.Edge<MapNode> edge in map.EMSTGraph)
        {
            MapNode a = null;
            MapNode b = null;

            foreach (MapNode node in map._cells)
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
                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(midpoint.x, aCenter.y),
                     new Delaunay.Vertex<MapNode>(midpoint.x, bCenter.y)));
                result.Add(new Delaunay.Edge<MapNode>(new Delaunay.Vertex<MapNode>(midpoint.x, bCenter.y),
                    new Delaunay.Vertex<MapNode>(bCenter.x, bCenter.y)));
            }
            else if (useY)
            {
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
                tiles[tileIndex] = TileContainer.FloorTiles[0];
            }

            BoundsInt bounds = new BoundsInt(pos, size);
            Floor.SetTilesBlock(bounds, tiles);
            Walls.SetTilesBlock(bounds, new TileBase[size.x * size.y]);
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
