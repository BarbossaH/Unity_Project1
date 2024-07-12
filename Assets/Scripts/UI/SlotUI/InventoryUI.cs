using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFarm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("tooltip")]
        public ItemTooltip itemTooltip;
        [Header("Drag picture")]
        public Image dragImage;
        [Header("Pack")]
        [SerializeField] private GameObject packUI;
        private bool isPackOpened;
        //this class is for managing all items in different inventories, because there are many type of inventory in this game. The variable below is assigned value by the lists of slots in player's backpack and action bar.
        [SerializeField] private SlotUI[] slotUIs;
        private void Start()
        {
            Init();
            isPackOpened = packUI.activeInHierarchy;
        }
        private void Init()
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                slotUIs[i].slotIndex = i;
            }
            RefreshInventoryUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                OnClickPackButton();
            }
        }
        private void OnEnable()
        {
            NotifyCenter<UIEvent, bool, float>.notifyCenter += OnDataChange;
        }

        private void OnDisable()
        {
            NotifyCenter<UIEvent, bool, float>.notifyCenter += OnDataChange;
        }

        private void OnDataChange(UIEvent location, bool arg2, float arg3)
        {
            // Debug.Log(location);
            // Debug.Log(list);
            switch (location)
            {
                case UIEvent.ActionBar:
                    RefreshInventoryUI();
                    // for (int i = 0; i < slotUIs.Length; i++)
                    // {
                    //     if (list[i].itemAmount > 0)
                    //     {
                    //         var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                    //         //update slot in acton bar and player's backpack
                    //         slotUIs[i].UpdateSlot(item, list[i].itemAmount);
                    //     }
                    //     else
                    //     {
                    //         slotUIs[i].CleanSlot();
                    //     }

                    // }
                    break;
                case UIEvent.Box:
                    break;
            }
        }

        private void RefreshInventoryUI()
        {
            var list = InventoryManager.Instance.playerBag_SO.itemsInBag;
            for (int i = 0; i < slotUIs.Length; i++)
            {
                if (list[i].itemAmount > 0)
                {
                    var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                    //update slot in acton bar and player's backpack
                    slotUIs[i].UpdateSlot(item, list[i].itemAmount);
                }
                else
                {
                    slotUIs[i].CleanSlot();
                }

            }
        }
        public void OnClickPackButton()
        {
            isPackOpened = !isPackOpened;
            packUI.SetActive(isPackOpened);
        }

        public void UpdateSlotHight(int index, bool isSelected)
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                slotUIs[i].InitSlotUISelected();
                if (slotUIs[i].slotIndex == index)
                {
                    slotUIs[i].SetHighlight(isSelected);
                }
            }
        }
    }
}

