using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MFarm.Map
{
    /*
    这个类是操作当前场景的格子的类，所以保留当前格子的数据似乎没有必要，应该他会一直改变，所有也就没有必要使用单例模式
    */
    //这个类目前主要是反馈鼠标滑过格子的信息 
    public class GridManager : Singleton<GridManager>
    {
        [Header("MapInfo")]
        public List<MapData_SO> mapDataList;
        private Grid currentSceneGrid;
        private Dictionary<string, TileDetails> tileDetailsDic = new Dictionary<string, TileDetails>();
        private Dictionary<string, bool> firstLoadDict = new Dictionary<string, bool>();
        [Header("SetTile")]
        //assign the created rule tile to them
        public RuleTile digRuleTile;
        public RuleTile waterRuleTile;

        private Tilemap digTileMap;
        private Tilemap waterTileMap;
        private Season currentSeason;

        [Header("Crops")]
        public CropDetailsList_SO cropDetailsList_SO;
        private Transform currentSceneCropParent; //create a parent game object for each crop object created

        private void OnEnable()
        {
            NotifyCenter<SceneEvent, Vector3, ItemDetails>.notifyCenter += OnAfterPlayerAnimation;
            NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnSceneChange;
            NotifyCenter<SceneEvent, int, Season>.notifyCenter += OnUpdateDay;
            NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnRefreshMap;
        }

        private void OnDisable()
        {
            NotifyCenter<SceneEvent, Vector3, ItemDetails>.notifyCenter -= OnAfterPlayerAnimation;
            NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnSceneChange;
            NotifyCenter<SceneEvent, int, Season>.notifyCenter -= OnUpdateDay;
            NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnRefreshMap;

        }

        private void OnRefreshMap(SceneEvent sceneEvent, bool arg2, bool arg3)
        {
            if (sceneEvent == SceneEvent.RefreshMap)
            {
                RefreshMap();
            }
        }

        private void OnUpdateDay(SceneEvent sceneEvent, int gameDay, Season gameSeason)
        {
            if (sceneEvent == SceneEvent.UpdateOneDay)
            {
                currentSeason = gameSeason;
                foreach (var tile in tileDetailsDic)
                {
                    if (tile.Value.daySinceWatered > -1)
                    {
                        //if this tile was watered, after a day, then set it to -1, which means dry
                        tile.Value.daySinceWatered = -1;
                    }
                    if (tile.Value.daySinceDig > -1)
                    {
                        tile.Value.daySinceDig++;
                        //如果挖坑超过一定数量可以恢复，比如超过3天就恢复了
                        if (tile.Value.daySinceDig > 1 && tile.Value.seedItemID == -1)
                        {
                            tile.Value.daySinceDig = -1;
                            tile.Value.canDig = true;
                            tile.Value.growDays = -1;
                        }
                    }
                    //如果这里种了东西，种植的日期就增加一天
                    if (tile.Value.seedItemID != -1)
                    {
                        tile.Value.growDays++;
                    }
                }
                RefreshMap();
            }
        }

        private void Start()
        {
            foreach (var mapData in mapDataList)
            {
                firstLoadDict.Add(mapData.sceneName, true);
                InitTileDetailDic(mapData);
            }
        }

        private void InitTileDetailDic(MapData_SO mapData)
        {
            //初始化格子的数据，将在tilemap编辑器生成的数据读取出来，放到字典里tileDetailsDic，这个字典存入了所有地图的数据，可以根据当前场景获得当前场景的地图数据。这里的初始化是在游戏还没有存储数据的时候，实际上当有数据的时候，这里的初始化应该从本地文件或者服务器中获得地图数据。如果对地图有操作，比如种下种子，或者开始成长就更新这里面的数据。比如如果一个地方在没有种地之前，可以被丢东西，甚至挖了也可以被丢东西，但是一旦种下种子和后面在成长，这个地方就可以被丢弃物品，直到收割并销毁种下的对象
            foreach (TileProperty tileProperty in mapData.tilePropertiesList)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridCoordinate = tileProperty.tileCoordinate,
                };

                //using the map name and coordinate as its key to ensure it's unique.
                // string key = mapData.sceneName + tileDetails.gridCoordinate.x + tileDetails.gridCoordinate.y;
                string key = GenerateKey(mapData.sceneName, tileDetails.gridCoordinate);
                //如果字典里有数据，那么先取出来赋值，只修改会被修改的部分，GridType，因为经常会修改编辑地图
                if (GetTileDetails(key) != null)
                {
                    tileDetails = GetTileDetails(key);
                }

                switch (tileProperty.gridType)
                {
                    case GridType.CanDig:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GridType.CanDrop:
                        tileDetails.canDropItem = true;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = true;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = true;
                        break;
                }

                if (GetTileDetails(key) != null)
                {
                    tileDetailsDic[key] = tileDetails;
                }
                else
                {
                    tileDetailsDic.Add(key, tileDetails);
                }
            }
        }

        private TileDetails GetTileDetails(string key)
        {
            //因为不是所有的地图格子都会存入这个字典的，所有当鼠标移到一些不可交互的区域时，就从字典里找不到
            if (tileDetailsDic.ContainsKey(key))
            {
                return tileDetailsDic[key];
            }
            return null;
        }

        private string GenerateKey(string sceneName, Vector2Int coordinate)
        {
            return sceneName + coordinate.x + coordinate.y;
        }

        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mousePos)
        {
            string key = GenerateKey(SceneManager.GetActiveScene().name, (Vector2Int)mousePos);
            return GetTileDetails(key);
        }
        public TileDetails GetTileDetailsByWorldPosition(Vector3Int worldPos)
        {
            string key = GenerateKey(SceneManager.GetActiveScene().name, (Vector2Int)worldPos);
            return GetTileDetails(key);
        }

        #region events
        private void OnSceneChange(SceneEvent sceneEvent, bool arg2, bool arg3)
        {
            if (sceneEvent == SceneEvent.AfterLoadScene)
            {
                //获取当前场景的格子
                currentSceneGrid = FindObjectOfType<Grid>();
                // Debug.Log(currentSceneGrid.name);
                //获取可以作为收获作物的父对象
                currentSceneCropParent = GameObject.FindWithTag("CropParent").transform;
                //获取当前场景种地的格子和浇水的格子 Retrieve the tile map for planting and watering in the current scene
                InitManipulatedTileMap();

                if (firstLoadDict[SceneManager.GetActiveScene().name])
                {
                    // Debug.Log(SceneManager.GetActiveScene().name);
                    //because the next scene could not have the crop object
                    Crop crop = FindFirstObjectByType<Crop>();
                    if (crop != null)
                        NotifyCenter<SceneEvent, bool, bool>.NotifyObservers(SceneEvent.GenerateCrop, true, true);
                    firstLoadDict[SceneManager.GetActiveScene().name] = false;
                }
                RefreshMap();
                // DisplayCurrentMapTiles(SceneManager.GetActiveScene().name);
            }
        }


        private void InitManipulatedTileMap()
        {
            //after the scene switch, initialize the tile maps can be manipulated
            digTileMap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            // Debug.Log(digTileMap.name + "who is it");
            waterTileMap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();

        }
        private void OnAfterPlayerAnimation(SceneEvent sceneEvent, Vector3 mouseWorldPos, ItemDetails seedAndTool)
        {
            if (sceneEvent == SceneEvent.AfterPlayerAnimation)
            {
                var mouseGridPos = currentSceneGrid.WorldToCell(mouseWorldPos);
                var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);
                if (currentTile != null)
                {
                    switch (seedAndTool.itemType)
                    {
                        case ItemType.Seed:
                            //从数据库看看拿到庄稼的信息
                            CropDetails cropDetails = GetCropDetails(seedAndTool.itemID);
                            if (cropDetails != null && IsSeasonable(cropDetails) && currentTile.seedItemID == -1)
                            {
                                //我认为在这里判断地里是否有种子已经迟了，因为已经播放了种植的动画，应该在角色哪里去判断
                                currentTile.seedItemID = seedAndTool.itemID;
                                currentTile.growDays = 0;
                                //显示农作物
                                DisplayCropPlant(currentTile, cropDetails);
                            }
                            else if (currentTile.seedItemID != -1)
                            {
                                //show the crop
                                DisplayCropPlant(currentTile, cropDetails);
                            }
                            InventoryManager.Instance.RemoveItem(seedAndTool.itemID);
                            // NotifyCenter<SceneEvent, int, TileDetails>.NotifyObservers(SceneEvent.PlantSeed, details.itemID, currentTile);
                            break;
                        case ItemType.Commodity:
                            //生成一个商品
                            //这里应该直接删除背包的数据，然后发出丢东西的通知，丢东西的数据不应该被带走
                            InventoryManager.Instance.RemoveItem(seedAndTool.itemID);
                            // NotifyCenter<SceneEvent, int, Vector3>.NotifyObservers(SceneEvent.CreateItemInScene, details.itemID, mouseWorldPos);
                            NotifyCenter<SceneEvent, int, Vector3>.NotifyObservers(SceneEvent.DropItemInScene, seedAndTool.itemID, mouseWorldPos);
                            break;
                        case ItemType.HeoTool:
                            SetDigGround(currentTile);
                            currentTile.daySinceDig = 0;
                            currentTile.canDig = false;
                            currentTile.canDropItem = false;
                            //add audio effect
                            break;
                        case ItemType.WaterTool:
                            SetWaterGround(currentTile);
                            currentTile.daySinceWatered = 0;
                            break;
                        case ItemType.ChopTool:
                        case ItemType.BreakTool:
                        case ItemType.CollectTool:
                            Crop currentCrop = GetCropObject(mouseWorldPos);
                            if (currentCrop != null)
                            {
                                //收割农作物
                                currentCrop.ProcessToolAction(seedAndTool, currentTile);
                                // Debug.Log(currentCrop.cropDetails.seedID);
                            }
                            break;
                    }
                }
                UpdateTileDetailsDic(currentTile);
            }
        }
        #endregion
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int tilePos = new Vector3Int(tile.gridCoordinate.x, tile.gridCoordinate.y, 0);

            //判断一下有没有可以挖的格子 check if there are a grid game object that can be dug.
            if (digTileMap != null)
            {
                //only need to set the rule tiles to a position
                digTileMap.SetTile(tilePos, digRuleTile);

            }
        }
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int tilePos = new Vector3Int(tile.gridCoordinate.x, tile.gridCoordinate.y, 0);
            if (waterTileMap != null)
            {
                //only need to set the rule tiles to a position
                waterTileMap.SetTile(tilePos, waterRuleTile);
            }
        }

        //update grid dictionary to store the tile map information
        public void UpdateTileDetailsDic(TileDetails tileDetails)
        {
            string key = GenerateKey(SceneManager.GetActiveScene().name, tileDetails.gridCoordinate);
            if (tileDetailsDic.ContainsKey(key))
            {
                tileDetailsDic[key] = tileDetails;
            }
            else
            {
                tileDetailsDic.Add(key, tileDetails);
            }
        }

        // 
        private void RefreshMap()
        {
            //to ensure every tilemap has a game object.
            if (digTileMap != null)
            {
                digTileMap.ClearAllTiles();
            }
            if (waterTileMap != null)
            {
                waterTileMap.ClearAllTiles();
            }

            //delete all crop game objects
            foreach (var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }
            //display the game objects based on the tile map data tileDetailsDic
            DisplayCurrentMapTiles(SceneManager.GetActiveScene().name);
        }

        private void DisplayCurrentMapTiles(string sceneName)
        {
            //based on the current scene, set each tilemap with tileDetails
            foreach (var tile in tileDetailsDic)
            {
                var key = tile.Key;
                var tileDetails = tile.Value;

                if (key.Contains(sceneName))
                {
                    //display the tile data in the current scene
                    if (tileDetails.daySinceDig > -1)
                    {
                        //display dig layer
                        SetDigGround(tileDetails);
                    }
                    if (tileDetails.daySinceWatered > -1)
                    {
                        //display water layer
                        SetWaterGround(tileDetails);
                    }
                    if (tileDetails.seedItemID > -1)
                    {
                        //display crop game object, they are game objects not tile objects
                        CropDetails cropDetails = GetCropDetails(tileDetails.seedItemID);
                        // Debug.Log(tileDetails.seedItemID);
                        // Debug.Log(cropDetails);
                        DisplayCropPlant(tileDetails, cropDetails);
                    }
                }
            }
        }
        //------------------------------Crops----------------------------------
        /*我认为庄稼应该写在这个类里，因为绝大多数庄稼是与格子互动，修改格子的数据，同样的数据不要来回传递增加维护成本，很容易复制数据，而且一旦被无意修改了数据都很难跟踪和找到*/
        public CropDetails GetCropDetails(int seedID)
        {
            return cropDetailsList_SO.cropDetailsList.Find(c => c.seedID == seedID);
        }

        private bool IsSeasonable(CropDetails cropDetails)
        {
            var currentSeason = TimeManager.Instance.GetCurrentSeason();
            for (int i = 0; i < cropDetails.plantableSeason.Length; i++)
            {
                if (cropDetails.plantableSeason[i] == currentSeason)
                {
                    return true;
                }
            }
            return false;
        }

        private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
        {
            //通过这个数组的长度得出阶段的数量
            int cropGrowthStageCount = cropDetails.growthDaysInEachStage.Length;
            int currentStage = 0;
            int dayCount = cropDetails.TotalGrowthDays;
            //找到当前庄稼的阶段
            for (int i = cropGrowthStageCount - 1; i > 0; i--)
            {
                //先判断是不是第五个阶段，如果是就直接跳出循环，已经成长的天数和每个阶段的总天数进行比较
                if (tileDetails.growDays >= dayCount)
                {
                    currentStage = i;
                    break;
                }
                dayCount -= cropDetails.growthDaysInEachStage[i];
            }
            //根据当前的阶段，显示对应的prefab
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];
            Sprite cropSprite = cropDetails.growthSpriteForItem[currentStage];
            //因为这个坐标是tilmap自动生产的，是根据左下角的位置，所以加上0.5就是格子中间的位置，要将crop放到格子中间的位置

            Vector3 pos = new Vector3(tileDetails.gridCoordinate.x + 0.5f, tileDetails.gridCoordinate.y + 0.5f,
            0);
            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, currentSceneCropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            cropInstance.GetComponent<Crop>().SetCropDetails(cropDetails);
        }

        //通过鼠标点击的位置检测crop
        private Crop GetCropObject(Vector3 mouseWorldPos)
        {

            Collider2D[] colls = Physics2D.OverlapPointAll(mouseWorldPos);
            Crop currentCrop = null;
            foreach (var coll in colls)
            {
                //这个地方可以出现bug，如果是普通果实，一般都在格子里，但是如果是树，那么一定会超出格子。而当树的碰撞体没有被格子完全覆盖，即他们不重叠时，那么就拿不到树的信息，也就说，点击树高一点的位置是无效的。

                /*这里就要思考一下，如何得到种子的信息，因为树和其他农作物是不同的，树有树杆，所有砍伐树杆是合理的。我现在的设想是通过检测碰撞体，看看没有没有crop组件，如果有，那就看看crop的种类，通过种类和当前使用的工具去判断可以可不可以执行动作，地图数据只是来获取种子的信息，而检测就完全交给碰撞体*/
                if (coll.GetComponent<Crop>())
                {
                    currentCrop = coll.GetComponent<Crop>();
                    break;
                }
            }

            return currentCrop;
        }

        //从指定地图获得到地图格子的宽高和原点的位置，而这些数据都在编辑器中已经编辑好了，是程序员测量出来的。这里只能手动的输入，因为每张地图的格子是手动绘制的，所以没办法自动获取。注意：这里是整个地图的数据，也就说这里包含了很多不能行走的区域，甚至没有信息的格子，因为地图不一定是规则的。而这里的地图尺寸则是矩形的。
        /*Get the width, height, and origin position of the map grid from the specified map. These data are already edited in the editor and measured by programmer. The input must be done manually because each map's grid is drawn manually, so it cannot be obtained automatically. Note: This refers to the data of the entire map, including many node-walkable areas and even cells without any information, as the map is not necessarily regular. However, the map size here is rectangular.*/
        public bool GetGridDimensions(string sceneName, out Vector2Int mapSize, out Vector2Int gridOrigin)
        {
            mapSize = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            foreach (var mapData in mapDataList)
            {
                if (mapData.sceneName == sceneName)
                {
                    //每个地图的数据已经被编辑在editor里了，所以这里就是要取出来，并且传出去。
                    //The data for each map has been already edited in Unity Editor, so the task here is to retrieve this data and pass it out
                    mapSize.x = mapData.gridWidth;
                    mapSize.y = mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;
                    return true;
                }
            }
            return false;
        }
    }

}