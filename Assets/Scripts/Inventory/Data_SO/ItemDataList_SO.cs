using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataList_SO", menuName = "Inventory/ItemDataList_SO")]
public class ItemDataList_SO : ScriptableObject
{
    //Don't change the variable name easily, once changed, the data will lose

    public List<ItemDetails> itemDetailsList;
}

//this is for the data of all items