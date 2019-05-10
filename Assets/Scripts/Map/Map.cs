using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map
{
    public List<MapNode> Cells { get; set; }
    public List<Delaunay.Triangle<MapNode>> Triangles { get; set; }
    public List<Delaunay.Edge<MapNode>> DelaunayGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> GabrielGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> EMSTGraph { get; set; }
    public List<Delaunay.Edge<MapNode>> CorridorGraph { get; set; }
    public int[,] CollisionMap { get; set; }
    public BoundsInt Bounds { get; set; }

    public List<GameObject> InteractiveObjects { get; }
    public List<GameObject> Enemies { get; }

    private readonly bool _drawCells;
    private readonly bool _drawDelaunay;
    private readonly bool _drawGabriel;
    private readonly bool _drawEMST;
    private readonly bool _drawCorridors;
    private readonly bool _drawBounds;

    private readonly Tilemap _floors;
    private readonly Tilemap _walls;
    private readonly LCG _random;

    public Map(Tilemap floors, Tilemap walls, LCG random)
    {
        _drawCells = true;
        _drawDelaunay = false;
        _drawGabriel = false;
        _drawEMST = false;
        _drawCorridors = false;
        _drawBounds = false;

        _floors = floors;
        _walls = walls;
        _random = random;

        InteractiveObjects = new List<GameObject>();
        Enemies = new List<GameObject>();
    }

    public void AddInteractiveObject(GameObject interactiveObject)
    {
        InteractiveObjects.Add(interactiveObject);
    }

    public void AddEnemy(GameObject enemy)
    {
        Enemies.Add(enemy);
    }

    public void ClearMap()
    {
        _floors.ClearAllTiles();
        _walls.ClearAllTiles();

        DestroyAllInteractiveObjects();
    }

    public void ActivateObjects()
    {
        Enemies.ForEach(x => x.SetActive(true));
    }

    public List<Enemy> GetEnemiesInCircle(Vector2 position, float radius)
    {
        List<Enemy> closeEnemies = new List<Enemy>();
        foreach (GameObject enemy in Enemies)
        {
            if (enemy == null || enemy.Equals(null))
            {
                continue;
            }

            if (Vector2.Distance(enemy.transform.position, position) <= radius)
            {
                closeEnemies.Add(enemy.GetComponent<Enemy>());
            }
        }
        return closeEnemies;
    }

    public Vector2Int GetPositionInMap(int widthInTiles, int heightInTiles, bool includeCorridorRooms, out MapNode room,
        List<MapNode> excludedRooms = null)
    {
        room = GetRandomRoom(widthInTiles, heightInTiles, includeCorridorRooms, excludedRooms);
        return GetRandomPositionInRoom(widthInTiles, heightInTiles, room);
    }

    public Vector2Int GetPositionInMap(int widthInTiles, int heightInTiles, bool includeCorridorRooms, 
        List<MapNode> excludedRooms = null)
    {
        return GetRandomPositionInRoom(widthInTiles, heightInTiles, 
            GetRandomRoom(widthInTiles, heightInTiles, includeCorridorRooms, excludedRooms));
    }

    public Tuple<MapNode, MapNode> GetRoomsFurthestApart()
    {
        List<MapNode> edgeNodes = new List<MapNode>();

        int edgeCount = 1;
        while(edgeNodes.Count < 2)
        {
            edgeNodes.AddRange(Cells.Where(x => x.Corridors.Count == edgeCount));
            edgeCount++;

            //exit condition
            if (edgeCount > 10)
            {
                return null;
            }
        }

        if (edgeNodes.Count == 2)
        {
            return new Tuple<MapNode, MapNode>(edgeNodes[0], edgeNodes[1]);
        }

        Tuple<MapNode, MapNode> result = null;
        float maxDistance = float.MinValue;

        for(int i = 0; i < edgeNodes.Count; i++)
        {
            for (int j = 0; j < edgeNodes.Count; j++)
            {
                if (edgeNodes[i].Equals(edgeNodes[j]))
                {
                    continue;
                }

                NavigationManager.Instance.AStar(edgeNodes[i], edgeNodes[j], out float distance);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    result = new Tuple<MapNode, MapNode>(edgeNodes[i], edgeNodes[j]);
                }
            }
        }

        return result;
    }

    public MapNode GetRandomRoom(int tileWidth, int tileHeight, bool includeCorridorRooms, List<MapNode> excludedRooms = null)
    {
        MapNode room = null;
        while (room == null)
        {
            MapNode potential = Cells[_random.Range(0, Cells.Count)];

            if (!includeCorridorRooms && potential.Type == MapNodeType.Corridor)
            {
                continue;
            }

            if (excludedRooms != null && excludedRooms.Contains(potential))
            {
                continue;
            }

            int floorWidth = potential.Cell.width - 2;
            int floorHeight = potential.Cell.height - 2;

            if (floorWidth < tileWidth || floorHeight < tileHeight)
            {
                continue;
            }

            room = potential;
        }

        return room;
    }

    public Vector2Int GetRandomPositionInRoom(int widthInTiles, int heightInTiles, MapNode room)
    {
        int halfWidth = (int)Mathf.Ceil(widthInTiles / 2.0f) + 1;
        int halfHeight = (int)Mathf.Ceil(heightInTiles / 2.0f + 1);

        int x = _random.Range(room.Cell.xMin + halfWidth, room.Cell.xMax - halfWidth);
        int y = _random.Range(room.Cell.yMin + halfHeight, room.Cell.yMax - halfHeight);

        return new Vector2Int(x, y);
    }

    public void DrawDebug()
    {
        DrawCells();
    }

    private void DestroyAllInteractiveObjects()
    {
        InteractiveObjects.ForEach(x =>
        {
            GameObject.Destroy(x);
        });
        InteractiveObjects.Clear();

        Enemies.ForEach(x =>
        {
            GameObject.Destroy(x);
        });
        Enemies.Clear();
    }

    private void DrawCells()
    {
        if (_drawBounds && Bounds != null)
        {
            GizmoUtility.DrawRectangle(Bounds.ToRectInt(), Color.cyan);
        }

        if (_drawCells && Cells != null)
        {
            Cells.ForEach(x =>
            {
                switch (x.Type)
                {
                    case MapNodeType.Default:
                        {
                            GizmoUtility.DrawRectangle(x.Cell, Color.black);
                        }
                        break;
                    case MapNodeType.Room:
                        {
                            GizmoUtility.DrawRectangle(x.Cell, Color.green);
                        }
                        break;
                    case MapNodeType.Corridor:
                        {
                            GizmoUtility.DrawRectangle(x.Cell, Color.blue);
                        }
                        break;
                    default:
                        break;
                }
            });
        }   

        if (_drawDelaunay && DelaunayGraph != null)
        {
            DelaunayGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.cyan);
            });
        }

        if (_drawGabriel && GabrielGraph != null)
        {
            GabrielGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.magenta);
            });
        }

        if (_drawEMST && EMSTGraph != null)
        {
            EMSTGraph.ForEach(x =>
            {
                GizmoUtility.DrawLine(x, Color.cyan);
            });
        }

        if (_drawCorridors && CorridorGraph != null)
        {
            CorridorGraph.ForEach(x =>
            {
               GizmoUtility.DrawLine(x, Color.red);
            });
        }
    }

   
}
