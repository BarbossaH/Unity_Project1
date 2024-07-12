
using System;
using UnityEngine;

[Serializable]
public class CropDetails
{
    public int seedID;

    //each stage seed has a number of day of growth, and after completing a certain number of days of growth, it moves on to the next stage
    public int[] growthDaysInEachStage;
    public int TotalGrowthDays
    {
        //表示总共要长多少天
        get
        {
            int amount = 0;
            foreach (int days in growthDaysInEachStage)
            {
                amount += days;
            }
            return amount;
        }
    }

    //in different state, we instantiate different game objects with the corresponding prefab
    public GameObject[] growthPrefabs;

    //because the item prefab needs a image( but the tree doesn't need them, check the prefab in Editor can know this)
    public Sprite[] growthSpriteForItem;

    //plantable season
    public Season[] plantableSeason;

    public int[] harvestToolID;
    //需要使用多少次工具才能收获，比如土豆就一次，砍树就得多次
    /*how many times do you need to use a tool to harvest, eg. once for potatoes, multiple times for cutting down a tree.*/
    public int[] harvestActionCount;

    //after harvest, the item transforms to another item.
    [Header("Transformed Item")]
    public int transformedItemID;

    //the items produced
    public int[] producedItemID;
    public int[] min_producedAccount;
    public int[] max_producedAccount;

    public Vector2 spawnRadius;

    // How many days it takes to regrow and how many times it can grow again
    public int daysToRegrow;
    public int regrowTimes;

    public bool isHeldByPlayer;
    public bool hasAnimation;
    public bool hasParticleEffect;
    //todo: effect type
    public ParticleEffectType effectType;
    public Vector3 effectPos;
    public bool CheckToolAvailable(int toolID)
    {
        foreach (var tId in harvestToolID)
        {
            if (toolID == tId)
            {
                return true;
            }
        }
        return false;
    }

    public int GetOperateCount(int toolID)
    {
        for (int i = 0; i < harvestToolID.Length; i++)
        {
            if (harvestToolID[i] == toolID)
            {
                return harvestActionCount[i];
            }
        }
        return -1;
    }
}
//this class is for designing data structures