using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

    private bool _drawCells;
    private bool _drawDelaunay;
    private bool _drawGabriel;
    private bool _drawEMST;
    private bool _drawCorridors;

    private Tilemap _floors;
    private Tilemap _walls;
    private LCG _random;

    public Map(Tilemap floors, Tilemap walls, LCG random)
    {
        _drawCells = true;
        _drawDelaunay = false;
        _drawGabriel = false;
        _drawEMST = true;
        _drawCorridors = true;

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
        if (Bounds != null)
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
