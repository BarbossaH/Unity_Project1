

using System;
using UnityEngine;
[Serializable]
public class ItemDetails
{
    //this is the data structure of the item
    public int itemID;
    public string itemName;

    public ItemType itemType;
    public string description;
    public Sprite itemIconInUI;
    public Sprite itemIconInWorld;

    // The range of items that can be used, for example an axe can chop trees within a range of 2 grid squares.
    public int itemUsedRadius;

    public bool canPickedUp;
    public bool canDropped;
    public bool canCarried;

    public float itemPrice;

    [Range(0, 1)]
    public float soldPercentage;
}

//The following data type is intended for items in the bag, as there is no need to record every attribute of an item, only the necessary one should be set.
[Serializable]
public struct InventoryItem
{
    //this is data details in the inventory including the pack and action bar
    public int itemID;
    public int itemAmount;

    public InventoryItem(int itemID, int amount)
    {
        this.itemID = itemID;
        this.itemAmount = amount;
    }
}

[Serializable]
public class AnimatorType
{
    public ActionType actionType;
    public BodyPart bodyPart;
    public AnimatorOverrideController animatorOverrideController;
}

// [Serializable]
// public class MinuteAndHour
// {
//     public int gameMinute;
//     public int gameHour;

//     public MinuteAndHour(int gameMinute, int gameHour)
//     {
//         this.gameMinute = gameMinute;
//         this.gameHour = gameHour;
//     }
// }
// [Serializable]
// public class DayMonthYear
// {
//     public int gameHour;
//     public int gameDate;
//     public int gameMonth;
//     public int gameYear;
//     public Season season;

//     public DayMonthYear(int gameHour, int gameDate, int gameMonth, int gameYear, Season season)
//     {
//         this.gameHour = gameHour;
//         this.gameDate = gameDate;
//         this.gameMonth = gameMonth;
//         this.gameYear = gameYear;
//         this.season = season;
//     }
// }

[Serializable]
public class SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 position)
    {
        this.x = position.x;
        this.y = position.y;
        this.z = position.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

[Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}

[Serializable]
public class TileProperty
{
    public Vector2Int tileCoordinate;//该tile的坐标
    public GridType gridType; //每一层layer的枚举，代表这个瓦片属于哪一层tilemap
    public bool boolTypeValue;
}

[Serializable]
public class TileDetails
{
    //每个格子会存储的基础数据
    public Vector2Int gridCoordinate;
    public bool canDig;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool isNPCObstacle;

    //被挖了多少天了
    public int daySinceDig = -1;
    //浇水
    public int daySinceWatered = -1;
    //种子
    public int seedItemID = -1;
    //成长了多少天了
    public int growDays = -1;
    //是否可以反复收割,上次收割过了多少天
    public int daySinceLastHarvest = -1;
}

[Serializable]
public class NPCPosition
{
    public Transform transform;
    public string startScene;
    public Vector3 position; // ?是世界坐标还是格子坐标体系？
}