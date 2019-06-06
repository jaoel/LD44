using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    private static NavigationManager _instance;
    public static NavigationManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<NavigationManager>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a NavigationManager");
            }
            return _instance;
        }
    }

    public List<MapNode> AStar(MapNode start, MapNode target, out float distance)
    {
        distance = 0;
        if (start == target)
        {
            return new List<MapNode>();
        }

        List<Vector2Int> result = new List<Vector2Int>();
        HashSet<NavigationNode<MapNode>> closedSet = new HashSet<NavigationNode<MapNode>>();
        HashSet<NavigationNode<MapNode>> openSet = new HashSet<NavigationNode<MapNode>>() {
            new NavigationNode<MapNode>((int)(target.Cell.center - start.Cell.center).sqrMagnitude, 0, start) };

        while (openSet.Count > 0)
        {
            NavigationNode<MapNode> current = openSet.Aggregate((x, y) => x.FScore < y.FScore ? x : y);
            if (current.Data.Equals(target))
            {
                return UnwrapPath(current, out distance);
            }

            openSet.Remove(current);
            closedSet.Add(current);
            List<Tuple<MapNode, int>> neighbours = current.Data.Corridors.Aggregate(new List<Tuple<MapNode, int>>(), (list, y) =>
            {
                if (!y.Point1.Data.Equals(current.Data))
                {
                    list.Add(new Tuple<MapNode, int>(y.Point1.Data, (int)y.DistanceSquared));
                }

                if (!y.Point2.Data.Equals(current.Data))
                {
                    list.Add(new Tuple<MapNode, int>(y.Point2.Data, (int)y.DistanceSquared));
                }

                return list;
            });

            neighbours.ForEach(x =>
            {
                if (!closedSet.Any(closed => closed.Data.Equals(x.Item1)))
                {
                    if (!openSet.Any(open => open.Data.Equals(x.Item1)))
                    {
                        NavigationNode<MapNode> node = new NavigationNode<MapNode>((int)(target.Cell.center - x.Item1.Cell.center).sqrMagnitude, x.Item2, x.Item1);
                        node.Parent = current;
                        openSet.Add(node);
                    }
                }
            });
        }

        return null;
    }

    public List<MapNode> UnwrapPath(NavigationNode<MapNode> node, out float distance)
    {
        distance = 0;
        List<MapNode> result = new List<MapNode>();
        while (node != null)
        {
            distance += node.FScore;
            result.Add(node.Data);
            node = node.Parent;
        }

        return result;
    }

    public List<Vector2Int> AStar(Vector2Int origin, Vector2Int destination, out float distance)
    {
        Vector2Int start = origin;
        Vector2Int target = destination;

        distance = float.NegativeInfinity;
        if (start == target)
        {
            distance = 0.0f;
            return new List<Vector2Int>();
        }

        int collisionIndex = Main.Instance.CurrentMap.GetCollisionIndex(target.x, target.y);
        if (collisionIndex != 0)
        {
            return new List<Vector2Int>();
        }

        List<Vector2Int> result = new List<Vector2Int>();
        List<NavigationNode<Vector2Int>> closedSet = new List<NavigationNode<Vector2Int>>();
        List<NavigationNode<Vector2Int>> openSet = new List<NavigationNode<Vector2Int>>() { new NavigationNode<Vector2Int>((target - start).sqrMagnitude, 1, start) };

        while (openSet.Count > 0)
        {
            NavigationNode<Vector2Int> current = openSet.Aggregate((x, y) => x.FScore < y.FScore ? x : y);

            if (current.Data == target)
            {
                result = UnwrapPath(current, out distance);
                result.Reverse();
                break;
            }

            openSet.Remove(current);
            closedSet.Add(current);
            List<NavigationNode<Vector2Int>> neighbours = GetNeighbours(current, Main.Instance.CurrentMap.CollisionMap, false);

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (!closedSet.Any(x => x.Equals(neighbours[i])))
                {
                    if (!openSet.Any(x => x.Equals(neighbours[i])))
                    {
                        neighbours[i].Parent = current;
                        neighbours[i].HScore = (target - neighbours[i].Data).sqrMagnitude;
                        neighbours[i].GScore = current.GScore + 1;
                        openSet.Add(neighbours[i]);
                    }
                }
            }
        }
        return result;
    }

    public List<Vector2Int> UnwrapPath(NavigationNode<Vector2Int> node, out float distance)
    {
        distance = 0.0f;
        List<Vector2Int> result = new List<Vector2Int>();
        while (node != null)
        {
            distance += node.GScore;
            result.Add(node.Data);
            node = node.Parent;
        }

        return result;
    }

    public List<NavigationNode<Vector2Int>> GetNeighbours(NavigationNode<Vector2Int> node, int[,] collisionMap, bool allowDiagonal)
    {
        List<NavigationNode<Vector2Int>> result = new List<NavigationNode<Vector2Int>>();


        if (allowDiagonal)
        {
            for (int x = node.Data.x - 1; x <= node.Data.x + 1; x++)
            {
                for (int y = node.Data.y - 1; y <= node.Data.y + 1; y++)
                {
                    if (x < 0 || x >= collisionMap.GetLength(0) || y < 0 || y >= collisionMap.GetLength(1))
                    {
                        continue;
                    }

                    if (new Vector2Int(x, y) == node.Data || Main.Instance.CurrentMap.GetCollisionIndex(x, y) != 0)
                    {
                        continue;
                    }

                    result.Add(new NavigationNode<Vector2Int>(x, y, new Vector2Int(x, y)));
                }
            }
        }
        else
        {
            List<Vector2Int> potentials = new List<Vector2Int>();

            potentials.Add(new Vector2Int(node.Data.x, node.Data.y + 1));
            potentials.Add(new Vector2Int(node.Data.x, node.Data.y - 1));
            potentials.Add(new Vector2Int(node.Data.x + 1, node.Data.y));
            potentials.Add(new Vector2Int(node.Data.x - 1, node.Data.y));

            potentials.ForEach(x =>
            {
                if (Main.Instance.CurrentMap.GetCollisionIndex(x.x, x.y) == 0)
                {
                    result.Add(new NavigationNode<Vector2Int>(x.x, x.y, new Vector2Int(x.x, x.y)));
                }

            });
        }

        return result;
    }
}
