using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        //所有物品数据
        public ItemDataList_SO itemDataList_SO;
        [Header("Bag Data")]
        //背包数据库
        public InventoryBag_SO playerBag_SO;
        private int CountOfBag;

        private ItemDetails currentSelectedItem;

        protected override void Awake()
        {
            base.Awake();
            CountOfBag = playerBag_SO.itemsInBag.Count;
        }

        private void Start()
        {
            //get data from data base to refresh UI, but in an online, the data should be stored in memory
            NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.ActionBar, true);

        }
        private void OnEnable()
        {
            NotifyCenter<SceneEvent, int, bool>.notifyCenter += OnHarvestAtPlayerPosition;
        }

        private void OnDisable()
        {
            NotifyCenter<SceneEvent, int, bool>.notifyCenter -= OnHarvestAtPlayerPosition;
        }
        private void OnHarvestAtPlayerPosition(SceneEvent sceneEvent, int itemId, bool arg3)
        {
            if (sceneEvent == SceneEvent.InstantiateCropAtPlayer)
            {
                var index = GetItemIndexInBag(itemId);
                // Debug.Log("index:" + index + "itemID: " + itemId);
                AddItemAtIndex(itemId, index, 1);
                // Update UI, here using observer pattern
                NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.ActionBar, true);
            }
        }
        public ItemDetails GetItemDetails(int itemID)
        {
            // Debug.Log(itemID);
            return itemDataList_SO.itemDetailsList.Find(item => item.itemID == itemID);
            // return itemDataList_SO.itemDetails.Find(item => item.itemID == itemID);
        }

        public ItemDetails GetItemInBagByIndex(int index)
        {
            // var item = playerBag_SO.itemsInBag[index];

            return GetItemDetails(playerBag_SO.itemsInBag[index].itemID);
        }

        public void AddItem(Item item, bool toDestroy)
        {
            //check is there room for new items;
            //check is there the same items
            var index = GetItemIndexInBag(item.itemID);

            if (AddItemAtIndex(item.itemID, index, 1))
            {
                if (toDestroy)
                {
                    Destroy(item.gameObject);
                }
            }
            // Update UI, here using observer pattern
            NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.ActionBar, true);
        }

        //check the capacity of the bag
        private bool CheckBadCapacity()
        {
            for (int i = 0; i < CountOfBag; i++)
            {
                if (playerBag_SO.itemsInBag[i].itemID == 0)
                {
                    return true;
                }
            }
            return false;
        }
        //get the position of the same items

        private int GetItemIndexInBag(int itemID)
        {
            for (int i = 0; i < CountOfBag; i++)
            {
                if (playerBag_SO.itemsInBag[i].itemID == itemID)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool AddItemAtIndex(int itemID, int index, int amount)
        {
            if (index == -1 && CheckBadCapacity())
            {
                //add a new item
                var item = new InventoryItem { itemID = itemID, itemAmount = amount };
                // Debug.Log($"item's {item.itemID}");
                for (int i = 0; i < CountOfBag; i++)
                {
                    if (playerBag_SO.itemsInBag[i].itemID == 0)
                    {
                        // Debug.Log(123321);
                        playerBag_SO.itemsInBag[i] = item;
                        break;
                    }
                }
                return true;
            }
            else if (index != -1)
            {
                //stacking this item in the inventory onto the same item means only stack the quantity
                int currentAmount = playerBag_SO.itemsInBag[index].itemAmount + amount;
                //does not work
                // playerBag_SO.itemsInBag[index].itemAmount = playerBag_SO.itemsInBag[index].itemAmount + amount;
                var item = new InventoryItem(itemID, currentAmount);
                playerBag_SO.itemsInBag[index] = item;
                // Debug.Log(playerBag_SO.itemsInBag[index]);
                return true;

            }
            return false;
        }

        public void SwapItem(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = playerBag_SO.itemsInBag[fromIndex];
            InventoryItem targetItem = playerBag_SO.itemsInBag[targetIndex];

            if (targetItem.itemID != 0)
            {
                playerBag_SO.itemsInBag[fromIndex] = targetItem;
                playerBag_SO.itemsInBag[targetIndex] = currentItem;
            }
            else
            {
                playerBag_SO.itemsInBag[targetIndex] = currentItem;
                playerBag_SO.itemsInBag[fromIndex] = new InventoryItem();

            }

            NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.ActionBar, true);

        }

        public void RemoveItem(int id, int removeAmount = 1)
        {
            var index = GetItemIndexInBag(id);
            // Debug.Log(index);
            if (index == -1) return;//表示没有这个物品
            // Debug.Log(index);
            if (playerBag_SO.itemsInBag[index].itemAmount >= removeAmount)
            {
                var amount = playerBag_SO.itemsInBag[index].itemAmount - removeAmount;
                if (amount > 0)
                {
                    var item = new InventoryItem(id, amount);
                    playerBag_SO.itemsInBag[index] = item;
                    //I can't use the following method because the data below is a list of structures, and assigning a structure directly will create a copy of the data, rather than modifying the original data. You can either completely overwrite the original data or use class types instead of structures as the data type.

                    // playerBag_SO.itemsInBag[index].itemAmount = amount;
                }
                else
                {
                    var item = new InventoryItem(0, 0);
                    SetCurrentSelectedItem(null); //东西扔没了，就没有当前物品了
                    playerBag_SO.itemsInBag[index] = item;
                    //通知鼠标改变状态，和角色动作改变
                }
            }
            //通知UI刷新
            NotifyCenter<UIEvent, bool, float>.NotifyObservers(UIEvent.ActionBar, true);
        }

        public void SetCurrentSelectedItem(ItemDetails itemDetails)
        {
            currentSelectedItem = itemDetails;
        }

        public void SetCurrentSelectedItem(int index)
        {
            if (index < 0) { currentSelectedItem = null; }
            else
                currentSelectedItem = GetItemInBagByIndex(index);
        }

        public ItemDetails GetCurrentSelectedItem()
        {
            // Debug.Log(currentSelectedItem);
            // //需要判断这个物品还在仓库中存在不存在，不能直接返回
            // if (GetItemIndexInBag(currentSelectedItem.itemID) == -1)
            // {
            //     currentSelectedItem = null;
            // }
            return currentSelectedItem;
        }
    }

}
