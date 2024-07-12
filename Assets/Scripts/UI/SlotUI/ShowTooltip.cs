using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private SlotUI slotUI;
        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotUI.itemAmount > 0)
            {
                int index = slotUI.slotIndex;
                ItemDetails itemDetails = InventoryManager.Instance.GetItemInBagByIndex(index);
                if (itemDetails != null)
                {
                    inventoryUI.itemTooltip.gameObject.SetActive(true);
                    inventoryUI.itemTooltip.SetTooltip(itemDetails, slotUI.slotType);
                    inventoryUI.itemTooltip.transform.position = transform.position + Vector3.up * 50;
                }

            }
            else
            {
                inventoryUI.itemTooltip.gameObject.SetActive(false);
            }
            // LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemTooltip.gameObject.SetActive(false);

        }

        private string GetItemType(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Seed => "种子",
                ItemType.ChopTool => "工具",
                ItemType.CollectTool => "工具",
                _ => "nothing" //default
            };
        }
    }

}
