using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MFarm.AStar
{
    public class AStarTest : MonoBehaviour
    {
        private AStar aStar;
        [Header("用于测试")]
        public Vector2Int startPos;
        public Vector2Int endPos;

        public Tilemap displayMap;

        public TileBase displayTile;

        public bool displayStartAndFinish; //只有勾选在现实tile 显示起点和终点
        public bool displayPath;//显示路径

        private Stack<MovementStep> npcMovementStack;
        [Header("测试移动NPC")]
        public NPCMovement npcMovement;
        public bool moveNPC;
        public string sceneName;
        public Vector2Int targetPos;
        public AnimationClip stopClip;
        private void Awake()
        {
            aStar = GetComponent<AStar>();
            npcMovementStack = new Stack<MovementStep>();
        }

        private void Update()
        {
            ShowPathGridMap();
            if (moveNPC)
            {
                moveNPC = false;
                var schedule = new ScheduleDetails(0, 0, 0, 0, Season.Spring, sceneName, targetPos, stopClip, true);
                npcMovement.BuildPath(schedule);
            }
        }

        private void ShowPathGridMap()
        {
            if (displayMap != null && displayTile != null)
            {
                if (displayStartAndFinish)
                {
                    displayMap.SetTile((Vector3Int)startPos, displayTile);
                    displayMap.SetTile((Vector3Int)endPos, displayTile);
                }
                else
                {
                    ResetStartAndEnd();
                }

                if (displayPath)
                {
                    var sceneName = SceneManager.GetActiveScene().name;
                    // Debug.Log($"aStar{aStar}");
                    // Debug.Log($"startPos{startPos}");
                    // Debug.Log($"endPos{endPos}");
                    // Debug.Log($"npcMovementStack{npcMovementStack}");

                    aStar.BuildPath(sceneName, startPos, endPos, npcMovementStack);
                    foreach (var step in npcMovementStack)
                    {
                        displayMap.SetTile((Vector3Int)step.gridCoordinate, displayTile);
                    }
                }
                else
                {
                    ResetStack();
                }
            }
        }
        private void ResetStartAndEnd()
        {
            displayMap.SetTile((Vector3Int)startPos, null);
            displayMap.SetTile((Vector3Int)endPos, null);
        }
        private void ResetStack()
        {
            if (npcMovementStack.Count > 0)
            {
                foreach (var step in npcMovementStack)
                {
                    displayMap.SetTile((Vector3Int)step.gridCoordinate, null);
                }
                npcMovementStack.Clear();
            }
        }
    }
}