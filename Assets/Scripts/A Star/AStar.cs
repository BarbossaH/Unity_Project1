using System.Collections;
using System.Collections.Generic;
using MFarm.AStar;
using MFarm.Map;
using UnityEngine;
namespace MFarm.AStar
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes gridNodes;
        private Node startNode;
        private Node endNode;

        private int gridWidth;
        private int gridHeight;

        private int originX;
        private int originY;

        private List<Node> openNodeList; //to store the eight nodes around the current chosen node.
                                         //All selected nodes have the smallest FCost value, but this does mean they are the final navigation route.
        private HashSet<Node> closeNodeHashSet;

        private bool isPathFound;

        //实际生成巡路格子的函数。the function that actually generates the pathfinding grids
        private bool GenerateGridNodes(string sceneName, Vector2Int startPosInWorld, Vector2Int endPosInWorld)
        {
            if (GridManager.Instance.GetGridDimensions(sceneName, out Vector2Int mapSize, out Vector2Int gridOrigin))
            {
                gridNodes = new GridNodes(mapSize.x, mapSize.y);
                gridWidth = mapSize.x;
                gridHeight = mapSize.y;
                originX = gridOrigin.x;
                originY = gridOrigin.y;

                openNodeList = new List<Node>();
                closeNodeHashSet = new HashSet<Node>();
            }
            else
                return false;

            startNode = gridNodes.GetGridNode(startPosInWorld.x - originX, startPosInWorld.y - originY);
            endNode = gridNodes.GetGridNode(endPosInWorld.x - originX, endPosInWorld.y - gridOrigin.y);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    //获得当前场景的合法的格子数据
                    //这个只能返回当前激活的场景的格子，可能是错误的？？？？
                    Vector3Int cellPosInWorld = new Vector3Int(x + originX, y + originY, 0);
                    TileDetails tileDetails = GridManager.Instance.GetTileDetailsByWorldPosition(cellPosInWorld);
                    if (tileDetails != null)
                    {
                        //tileDetails 可能是空，因为地图是不规则的
                        Node node = gridNodes.GetGridNode(x, y);
                        if (tileDetails.isNPCObstacle)
                        {
                            //这里也就说如果没有故意设置阻挡，那么就是可以行走的。great
                            //这里等于把node的isObstacle赋值了
                            node.isObstacle = true;
                        }
                    }
                }
            }

            return true;
        }
        public void BuildPath(string sceneName, Vector2Int startPosInWorld, Vector2Int endPosInWorld, Stack<MovementStep> npcMovementStep)
        {
            isPathFound = false;
            if (GenerateGridNodes(sceneName, startPosInWorld, endPosInWorld))
            {
                if (FindShortestPath())
                {
                    //构建npc路径
                    UpdatePathMovementStepStack(sceneName, npcMovementStep);
                }
            }
        }

        private bool FindShortestPath()
        {
            openNodeList.Add(startNode);

            while (openNodeList.Count > 0)
            {
                //排序，取出最小的FCost的点
                openNodeList.Sort();
                Node closeNode = openNodeList[0];
                openNodeList.RemoveAt(0);
                closeNodeHashSet.Add(closeNode);
                if (closeNode == endNode)
                {
                    //每次都要判断一下取出的点是不是终点，如果是终点，那么就跳出巡路。
                    isPathFound = true;
                    break;
                }
                //当获得最近的点时，就去找附近的周围的8个点
                EvaluateNeighborNodes(closeNode);
            }
            return isPathFound;
        }
        private void EvaluateNeighborNodes(Node currentNode)
        {
            Vector2Int currentPos = currentNode.gridPosition;
            Node validNeighborNode;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        //don't need to check itself
                        continue;
                    }
                    //获得合法的点，如果不是障碍和已经在关闭哈希表中，那么就是合法的点
                    validNeighborNode = GetValidNeighborNode(currentPos.x + x, currentPos.y + y);
                    if (validNeighborNode != null)
                    {
                        //这些合法的点，看看是不是新生成的点，还是已经在open list中了
                        #region my method
                        // if (!openNodeList.Contains(validNeighborNode))
                        // {
                        //     openNodeList.Add(validNeighborNode);
                        // }
                        // validNeighborNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighborNode);
                        // validNeighborNode.hCost = currentNode.gCost + GetDistance(currentNode, endNode);
                        // validNeighborNode.parentNode = currentNode;
                        #endregion

                        if (!openNodeList.Contains(validNeighborNode))
                        {
                            //如果不在open list中就生成就添加到open list
                            //计算gCost，hCost和FCost
                            //(这个地方可能有问题，因为有可能一开始就找到了一条错误的路，但是也能走通，却不是最短路径，比如遇到了比较长的障碍时，就可能出现这种情况，所以即便是已经进入open list，但是周边的八个点仍然要重新计算gCost和hCost，如果重新计算的值FCost比原来的小，则重新指定父节点，如果大于或者等于则可以不用重新赋值父节点)
                            validNeighborNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighborNode);
                            validNeighborNode.hCost = currentNode.gCost + GetDistance(currentNode, endNode);
                            validNeighborNode.parentNode = currentNode;
                            openNodeList.Add(validNeighborNode);
                        }
                        else
                        {
                            //如果在open list中，那么就要对比一下原来的FCost值
                            int temporaryGCost = currentNode.gCost + GetDistance(currentNode, validNeighborNode);
                            int temporaryHCost = currentNode.gCost + GetDistance(currentNode, endNode);
                            if (temporaryGCost + temporaryHCost < validNeighborNode.FCost)
                            {
                                //如果以新的当前点为父节点，FCost更小的话，那么就重新修改节点的值
                                validNeighborNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighborNode);
                                validNeighborNode.hCost = currentNode.gCost + GetDistance(currentNode, endNode);
                                validNeighborNode.parentNode = currentNode;
                            }

                        }
                    }
                }
            }
        }

        private Node GetValidNeighborNode(int x, int y)
        {
            if (x >= gridWidth || y >= gridHeight || x < 0 || y < 0)
            {
                return null;
            }
            Node neighborNode = gridNodes.GetGridNode(x, y);
            if (neighborNode.isObstacle || closeNodeHashSet.Contains(neighborNode))
            {
                return null;
            }

            return neighborNode;

        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int distance_X = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int distance_Y = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            if (distance_X > distance_Y)
            {
                return 14 * distance_Y + 10 * (distance_X - distance_Y);
            }

            return 14 * distance_X + 10 * (distance_Y - distance_X);
        }

        private void UpdatePathMovementStepStack(string sceneName, Stack<MovementStep> npcMovementStep)
        {
            Node nextNode = endNode;
            while (nextNode != null)
            {
                MovementStep newStep = new MovementStep();
                newStep.sceneName = sceneName;
                newStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY);
                npcMovementStep.Push(newStep);
                nextNode = nextNode.parentNode;
            }
        }
    }
}