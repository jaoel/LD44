using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static int[,] collisionMap;
    public static Map map;

    private static NavigationManager instance;
    public static NavigationManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            instance = FindObjectOfType<NavigationManager>();
            if (instance == null || instance.Equals(null))
            {
                Debug.LogError("The scene needs a NavigationManager");
            }
            return instance;
        }
    }

    public List<Vector2Int> AStar(Vector2Int start, Vector2Int target)
    {
        if (start == target)
            return new List<Vector2Int>();

        int collisionIndex = collisionMap[target.x, target.y];
        if (collisionIndex != 0)
            return new List<Vector2Int>();

        List<Vector2Int> result = new List<Vector2Int>();
        List<NavigationNode> closedSet = new List<NavigationNode>();
        List<NavigationNode> openSet = new List<NavigationNode>() { new NavigationNode((target - start).sqrMagnitude, 1, start) };
        
        while(openSet.Count > 0)
        {
            NavigationNode current = openSet.Aggregate((x, y) => x.FScore < y.FScore ? x : y);

            if (current.Position == target)
            {
                result = UnwrapPath(current);
                result.Reverse();
                break;
            }

            openSet.Remove(current);
            closedSet.Add(current);
            List<NavigationNode> neighbours = GetNeighbours(current, collisionMap, false);
            
            for(int i = 0; i < neighbours.Count; i++)
            {
                if (!closedSet.Contains(neighbours[i]))
                {
                    if (!openSet.Contains(neighbours[i]))
                    {
                        neighbours[i].Parent = current;
                        neighbours[i].HScore = (target - neighbours[i].Position).sqrMagnitude;
                        neighbours[i].GScore = current.GScore + 1;
                        openSet.Add(neighbours[i]);
                    }
                }
            }
        }
        return result;
    }

    public List<Vector2Int> UnwrapPath(NavigationNode node)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        while (node != null && node.Parent != null)
        {
            result.Add(node.Position);
            node = node.Parent;
        }

        return result;
    }

    public List<NavigationNode> GetNeighbours(NavigationNode node, int[,] collisionMap, bool allowDiagonal)
    {
        List<NavigationNode> result = new List<NavigationNode>();


        if (allowDiagonal)
        {
            for (int x = node.Position.x - 1; x <= node.Position.x + 1; x++)
            {
                for (int y = node.Position.y - 1; y <= node.Position.y + 1; y++)
                {
                    if (x < 0 || x >= collisionMap.GetLength(0) || y < 0 || y >= collisionMap.GetLength(1))
                        continue;

                    if (new Vector2Int(x, y) == node.Position || collisionMap[x, y] != 0)
                        continue;

                    result.Add(new NavigationNode(x, y));
                }
            }
        }
        else
        {
            List<Vector2Int> potentials = new List<Vector2Int>();

            potentials.Add(new Vector2Int(node.Position.x, node.Position.y + 1));
            potentials.Add(new Vector2Int(node.Position.x, node.Position.y - 1));
            potentials.Add(new Vector2Int(node.Position.x + 1, node.Position.y));
            potentials.Add(new Vector2Int(node.Position.x - 1, node.Position.y - 1));

            potentials.ForEach(x =>
            {
                if (collisionMap[x.x, x.y] == 0)
                    result.Add(new NavigationNode(x.x, x.y));

            });
        }

      

        return result;
    }
}
