using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ActionBarBtn : MonoBehaviour
    {
        public KeyCode keyCode;

        private SlotUI slotUI;

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(keyCode))
            {
                if (slotUI.itemAmount > 0)
                {
                    slotUI.isSelected = !slotUI.isSelected;
                    // if (slotUI.isSelected)
                    InventoryManager.Instance.SetCurrentSelectedItem(slotUI.isSelected ? slotUI.slotIndex : -1);
                    slotUI.inventoryUI.UpdateSlotHight(slotUI.slotIndex, slotUI.isSelected);
                    NotifyCenter<UIEvent, bool, bool>.NotifyObservers(UIEvent.ClickSlot, true, true);
                }
            }
        }
    }
}