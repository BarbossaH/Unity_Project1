using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFarm.Inventory
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Components")]
        //one slot consists of the following four attributes.
        [SerializeField] private Image slotImg;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image slotHeightLight;
        [SerializeField] private Button button;

        [Header("Slot Type")]
        //because a slot can locate in different type UI, such as bag, shop, box, and so on. In different containers, there are many different way to deal with.
        public SlotType slotType;
        public bool isSelected;
        public int slotIndex;
        public int itemAmount;

        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        private void Start()
        {
            isSelected = false;
            // CleanSlot();
        }
        public void UpdateSlot(ItemDetails item, int amount)
        {
            // Debug.Log(item);
            //when this slot is set with items, its function should be activated.
            slotImg.sprite = item.itemIconInUI;
            slotImg.enabled = true;
            itemAmount = amount;
            amountText.text = amount.ToString();
            button.interactable = true;
        }

        public void CleanSlot()
        {
            //when slot doesn't contain any items, it should be cleared, canceling the selected state, hide the images, clear the text content, and disable the button
            if (isSelected)
            {
                isSelected = false;
            }

            slotImg.enabled = false; //hide the image
            amountText.text = string.Empty;
            button.interactable = false;
            SetHighlight(false);
        }

        public void InitSlotUISelected()
        {
            isSelected = false;
            SetHighlight(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // if (itemAmount <= 0) return;

            ItemDetails itemDetails = InventoryManager.Instance.GetItemInBagByIndex(slotIndex);
            if (itemDetails == null) return;
            isSelected = !isSelected;
            // Debug.Log(isSelected);
            inventoryUI.UpdateSlotHight(slotIndex, isSelected);
            // Debug.Log(itemDetails);
            if (slotType == SlotType.Bag && itemDetails != null)
            {
                InventoryManager.Instance.SetCurrentSelectedItem(isSelected ? itemDetails : null);
                NotifyCenter<UIEvent, bool, bool>.NotifyObservers(UIEvent.ClickSlot, true, true);
            }
        }

        public void SetHighlight(bool isHighlight)
        {
            slotHeightLight.gameObject.SetActive(isHighlight);
            isSelected = isHighlight;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemAmount > 0)
            {
                inventoryUI.dragImage.enabled = true;
                inventoryUI.dragImage.sprite = slotImg.sprite;
                inventoryUI.dragImage.SetNativeSize();
                isSelected = true;
                inventoryUI.UpdateSlotHight(slotIndex, isSelected);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragImage.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragImage.enabled = false;
            // Debug.Log(eventData.pointerCurrentRaycast.gameObject);
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                {
                    return;
                }
                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                int targetIndex = targetSlot.slotIndex;
                //
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }
                // inventoryUI.UpdateSlotHight(targetIndex);
                inventoryUI.UpdateSlotHight(-1, isSelected);
            }
            // else
            // {
            //     //扔在地上
            //     if (itemDetails.canDropped)
            //     {
            //         var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            //         NotifyCenter<SceneEvent, int, Vector3>.NotifyObservers(SceneEvent.CreateItemInScene, itemDetails.itemID, pos);

            //     }
            // }
        }
    }

}
