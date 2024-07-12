using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.Inventory;
using UnityEngine;
/*
先用字典管理起来身体各个部位（player的多个子对象，如身体，手臂）的Animator组件
配置好动作和部位数据列表，然后根据快捷栏的事件来判断哪一个工具被选中了。
根据被选中的工具，修改player各个子对象Animator对应的runtimeAnimatorController
First manage the Animator components of various body parts(multiple child objects of the player, such as body and arm) by a dictionary.And then configure the list of actions(override controller) and body part data,and determine which tool is selected based on the event from the shortcut bar. Depending on the selected tool, modify the runtimeAnimatorController of the corresponding Animator for each player child object. 
*/
public class AnimatorManager : MonoBehaviour
{
    private Animator[] animators;
    public SpriteRenderer holdItem;
    //animatorTypeList 这个是需要配置的
    public List<AnimatorType> animatorTypeList;
    private Dictionary<string, Animator> animatorsDict = new Dictionary<string, Animator>();
    private void Awake()
    {

        animators = GetComponentsInChildren<Animator>();
        foreach (var anim in animators)
        {
            //anim.name is the name of the object, like Hair, Arm
            // Debug.Log(anim.name);
            animatorsDict.Add(anim.name, anim);
        }
    }

    private void OnEnable()
    {
        NotifyCenter<UIEvent, bool, bool>.notifyCenter += OnClickSlot;
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnBeforeSceneLoad;
        NotifyCenter<SceneEvent, int, Vector3>.notifyCenter += OnSceneChange;
        NotifyCenter<SceneEvent, int, bool>.notifyCenter += OnHarvestAtPlayerPosition;
    }

    private void OnDisable()
    {
        NotifyCenter<UIEvent, bool, bool>.notifyCenter -= OnClickSlot;
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnBeforeSceneLoad;
        NotifyCenter<SceneEvent, int, Vector3>.notifyCenter -= OnSceneChange;
        NotifyCenter<SceneEvent, int, bool>.notifyCenter -= OnHarvestAtPlayerPosition;

    }

    private void OnHarvestAtPlayerPosition(SceneEvent sceneEvent, int itemID, bool arg3)
    {
        if (sceneEvent == SceneEvent.InstantiateCropAtPlayer)
        {
            Sprite sprite = InventoryManager.Instance.GetItemDetails(itemID).itemIconInWorld;
            if (sprite != null && holdItem.enabled == false)
            {
                StartCoroutine(ShowItemOnHead(sprite));
            }
        }
    }

    private IEnumerator ShowItemOnHead(Sprite sprite)
    {
        holdItem.enabled = true;

        holdItem.sprite = sprite;

        yield return new WaitForSeconds(2f);

        holdItem.sprite = null;

        holdItem.enabled = false;
    }
    private void OnSceneChange(SceneEvent scentEvent, int ID, Vector3 pos)
    {
        if (scentEvent == SceneEvent.DropItemInScene)
        {
            ItemDetails itemDetails = InventoryManager.Instance.GetCurrentSelectedItem();
            if (itemDetails == null)
            {
                holdItem.enabled = false;
                SwitchAnimator(ActionType.None);
            }
        }
    }

    private void OnBeforeSceneLoad(SceneEvent sceneEvent, bool arg2, bool arg3)
    {
        if (sceneEvent == SceneEvent.BeforeLoadScene)
        {
            holdItem.enabled = false;
            SwitchAnimator(ActionType.None);
        }
    }

    //By the type of items picked up, identify which part of the character needs to change the Animator Controller.
    private void OnClickSlot(UIEvent iEvent, bool arg2, bool arg3)
    {
        if (iEvent == UIEvent.ClickSlot)
        {
            ActionType currentType;
            ItemDetails itemDetails = InventoryManager.Instance.GetCurrentSelectedItem();

            if (itemDetails != null)
            {
                currentType = itemDetails.itemType switch
                {
                    ItemType.Seed => ActionType.Carry,
                    ItemType.Commodity => ActionType.Carry,
                    ItemType.HeoTool => ActionType.Hoe,
                    ItemType.WaterTool => ActionType.Water,
                    ItemType.CollectTool => ActionType.Basket,
                    ItemType.ChopTool => ActionType.Chop,
                    _ => ActionType.None
                };
                if (currentType == ActionType.Carry)
                {
                    holdItem.sprite = itemDetails.itemIconInWorld;
                    holdItem.enabled = true;
                }
                else
                {
                    //这个地方是有问题的。因为除了水之外，其他的工具的动作都是跟常规的混合在一起的，所以当举起物品时，再切换到非水工具时就会出问题。因为没有办法切换到常规的动作。动作应该根据工具去制作。每持有一种工具都是一套的动作。而且不要在编辑器里写，无法跟踪，最好在代码里写，并写上注释
                    holdItem.sprite = null;
                    holdItem.enabled = false;
                }

            }
            else
            {
                holdItem.enabled = false;
                currentType = ActionType.None;
                // Debug.Log(123);
            }

            //根据工具和工具的是否被选中的状态修改角色动作的
            SwitchAnimator(currentType);
        }

    }

    private void SwitchAnimator(ActionType actionType)
    {
        // Debug.Log(actionType.ToString());
        foreach (var item in animatorTypeList)
        {
            if (item.actionType == actionType)
            {
                // Debug.Log(item.actionType.ToString());
                animatorsDict[item.bodyPart.ToString()].runtimeAnimatorController = item.animatorOverrideController;
            }
        }
    }
}
