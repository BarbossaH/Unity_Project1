using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour
{
    public Item itemPrefab;
    public Item castingPrefab;
    Transform playerTransform => FindObjectOfType<Player>().transform;

    private Transform itemParent;
    private Dictionary<string, List<SceneItem>> sceneItemDict = new Dictionary<string, List<SceneItem>>();

    // private void Start()
    // {
    //     itemParent = GameObject.FindWithTag("ItemParent").transform;
    // }
    private void OnEnable()
    {
        NotifyCenter<SceneEvent, int, Vector3>.notifyCenter += OnSceneChange;
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter += FindItemParent;

    }
    private void OnDisable()
    {
        NotifyCenter<SceneEvent, int, Vector3>.notifyCenter -= OnSceneChange;
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= FindItemParent;

    }
    // private void Update()
    // {
    //     Debug.Log(playerTransform);
    // }
    private void FindItemParent(SceneEvent sceneEvent, bool arg2, bool arg3)
    {
        if (sceneEvent == SceneEvent.BeforeLoadScene)
        {
            GetAllItemsInScene();
        }
        else if (sceneEvent == SceneEvent.AfterLoadScene)
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;
            CreateItemFromDictionary();
        }
    }

    private void OnSceneChange(SceneEvent scentEvent, int ID, Vector3 mousePos)
    {

        //需要研究一下
        if (scentEvent == SceneEvent.CreateItemInScene)
        {
            // var item = Instantiate(itemPrefab, mousePos, Quaternion.identity, itemParent);
            var item = Instantiate(castingPrefab, mousePos, Quaternion.identity, itemParent);
            item.itemID = ID;
            item.GetComponent<ItemCasting>().InitCastingItem(mousePos, Vector3.up);

        }
        else if (scentEvent == SceneEvent.DropItemInScene)
        {
            // Debug.Log(456);

            var item = Instantiate(castingPrefab, playerTransform.position, Quaternion.identity, itemParent);
            item.itemID = ID;
            // Debug.Log(mousePos);
            // Debug.Log(playerTransform.position);
            var dir = (mousePos - playerTransform.position).normalized;
            item.GetComponent<ItemCasting>().InitCastingItem(mousePos, dir);
        }

    }


    private void GetAllItemsInScene()
    {
        // Debug.Log(1);
        List<SceneItem> currentSceneItemsList = new List<SceneItem>();
        foreach (var item in FindObjectsOfType<Item>())
        {
            SceneItem sceneItem = new SceneItem
            {
                itemID = item.itemID,
                position = new SerializableVector3(item.transform.position)
            };
            currentSceneItemsList.Add(sceneItem);
        }
        if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
        {
            sceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItemsList;
        }
        else
        {
            sceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItemsList);
        }
    }

    private void CreateItemFromDictionary()
    {
        // Debug.Log(2);
        List<SceneItem> itemsList = new List<SceneItem>();
        //check if the scene exists items
        if (sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name, out itemsList))
        {
            if (itemsList != null)
            {
                //perhaps it should be done when unload the scene to release the memory.
                foreach (var item in FindObjectsOfType<Item>())
                {
                    Destroy(item.gameObject);
                }

                foreach (var item in itemsList)
                {
                    Item newItem = Instantiate(itemPrefab, item.position.ToVector3(), Quaternion.identity, itemParent);
                    newItem.Init(item.itemID);
                }
            }
        }
    }
}
