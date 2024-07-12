using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropManager : Singleton<CropManager>
{
    //this class is useless in Unity

    // public CropDetailsList_SO cropDetailsList_SO;
    // private Transform currentSceneCropParent; //create a parent game object for each crop object created

    // private void OnEnable()
    // {
    //     NotifyCenter<SceneEvent, int, TileDetails>.notifyCenter += OnPlantSeed;
    //     NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnSceneChange;
    // }

    // private void OnDisable()
    // {
    //     NotifyCenter<SceneEvent, int, TileDetails>.notifyCenter -= OnPlantSeed;
    //     NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnSceneChange;

    // }
    // private void OnSceneChange(SceneEvent sceneEvent, bool arg2, bool arg3)
    // {
    //     if (sceneEvent == SceneEvent.AfterLoadScene)
    //     {
    //         // Debug.Log(123);
    //         currentSceneCropParent = GameObject.FindWithTag("CropParent").transform;
    //     }
    // }
    // private void OnPlantSeed(SceneEvent sceneEvent, int seedItemID, TileDetails currentTileDetails)
    // {
    //     //grid manager 传递过来格子的信息，这里要做的是修改格子的信息，从而实现种植的效果
    //     if (sceneEvent == SceneEvent.PlantSeed)
    //     {
    //         CropDetails cropDetails = GetCropDetails(seedItemID);
    //         if (cropDetails != null && IsSeasonable(cropDetails) && currentTileDetails.seedItemID == -1)
    //         {
    //             //我认为在这里判断地里是否有种子已经迟了，因为已经播放了种植的动画，应该在角色哪里去判断
    //             currentTileDetails.seedItemID = seedItemID;
    //             currentTileDetails.growDays = 0;
    //             //显示农作物
    //             DisplayCropPlant(currentTileDetails, cropDetails);
    //         }
    //         else if (currentTileDetails.seedItemID != -1)
    //         {
    //             //show the crop
    //             DisplayCropPlant(currentTileDetails, cropDetails);

    //         }
    //     }
    // }

    // private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
    // {
    //     //通过这个数组的长度得出阶段的数量
    //     int cropGrowthStageCount = cropDetails.growthDaysInEachStage.Length;
    //     int currentStage = 0;
    //     int dayCount = cropDetails.TotalGrowthDays;
    //     for (int i = cropGrowthStageCount - 1; i > 0; i--)
    //     {
    //         //先判断是不是第五个阶段，如果是就直接跳出循环，已经成长的天数和每个阶段的总天数进行比较
    //         if (tileDetails.growDays >= dayCount)
    //         {
    //             currentStage = i;
    //             break;
    //         }
    //         dayCount -= cropDetails.growthDaysInEachStage[i];
    //     }
    //     //根据当前的阶段，显示对应的prefab
    //     GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];
    //     Sprite cropSprite = cropDetails.growthSpriteForItem[currentStage];
    //     // Vector3 pos = new Vector3(tileDetails.gridCoordinate.x+0.5f)
    // }

    // public CropDetails GetCropDetails(int seedID)
    // {
    //     return cropDetailsList_SO.cropDetailsList.Find(c => c.seedID == seedID);
    // }
    // private bool IsSeasonable(CropDetails cropDetails)
    // {
    //     var currentSeason = TimeManager.Instance.GetCurrentSeason();
    //     for (int i = 0; i < cropDetails.plantableSeason.Length; i++)
    //     {
    //         if (cropDetails.plantableSeason[i] == currentSeason)
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }
}
