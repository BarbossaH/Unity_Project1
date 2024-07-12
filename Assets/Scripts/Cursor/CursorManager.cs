using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MFarm.Map;
using MFarm.Inventory;

public class CursorManager : MonoBehaviour
{
    public Sprite normalSprite, toolSprite, seedSprite, itemSprite;

    private Sprite currentSprite;

    private Image cursorImage;
    private RectTransform cursorCanvas;

    //checking cursor.因为鼠标要根据当前地图的信息和物品的状态显示不同的图片
    private Camera mainCamera;
    private Grid currentSceneGrid; //在完成切换完地图取得当前地图格子
    private Vector3 mouseWorldPosition;
    private Vector3Int mouseGridPosition;
    private bool enableCursor;//加载场景地图完成之后，鼠标检测再开始启用，实际上是只有要使用道具时开需要开启，默认是关闭的
    private bool isValidAtMousePos; //根据位置判断鼠标是不是可用的
    //拿到player的位置，这样就可以判断物品扔的范围，不能扔太远
    private Transform playerTransform => FindObjectOfType<Player>().transform;

    // private ItemDetails currentSelectedItem; //记录一下当前被选中的物品in action bar

    private void Start()
    {
        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        // cursorImage = GameObject.FindGameObjectWithTag("CursorImage").GetComponent<Image>();
        //the method below is not reliable,because once the oder has been changed inadvertently, it will go wrong.
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        // Debug.Log(cursorImage.name);
        mainCamera = Camera.main;
        Init();
    }

    private void Init()
    {
        currentSprite = normalSprite;
        SetCursorImage(currentSprite);
        enableCursor = false;
        isValidAtMousePos = false;
        currentSceneGrid = null;
    }
    private void Update()
    {
        //鼠标的操作最好在格子数据准备好后再进行，否则如果格子没有数据就会报错，这就是之前我需要判断格子是否存在的原因。
        //而格子的数据智能在内存中保持一份，就是交给GridManager去管理
        if (cursorCanvas == null) return;
        cursorImage.transform.position = Input.mousePosition;
        // Debug.Log(currentSelectedItem);
        if (enableCursor && !CheckInteractWithUI())
        {
            SetCursorImage(currentSprite);
            CheckCursorValid();
            CheckInput();
        }
        else
        {
            SetCursorImage(normalSprite);
        }
        // SetCursorImage(CheckInteractWithUI() ? normalSprite : currentSprite);
    }
    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);
    }

    private void OnEnable()
    {
        // NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnBeforeScene;
        NotifyCenter<UIEvent, bool, bool>.notifyCenter += OnClickSlotEvent;
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnSceneChange;
    }

    private void OnDisable()
    {
        // NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnBeforeScene;

        NotifyCenter<UIEvent, bool, bool>.notifyCenter -= OnClickSlotEvent;
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnSceneChange;
    }

    // private void OnBeforeScene(SceneEvent sceneEvent, bool arg2, bool arg3)
    // {
    // }

    private void OnSceneChange(SceneEvent sceneEvent, bool arg2, bool arg3)
    {
        if (sceneEvent == SceneEvent.AfterLoadScene)
        {
            // Debug.Log(123);
            currentSceneGrid = FindObjectOfType<Grid>();
        }
        else if (sceneEvent == SceneEvent.BeforeLoadScene)
        {
            //在切换场景前，重置鼠标状态为初始的状态；
            Init();
            // //在切换场景前，清除鼠标状态
            // enableCursor = false;
            // //设置鼠标图片为默认图片
            // currentSprite = normalSprite;
            // //我认为也应该清除记录的商品
            // currentSelectedItem = null;
        }
    }

    private void OnClickSlotEvent(UIEvent uiEvent, bool arg2, bool arg3)
    {
        if (uiEvent == UIEvent.ClickSlot)
        {
            var currentItem = InventoryManager.Instance.GetCurrentSelectedItem();
            if (currentItem != null)
            {
                //只有在选中的状态下才进行鼠标检测
                enableCursor = true;

            }
            else
            {
                enableCursor = false;
            }

        }
    }
    private bool CheckInteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        return false;
    }

    private void SetCursorState(bool isValid)
    {
        // Debug.Log(isValid);
        isValidAtMousePos = isValid;
        cursorImage.color = isValid ? new Color(1, 1, 1, 1) : new Color(1, 0, 0, 0.5f);
    }
    private void CheckCursorValid()
    {
        if (currentSceneGrid != null)
        {
            // mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            //拿到鼠标的世界坐标
            mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
            //获取当前选中的物品
            ItemDetails currentSelectedItem = InventoryManager.Instance.GetCurrentSelectedItem();
            if (currentSelectedItem != null)
            {
                currentSprite = currentSelectedItem.itemType switch
                {
                    ItemType.Seed => seedSprite,
                    ItemType.Commodity => itemSprite,
                    ItemType.HeoTool => toolSprite,
                    ItemType.ChopTool => toolSprite,
                    ItemType.BreakTool => toolSprite,
                    ItemType.WaterTool => toolSprite,
                    ItemType.Furniture => toolSprite,
                    ItemType.CollectTool => toolSprite,
                    _ => normalSprite
                };
                // Debug.Log(111);
                // Debug.Log(currentSelectedItem.itemType.ToString());
                SetCursorImage(currentSprite);

                //获取当前鼠标所在的格子
                TileDetails currentTile = GridManager.Instance.GetTileDetailsOnMousePosition(mouseGridPosition);
                //判断是否在可以扔的范围内
                mouseGridPosition = currentSceneGrid.WorldToCell(mouseWorldPosition);
                var playerGridPos = currentSceneGrid.WorldToCell(playerTransform.position);
                if (currentTile != null
                    && Mathf.Abs(mouseGridPosition.x - playerGridPos.x) <= currentSelectedItem.itemUsedRadius
                    && Mathf.Abs(mouseGridPosition.y - playerGridPos.y) <= currentSelectedItem.itemUsedRadius)
                {

                    switch (currentSelectedItem.itemType)
                    {
                        case ItemType.Seed:
                            SetCursorState(currentTile.daySinceDig > -1 && currentTile.seedItemID == -1);
                            break;
                        case ItemType.Commodity:
                            SetCursorState(currentSelectedItem.canDropped && currentTile.canDropItem);
                            break;
                        case ItemType.HeoTool:
                            SetCursorState(currentTile.canDig);
                            break;
                        case ItemType.WaterTool:
                            SetCursorState(currentTile.daySinceDig >= 0 && currentTile.daySinceWatered == -1);
                            break;
                        case ItemType.BreakTool:
                        case ItemType.ChopTool:
                        // break;
                        case ItemType.CollectTool:
                            // Debug.Log(CropManager.Instance);
                            CropDetails currentCropDetails = GridManager.Instance.GetCropDetails(currentTile.seedItemID);

                            if (currentCropDetails != null)
                            {
                                if (currentCropDetails.CheckToolAvailable(currentSelectedItem.itemID))
                                {
                                    //代表是否成熟了，如果成熟了就可以将鼠标设置为合法的。这个地方也成为检测是否拔出成熟的条件，但其实放在这里是极其不合适的。我觉得鼠标只需要检测或者显示当前工具的样子就行，检测应该放到具体的格子去执行。这个地方造成了很多理解的困难
                                    SetCursorState(currentTile.growDays >= currentCropDetails.TotalGrowthDays);

                                }
                            }
                            else
                            {
                                SetCursorState(false);
                            }
                            break;
                    }
                }
                else
                {
                    SetCursorState(false);
                }
            }
            else
            {
                //代表没有选中物品，就显示正常鼠标
                SetCursorImage(normalSprite);
                // Debug.Log(222);
            }



            //如果鼠标的位置和角色的位置超过了物品的扔的范围，就显示不可扔的状态
            // if (Mathf.Abs(mouseGridPosition.x - playerGridPos.x) > currentSelectedItem.itemUsedRadius
            // || Mathf.Abs(mouseGridPosition.y - playerGridPos.y) > currentSelectedItem.itemUsedRadius)
            // {
            //     SetCursorState(false);
            //     return;
            // }
            // Debug.Log("WorldPos:" + mouseWorldPosition + "GridPos: " + mouseGridPosition);

            //因为这里是需要两件事情同时作用才会生效，第一就是选中了某个物品，第二就是检测鼠标所在的格子信息；如果没有选中物品，则没必要检测
            // if (currentTile != null && currentSelectedItem != null)
            // {
            //     switch (currentSelectedItem.itemType)
            //     {
            //         case ItemType.Commodity:
            //             SetCursorState(currentSelectedItem.canDropped && currentTile.canDropItem);
            //             break;
            //     }
            // }
            // else
            // {
            //     SetCursorState(false);
            // }
        }
    }


    private void CheckInput()
    {
        //按下鼠标左键,并且在鼠标所在的位置是合法的位置
        //只有在可扔范围内，在可扔区域，而且物品是可扔的三个条件都达成的前提下才成立的
        if (Input.GetMouseButtonDown(0) && isValidAtMousePos)
        {
            //发出事件通知对应的响应,这里只是通知player播放动画，因为最好的效果是等动画播放完了，再产生实际的效果，比如砍树，砍树的动作播放完成场景再开始响应
            var currentSelectedItem = InventoryManager.Instance.GetCurrentSelectedItem();
            if (currentSelectedItem != null)
            {
                //这里用鼠标的状态检测是否可以进行操作，比如浇水和挖地，但是鼠标管理类只负责鼠标的显示，而真正是否可以操作，则应该由格子管理类自己去判断
                /*The state of the cursor is used to check whether action like digging and watering can be performed.However the cursor management class only handles display, while the actual determination of whether an action can be performed should be handled by the tile management class itself*/
                NotifyCenter<SceneEvent, Vector3, ItemDetails>.NotifyObservers(SceneEvent.MouseClickEvent, mouseWorldPosition, currentSelectedItem);
            }

        }
    }
}
