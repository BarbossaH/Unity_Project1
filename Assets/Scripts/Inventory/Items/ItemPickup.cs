using System.Collections;
using System.Collections.Generic;
using MFarm.Inventory;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Item item = other.GetComponent<Item>();
        // Debug.Log(item.name);
        //check the component existing
        if (item != null)
        {
            // Debug.Log(item.GetItemDetails());
            if (item.GetItemDetails().canPickedUp)
            {
                // Debug.Log(item.GetItemDetails());
                // pick up the item
                InventoryManager.Instance.AddItem(item, true);
            }
        }
    }
}
