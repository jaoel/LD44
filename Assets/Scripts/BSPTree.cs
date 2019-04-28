using System.Collections.Generic;
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

        public void GetAllLeafNodes(ref List<BSPTree> leaves)
        {
            if (IsLeaf)
            {
                leaves.Add(this);
            }
            else
            {
                if (Right != null)
                    Right.GetAllLeafNodes(ref leaves);

                if (Left != null)
                    Left.GetAllLeafNodes(ref leaves);
            }
        }

        public BSPTree GetSibling()
        {
            BSPTree parent = Parent;

            while(true)
            {
                if (parent.Left != null && parent.Left != this)
                    return parent.Left;
                else if (parent.Right != null && parent.Right != this)
                    return parent.Right;

                parent = parent.Parent;
            } 
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
