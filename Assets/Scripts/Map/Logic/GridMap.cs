using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class GridMap : MonoBehaviour
{
    //通过这个拿到tilemap的数据，但是这些数据是没有整理过的，因为会非常大，而且有重复。所以需要再次进行整理（在GridManger里）
    public MapData_SO mapData;

    public GridType gridType;

    private Tilemap currentTilemap;

    private void OnEnable()
    {
        if (!Application.IsPlaying(this))
        {
            //每一层地图的tilemap组件，比如water，dig等等获得其组件
            currentTilemap = GetComponent<Tilemap>();
            if (mapData != null)
            {
                mapData.tilePropertiesList.Clear();
            }
        }
    }

    private void OnDisable()
    {
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();
            UpdateTileInfo();
#if UNITY_EDITOR
            if (mapData != null)
            {
                EditorUtility.SetDirty(mapData);
                Debug.Log($"data has been updated, the amount is {mapData.tilePropertiesList.Count} in the scene {mapData.sceneName}");
            }
#endif
        }
    }

    private void UpdateTileInfo()
    {
        //26
        currentTilemap.CompressBounds();
        if (!Application.IsPlaying(this))
        {
            if (mapData != null)
            {
                Vector3Int startPos = currentTilemap.cellBounds.min;
                Vector3Int endPos = currentTilemap.cellBounds.max;

                for (int x = startPos.x; x < endPos.x; x++)
                {
                    for (int y = startPos.y; y < endPos.y; y++)
                    {
                        TileBase tile = currentTilemap.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            TileProperty newTile = new TileProperty
                            {
                                tileCoordinate = new Vector2Int(x, y),
                                gridType = this.gridType,
                                boolTypeValue = true
                            };
                            mapData.tilePropertiesList.Add(newTile);
                        }
                    }
                }
            }
        }
    }
}
