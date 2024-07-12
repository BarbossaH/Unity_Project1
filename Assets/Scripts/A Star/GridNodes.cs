using UnityEngine;

namespace MFarm.AStar
{
    /*这类是管理和计算所有格子的类*/
    public class GridNodes
    {
        //横向有多少格子
        private int width;
        //纵向有多少个格子
        private int height;

        //this variable is for storing all the node
        private Node[,] gridNodes;

        public GridNodes(int width, int height)
        {
            this.width = width;
            this.height = height;
            gridNodes = new Node[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridNodes[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }

        public Node GetGridNode(int posX, int posY)
        {
            if (posX < width && posY < height && posX >= 0 && posY >= 0)
            {
                return gridNodes[posX, posY];
            }
            Debug.Log("Out of the gird range ");
            return null;
        }


    }


}