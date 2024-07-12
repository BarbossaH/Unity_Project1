using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.Map;
using UnityEngine;
namespace MFarm.CropPlant
{
    public class CropGenerator : MonoBehaviour
    {
        //grid contains all tiles in current map
        private Grid currentGrid;
        public int seedItemID;
        public int growDays;

        private void Awake()
        {
            currentGrid = FindObjectOfType<Grid>();
            GenerateCrop();
        }
        private void OnEnable()
        {
            NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnGenerateCrop;

        }
        private void OnDisable()
        {
            NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnGenerateCrop;
        }

        private void OnGenerateCrop(SceneEvent sceneEvent, bool arg2, bool arg3)
        {
            if (sceneEvent == SceneEvent.GenerateCrop)
            {
                GenerateCrop();
            }
        }

        private void GenerateCrop()
        {
            Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);
            if (seedItemID != 0)
            {
                var tile = GridManager.Instance.GetTileDetailsOnMousePosition(cropGridPos);
                if (tile == null)
                {
                    tile = new TileDetails();
                }
                tile.daySinceWatered = -1;
                tile.seedItemID = seedItemID;
                tile.growDays = growDays;
                //generate tile details, then store it into the dictionary that manage the data of all tiles in current map
                GridManager.Instance.UpdateTileDetailsDic(tile);
            }
        }
    }
}