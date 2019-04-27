using UnityEngine;

namespace Assets.Scripts
{
    public class BSPTree
    {
        public BSPTree Parent { get; set; }
        public BSPTree Left { get; set; }
        public BSPTree Right { get; set; }
        public RectInt Grid { get; set; }  
        public RectInt Room { get; set; }

        public bool IsLeaf
        {
            get { return Left == null && Right == null; }
        }

        public bool IsInternal
        {
            get { return Left != null || Right != null; }
        }

        public BSPTree(BSPTree parent, RectInt grid)
        {
            Parent = parent;
            Grid = grid;    
        }

        public static void DebugDrawBspNode(BSPTree node)
        {
            // Container
            Gizmos.color = Color.green;
            // top
            Gizmos.DrawLine(new Vector3(node.Grid.x, node.Grid.y, 0), new Vector3Int(node.Grid.xMax, node.Grid.y, 0));
            // right
            Gizmos.DrawLine(new Vector3(node.Grid.xMax, node.Grid.y, 0), new Vector3Int(node.Grid.xMax, node.Grid.yMax, 0));
            // bottom
            Gizmos.DrawLine(new Vector3(node.Grid.x, node.Grid.yMax, 0), new Vector3Int(node.Grid.xMax, node.Grid.yMax, 0));
            // left
            Gizmos.DrawLine(new Vector3(node.Grid.x, node.Grid.y, 0), new Vector3Int(node.Grid.x, node.Grid.yMax, 0));

            // children
            if (node.Left != null)
                DebugDrawBspNode(node.Left);
            if (node.Right != null)
                DebugDrawBspNode(node.Right);
        }
    }
}
