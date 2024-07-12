public enum ItemType
{
  Seed = 0, Commodity, Furniture,
  //tools
  HeoTool, ChopTool, BreakTool, ReapTool, WaterTool, CollectTool,
  //weeds
  ReapableScenery
};

public enum SlotType
{
  Bag = 0, Box, Shop
}

//it stands for the exact container, bag, action bar, or warehouse, shop, etc.
public enum UIEvent
{
  ActionBar = 0,
  Box,
  ClickSlot,
  MinuteAndHour,
  DayMonthYear
}

public enum SceneEvent
{
  CreateItemInScene = 0, //在场景中创建一个物品
  DropItemInScene, //
  Transition, //传送点发出传送事件，于是传送的manager就开始做各种操作
  MovePlayer,   //切换场景后移到角色到指定的位置
  BeforeLoadScene,//场景卸载之前，必要的数据清理和状态重置
  AfterLoadScene, //场景加载完成之后，状态的准备
  MouseClickEvent, //鼠标点击场景中的物品，进行互动
  AfterPlayerAnimation, //等player动画播完之后
  UpdateOneDay, //经过一天之后地块刷新
  PlantSeed,//种下种子
  InstantiateCropAtPlayer, //在角色头上生成农作物，代表收割到了
  RefreshMap, //刷新地图
  PlayParticleEffect, //
  GenerateCrop,
}

public enum ActionType
{
  None = 0, Carry, Hoe, Break, Water, Basket, Chop
}

public enum BodyPart
{
  Body, Hair, Arm, Tool
}

public enum Season
{
  Spring = 0, Summer, Autumn, Winter
}

public enum GridType
{
  CanDig, CanDrop, CanFish, PlaceFurniture, NPCObstacle,
}


public enum ParticleEffectType
{
  None = 0, LeaveFalling_01, LeaveFalling_02, Rock, ReapableScenery
}