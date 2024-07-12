using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData_SO", menuName = "Map/MapData_SO")]
public class MapData_SO : ScriptableObject
{
    //这个数据存储了一个场景的名字和对应的瓦片列表信息
    public string sceneName;
    [Header("Map Info")]
    public int gridWidth;
    public int gridHeight;
    [Header("世界坐标系中左下角原点bottom-left origin")]

    //因为tile map绘制地图时，（0，0）点的位置是在地图的中心位置，而不是在左下角。
    public int originX;
    public int originY;
    public List<TileProperty> tilePropertiesList;
}