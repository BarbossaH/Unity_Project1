using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.AStar
{
    public class Node : IComparable<Node>
    {
        public Vector2Int gridPosition;
        public int gCost = 0;//the distance to start point
        public int hCost = 0;//the distance to target

        public int FCost => gCost + hCost;

        public bool isObstacle = false;
        public Node parentNode;
        public Node(Vector2Int pos)
        {
            gridPosition = pos;
            parentNode = null;
        }

        //用于比较open list当中最小的那个节点，放入close list中
        public int CompareTo(Node other)
        {
            int result = FCost.CompareTo(other.FCost);
            if (result == 0)
            {
                //对比到终点的距离
                result = hCost.CompareTo(other.hCost);
            }

            return result;
        }

    }
}