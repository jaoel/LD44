﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapPopulator : MonoBehaviour
{
    private LCG _random;
    private InteractiveDungeonObject _interactiveObjectContainer;
    private SpawnableContainer _spawnKeyframes;
    private TrapContainer _trapContainer;

    private MapPainter _mapPainter;

    public void Initialize(LCG random, InteractiveDungeonObject interactiveObjectContainer, SpawnableContainer spawnKeyframes,
        TrapContainer trapContainer, MapPainter mapPainter)
    {
        _random = random;
        _interactiveObjectContainer = interactiveObjectContainer;
        _spawnKeyframes = spawnKeyframes;
        _trapContainer = trapContainer;
        _mapPainter = mapPainter;
    }

    public void PopulateMap(ref Map map, ref Player player, in MapGeneratorParameters generationParameters, int level)
    {
        Tuple<MapNode, MapNode> startAndGoal = map.GetRoomsFurthestApart(true, out List<MapNode> path);

        CalculateSeclusionFactor(map, startAndGoal, path);

        if (_random.NextFloat() < 0.5f)
        {
            startAndGoal = new Tuple<MapNode, MapNode>(startAndGoal.Item2, startAndGoal.Item1);
        }

        player.transform.position = map.GetRandomPositionInRoom(1, 1, startAndGoal.Item1).ToVector3();
        CameraManager.Instance.SetCameraPosition(player.transform.position);

        map.AddInteractiveObject(Instantiate(_interactiveObjectContainer.Stairs,
            map.GetRandomPositionInRoom(2, 2, startAndGoal.Item2).ToVector3(), Quaternion.identity));
        startAndGoal.Item2.ContainsStairs = true;

        GenerateDoors(ref map, startAndGoal.Item1, startAndGoal.Item2, generationParameters);
        _mapPainter.PostProcessTiles(map, generationParameters);
        SpawnSpawnables(map, level, startAndGoal);
        PlaceTraps(map, startAndGoal.Item1, player);
    }

    private void CalculateSeclusionFactor(Map map, Tuple<MapNode, MapNode> startAndGoal, List<MapNode> path)
    {
        path.Add(startAndGoal.Item1);
        for (int i = 0; i < path.Count - 2; i++)
        {
            LineSegment2D connection = new LineSegment2D(path[i].Cell.center, path[i + 1].Cell.center);
            map.StartToGoalPath.Add(connection);
        }

        float[] distances = new float[map.Cells.Count];
        int index = 0;
        foreach (MapNode room in map.Cells)
        {
            distances[index] = float.PositiveInfinity;
            foreach (LineSegment2D edge in map.StartToGoalPath)
            {
                float dist = edge.DistanceToPoint(room.Cell.center);
                if (dist < distances[index])
                {
                    distances[index] = dist;
                }
            }

            index++;
        }

        float maxDist = distances.Max();
        float minDist = distances.Min();

        index = 0;
        foreach (float dist in distances)
        {
            if (maxDist == minDist)
            {
                map.Cells[index].SeclusionFactor = minDist;
            }
            else
            {
                map.Cells[index].SeclusionFactor = Utility.ConvertRange(minDist, maxDist, 0.0f, 1.0f, dist);
            }
            index++;
        }

        map.UpdateStatisticsMapDebug();
    }

    private void GenerateDoors(ref Map map, MapNode spawnRoom, in MapNode exitRoom, in MapGeneratorParameters parameters)
    {
        List<MapNode> rooms = new List<MapNode>(map.Cells.Where(x => x.Type == MapNodeType.Room));

        LockRoom(map, spawnRoom, exitRoom, rooms, out MapNode keyRoom, true);
        int lockedDoorCount = (int)Mathf.Round(rooms.Count * parameters.LockFactor);

        for (int i = 0; i < lockedDoorCount; i++)
        {
            MapNode room = rooms[_random.Range(0, rooms.Count)];

            if (!room.Lockable || room == spawnRoom || room == exitRoom)
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

        SpawnSkeletonKeys(map, spawnRoom);
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
                    door = Instantiate(_interactiveObjectContainer.horizontalDoor, chokepoint.center,
                        Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.xMin + 1, chokepoint.yMin, 1, 1);
                }
                else if (chokepoint.size.x == 4)
                {
                    door = Instantiate(_interactiveObjectContainer.horizontalDoor, chokepoint.center,
                        Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.xMin + 1, chokepoint.yMin, 2, 1);
                }
                else
                {

                    door = Instantiate(_interactiveObjectContainer.horizontalDoor, new Vector3(chokepoint.xMin + 2, chokepoint.center.y),
                        Quaternion.identity).GetComponent<Door>();
                    _mapPainter.BuildHorizontalDoorWalls(map, target, chokepoint);
                    door.Bounds = new RectInt(chokepoint.xMin + 1, chokepoint.yMin, 2, 1);
                }
            }
            else if (chokepoint.size.y > 1)
            {
                if (chokepoint.size.y == 3)
                {
                    door = Instantiate(_interactiveObjectContainer.verticalDoor, chokepoint.center, Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.x, chokepoint.yMin + 1, 1, 1);
                }
                else if (chokepoint.size.y == 4)
                {
                    door = Instantiate(_interactiveObjectContainer.verticalDoor, chokepoint.center, Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.x, chokepoint.yMin + 1, 1, 2);
                }
                else
                {
                    door = Instantiate(_interactiveObjectContainer.verticalDoor, new Vector3(chokepoint.center.x, chokepoint.yMin + 2),
                        Quaternion.identity).GetComponent<Door>();
                    door.Bounds = new RectInt(chokepoint.x, chokepoint.yMin + 1, 1, 2);
                    _mapPainter.BuildVerticalDoorWalls(map, target, chokepoint);
                }
            }

            if (door != null)
            {
                doors.Add(door);
                map.UpdateCollisionMap(chokepoint.ToRectInt(), 1);
                map.UpdateCollisionMap(door.Bounds, 1);
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

            target.Locked = true;

            Delaunay.Vertex<MapNode> lockedRoom = new Delaunay.Vertex<MapNode>(target.Cell.center, target);
            rooms.ForEach(x =>
            {
                int removed = x.Corridors.RemoveAll(corridor => corridor.ContainsVertex(lockedRoom));
            });

            rooms.RemoveAll(x => x.Equals(target));
            keyRoom = FindKeyRoom(spawnRoom, target, rooms);

            Key newKey = null;
            if (goldKey)
            {
                newKey = Instantiate(_interactiveObjectContainer.goldKey, map.GetRandomPositionInRoom(1, 1, keyRoom).ToVector3(),
                    Quaternion.identity).GetComponent<Key>();

                map.AddInteractiveObject(newKey.gameObject);
                keyRoom.Keys.Add(newKey.gameObject);
            }

            doors.ForEach(x =>
            {
                x.IsGoalDoor = goldKey;
            });

            return true;
        }

        return false;
    }
    
    private MapNode FindKeyRoom(MapNode spawnRoom, MapNode target, List<MapNode> rooms)
    {
        List<Tuple<MapNode, float>> distances = new List<Tuple<MapNode, float>>();

        rooms.ForEach(x =>
        {
            float distToSpawn = (x.Cell.center - spawnRoom.Cell.center).sqrMagnitude;
            float distToTarget = (x.Cell.center - target.Cell.center).sqrMagnitude;

            distances.Add(new Tuple<MapNode, float>(x, Mathf.Min(distToSpawn, distToTarget)));
        });

        distances = distances.OrderBy(x => x.Item1.Keys.Count).ThenByDescending(x => x.Item2).ToList();

        MapNode result = null;
        while (result == null)
        {
            if (distances.Count == 0)
            {
                result = spawnRoom;
                break;
            }

            MapNode candidate = rooms.Single(x => x.Id == distances[0].Item1.Id);
            List<MapNode> path = NavigationManager.Instance.AStar(spawnRoom, candidate, out float distance);
            distances.RemoveAt(0);

            if (path != null)
            {
                result = candidate;
            }
        }

        return result;
    }

    private void SpawnSkeletonKeys(Map map, MapNode spawnRoom)
    {
        List<MapNode> rooms = new List<MapNode>();
        MapNode keyRoom = null;
        while (true)
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

            rooms = new List<MapNode>(map.Cells.Where(x => x.Type == MapNodeType.Room && !x.ContainsStairs));

            List<MapNode> lockedRooms = new List<MapNode>();
            foreach (MapNode room in rooms)
            {
                if (!room.Locked || room.ContainsStairs)
                {
                    continue;
                }

                List<MapNode> path = NavigationManager.Instance.AStar(spawnRoom, room, out float distance);

                if (path.Where(x => x.Locked).Count() == 1)
                {
                    lockedRooms.Add(room);
                }
            }

            if (lockedRooms.Count == 0)
            {
                break;
            }

            foreach (MapNode room in rooms)
            {
                foreach (MapNode lockedRoom in lockedRooms)
                {
                    room.Corridors.RemoveAll(x => x.ContainsVertex(new Delaunay.Vertex<MapNode>(lockedRoom.Cell.center, lockedRoom)));
                }
            }

            foreach (MapNode lockedRoom in lockedRooms)
            {
                rooms.Remove(lockedRoom);
            }

            foreach (MapNode lockedRoom in lockedRooms)
            {
                keyRoom = FindKeyRoom(spawnRoom, lockedRoom, rooms);

                Key newKey = Instantiate(_interactiveObjectContainer.skeletonKey, map.GetRandomPositionInRoom(1, 1, keyRoom).ToVector3(),
                    Quaternion.identity).GetComponent<Key>();

                map.AddInteractiveObject(newKey.gameObject);
                lockedRoom.Keys.Add(newKey.gameObject);
                lockedRoom.Locked = false;

                if (rooms.Count > 1)
                {
                    foreach (MapNode room in rooms)
                    {
                        room.Corridors.RemoveAll(x => x.ContainsVertex(new Delaunay.Vertex<MapNode>(keyRoom.Cell.center, keyRoom)));
                    }
                }
            }
        }
    }

    private void PlaceTraps(Map map, MapNode spawnRoom, Player player)
    {
        List<GameObject> traps = new List<GameObject>();
        Bounds playerBounds = new Bounds(player.transform.position, new Vector3(1, 1));
        foreach (MapNode room in map.Cells)
        {
            if (room == spawnRoom)
            {
                continue;
            }

            int trapCount = Mathf.RoundToInt(room.Cell.Area() * _random.Range(0.0f, 0.02f));
            for (int i = 0; i < trapCount; i++)
            {
                GameObject trapType = _trapContainer.GetRandomTrap();
                Vector2 position = map.GetRandomPositionInRoom(2, 2, room);

                Bounds bounds = new Bounds(position, new Vector3(2, 2));
                bool intersects = false;

                if (bounds.Intersects(playerBounds))
                {
                    continue;
                }

                foreach (GameObject interactiveObject in map.InteractiveObjects)
                {
                    Bounds otherBounds = new Bounds(interactiveObject.transform.position, bounds.size);
                    if (otherBounds.Intersects(bounds))
                    {
                        intersects = true;
                        break;
                    }
                }

                if (!intersects)
                {
                    map.AddInteractiveObject(GameObject.Instantiate(trapType, position, Quaternion.identity));
                }
            }
        }
    }

    private void SpawnSpawnables(Map map, int level, Tuple<MapNode, MapNode> startAndGoal)
    {
        SpawnableKeyframe lowestAbove = _spawnKeyframes.keyframes.Where(x => x.keyframeIndex >= level).FirstOrDefault();
        SpawnableKeyframe highestBelow = _spawnKeyframes.keyframes.OrderByDescending(x => x.keyframeIndex).Where(x => x.keyframeIndex < level).FirstOrDefault();

        if (level >= _spawnKeyframes.keyframes.Count)
        {
            lowestAbove = _spawnKeyframes.keyframes.Last();
        }

        float scaledLevel = Utility.ConvertRange(highestBelow.keyframeIndex, lowestAbove.keyframeIndex, 0.0f, 1.0f, level);
        int spawnableTypeCount = lowestAbove.spawnableObjects.Count;
        int avgArea = Mathf.RoundToInt((float)map.Cells.Average(x => x.Cell.Area()));

        foreach (MapNode room in map.Cells)
        {
            if (room.Equals(startAndGoal.Item1))
            {
                continue;
            }

            for (int i = 0; i < spawnableTypeCount; i++)
            {
                float density = Mathf.Lerp(highestBelow.spawnableObjects[i].density, lowestAbove.spawnableObjects[i].density, scaledLevel);
                int spawnableCount = Mathf.RoundToInt(density * room.Cell.Area() * (1.5f - room.SeclusionFactor));

                for (int j = 0; j < spawnableCount; j++)
                {
                    Spawn(map, lowestAbove.spawnableObjects[i].spawnablePrefab, room);
                }
            }
        }
    }

    private void Spawn(Map map, GameObject prefab, MapNode room)
    {
        Vector3 spawnPos = map.GetRandomPositionInRoom(1, 1, room).ToVector3();
        map.AddEnemy(GameObject.Instantiate(prefab, new Vector3(spawnPos.x, spawnPos.y, 0.0f), Quaternion.identity));
        map.Enemies[map.Enemies.Count - 1].SetActive(false);
    }
}