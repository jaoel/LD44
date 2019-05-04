using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private MillerParkLCG _random;

    private Map _currentMap;
    private void Awake()
    {
        _random = new MillerParkLCG();
    }

    private void Update()
    {
        MapGeneratorParameters parameters = new MapGeneratorParameters();
        parameters.GenerationRadius = 25;

        parameters.MinCellSize = 3;
        parameters.MaxCellSize = 10;

        parameters.MinCellCount = 4;
        parameters.MaxCellCount = 4;

        parameters.MinRoomWidth = 0;
        parameters.MinRoomHeight = 0;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _currentMap = GenerateMap(2, parameters);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SeparateCells(ref _currentMap._cells, parameters);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            IdentifyRooms(ref _currentMap._cells, parameters);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Triangulate(ref _currentMap._cells);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _currentMap = GenerateMap(3, parameters);
            SeparateCells(ref _currentMap._cells, parameters);
            IdentifyRooms(ref _currentMap._cells, parameters);

            Triangulate(ref _currentMap._cells);
        }
    }

    private void OnDrawGizmos()
    {
        if (_currentMap != null)
            _currentMap.DrawDebug();
    }

    public Map GenerateMap(long seed, in MapGeneratorParameters parameters)
    {
        Map result = new Map();

        _random.SetSeed(seed);

        List<MapNode> nodes = new List<MapNode>();         

        GenerateCells(ref nodes, parameters);
        //SeparateCells(ref nodes, parameters);
        //IdentifyRooms(ref nodes, parameters);
        //Triangulate(ref nodes);

        result._cells = nodes;

        return result;
    }

    private void GenerateCells(ref List<MapNode> nodes, in MapGeneratorParameters parameters)
    {
        int cellCount = _random.Range(parameters.MinCellCount, parameters.MaxCellCount);

        for (int i = 0; i < cellCount; i++)
        {
            Vector2Int size = GenerateRandomSize(parameters);
            Vector2Int position = RandomPointInCircle(parameters.GenerationRadius); 

            nodes.Add(new MapNode(i, position, size));

            nodes = nodes.OrderBy(x => x.Cell.x).ThenBy(x => x.Cell.y).ToList();   
        }
    }

    private void SeparateCells(ref List<MapNode> cells, in MapGeneratorParameters parameters)
    {
        bool regionsSeparated = false;
        int iterations = 0;

        while(!regionsSeparated && iterations < 2 * cells.Count)
        {
            regionsSeparated = true;
            foreach (MapNode node in cells)
            {
                Vector2 movement = Vector2.zero;
                int separationCount = 0;
                foreach (MapNode other in cells)
                {
                    if (node == other)
                        continue;

                    if (!node.Cell.Overlaps(other.Cell))
                        continue;

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

    private void IdentifyRooms(ref List<MapNode> nodes, in MapGeneratorParameters parameters)
    {
        foreach (MapNode node in nodes)
        {
            if (node.Cell.width < parameters.MinRoomWidth || node.Cell.height < parameters.MinRoomHeight)
                node.Type = MapNodeType.Corridor;
            else
                node.Type = MapNodeType.Room;
        } 
    }

    private void Triangulate(ref List<MapNode> nodes)
    {
        _currentMap._triangulation = DelaunayTriangulation.Triangulate(nodes.Where(x => x.Type == MapNodeType.Room).ToList());
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
