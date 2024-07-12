using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    public class Item : MonoBehaviour
    {
        public int itemID;

        private SpriteRenderer spriteRenderer;
        private ItemDetails itemDetails;

        private BoxCollider2D coll;
        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            coll = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (itemID != 0)
            {
                Init(itemID);
                // Debug.Log(itemID);
            }
        }
        public void Init(int ID)
        {
            itemID = ID;
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
            if (itemDetails != null)
            {
                spriteRenderer.sprite = itemDetails.itemIconInWorld != null ? itemDetails.itemIconInWorld : itemDetails.itemIconInUI;
                // Debug.Log(itemDetails.itemIconInWorld);
                //adjust the collider size and offset, because the parent object's size is 1,1,1. so we need adjust the size due to the sprite size and offset
                Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
                coll.size = newSize;
                // Debug.Log(newSize); // 1
                // Debug.Log(spriteRenderer.sprite.bounds.center.y); // 0.5
                coll.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
            }
        }

        public ItemDetails GetItemDetails()
        {
            return itemDetails;
        }
    }
}

