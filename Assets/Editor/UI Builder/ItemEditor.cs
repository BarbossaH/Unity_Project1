using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ItemEditor : EditorWindow
{
    private ItemDataList_SO dataBase;
    private List<ItemDetails> itemList = new List<ItemDetails>();
    //item template for item list
    private VisualTreeAsset itemRowTemplate;
    //Items' list
    private ListView itemListView;
    //details of an item
    private ScrollView itemDetailSection;
    //selected item details
    private ItemDetails selectedItemDetails;
    //for icon
    private Sprite defaultIcon;
    private VisualElement iconPreview;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Will Studio/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        //get template data
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemRowTemplate.uxml");

        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_M.png");

        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");

        //the window on the right side is a scrollView type.
        itemDetailSection = root.Q<ScrollView>("ItemDetails");
        // the icon in the scrollView window
        iconPreview = root.Q<VisualElement>("Icon");

        //add button
        root.Q<Button>("AddItem").clicked += OnAddItemClicked;
        //delete button
        root.Q<Button>("DeleteItem").clicked += OnDeleteItemClicked;

        //加载数据
        LoadDataBase();
        GenerateListView();
    }

    private void OnDeleteItemClicked()
    {
        itemList.Remove(selectedItemDetails);
        itemListView.Rebuild();
        itemDetailSection.visible = false;
    }

    private void OnAddItemClicked()
    {
        ItemDetails newItem = new ItemDetails();
        newItem.itemName = "New One";
        newItem.itemID = 10000 + itemList.Count;
        itemList.Add(newItem);
        itemListView.Rebuild();
    }

    private void LoadDataBase()
    {
        var dataArray = AssetDatabase.FindAssets("ItemDataList_SO");
        // Debug.Log(dataArray.Length);
        if (dataArray.Length > 1)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            dataBase = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataList_SO)) as ItemDataList_SO;
        }
        //store the data
        itemList = dataBase.itemDetailsList;
        // itemList = dataBase.testList;
        // //如果不标记则无法存储数据
        EditorUtility.SetDirty(dataBase);
        // Debug.Log(itemList[0].Id);
    }

    private void GenerateListView()
    {
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if (i < itemList.Count)
            {
                if (itemList[i].itemIconInUI != null)
                {
                    e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIconInUI.texture;
                }
                e.Q<Label>("Name").text = itemList[i] == null ? "No Item" : itemList[i].itemName;

            }
        };
        // itemListView.fixedItemHeight = 60;
        //when generate the listView, store all data into the itemListView.itemSource,which also means there are many member variables in the listView.
        itemListView.itemsSource = itemList;
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;

        itemListView.selectionChanged += OnListSelectionChange;
        itemDetailSection.visible = false;
    }

    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        //remember the First() function
        selectedItemDetails = selectedItem.First() as ItemDetails;
        // selectedItemDetails = (ItemDetails)selectedItem.First();
        GetItemDetails();
        itemDetailSection.visible = true;
    }

    private void GetItemDetails()
    {
        itemDetailSection.MarkDirtyRepaint();
        //1.itemID
        itemDetailSection.Q<IntegerField>("ItemID").value = selectedItemDetails.itemID;
        itemDetailSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.itemID = evt.newValue;
        });
        //2.item name
        itemDetailSection.Q<TextField>("ItemName").value = selectedItemDetails.itemName;
        itemDetailSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.itemName = evt.newValue;
            itemListView.Rebuild();
        });
        //3. icon

        iconPreview.style.backgroundImage = selectedItemDetails.itemIconInUI == null ? defaultIcon.texture
        : selectedItemDetails.itemIconInUI.texture;
        // Debug.Log(selectedItemDetails.itemIconInUI);
        // Debug.Log(itemDetailSection.Q<ObjectField>("ItemIcon"));
        itemDetailSection.Q<ObjectField>("ItemIcon").value = selectedItemDetails.itemIconInUI;
        itemDetailSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            Sprite newIcon = evt.newValue as Sprite;
            if (newIcon == null) newIcon = defaultIcon;
            selectedItemDetails.itemIconInUI = newIcon;
            iconPreview.style.backgroundImage = newIcon.texture;
            itemListView.Rebuild();

        });
        //4. icon in world
        itemDetailSection.Q<ObjectField>("ItemWorldIcon").value = selectedItemDetails.itemIconInWorld;
        itemDetailSection.Q<ObjectField>("ItemWorldIcon").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.itemIconInWorld = evt.newValue as Sprite;
        });
        //5.int itemUsedRadius; --ItemUsedRadius  IntegerField
        itemDetailSection.Q<IntegerField>("ItemUsedRadius").value = selectedItemDetails.itemUsedRadius;
        itemDetailSection.Q<IntegerField>("ItemUsedRadius").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.itemUsedRadius = evt.newValue;
        });
        //6. bool canPickedUp; --CanPickedUp Toggle
        itemDetailSection.Q<Toggle>("CanPickedUp").value = selectedItemDetails.canPickedUp;
        itemDetailSection.Q<Toggle>("CanPickedUp").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.canPickedUp = evt.newValue;
        });
        //7.  bool canDropped; --CanDropped Toggle
        itemDetailSection.Q<Toggle>("CanDropped").value = selectedItemDetails.canDropped;
        itemDetailSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.canDropped = evt.newValue;
        });
        //8.  bool canCarried;--CanCarried Toggle
        itemDetailSection.Q<Toggle>("CanCarried").value = selectedItemDetails.canCarried;
        itemDetailSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.canCarried = evt.newValue;
        });
        //9. float itemPrice; --ItemPrice --FloatField
        itemDetailSection.Q<FloatField>("ItemPrice").value = selectedItemDetails.itemPrice;
        itemDetailSection.Q<FloatField>("ItemPrice").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.itemPrice = evt.newValue;
        });
        //10. float soldPercentage;--SoldPercentage
        itemDetailSection.Q<Slider>("SoldPercentage").value = selectedItemDetails.soldPercentage;
        itemDetailSection.Q<Slider>("SoldPercentage").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.soldPercentage = evt.newValue;
        });
        //11 ItemType itemType;--ItemType EnumField
        itemDetailSection.Q<EnumField>("ItemType").Init(selectedItemDetails.itemType);
        itemDetailSection.Q<EnumField>("ItemType").value = selectedItemDetails.itemType;
        itemDetailSection.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.itemType = (ItemType)evt.newValue;
            //doesn't work
            // selectedItemDetails.itemType = evt.newValue as ItemType;
        });
        //12 string description;--Description  TextField
        itemDetailSection.Q<TextField>("Description").value = selectedItemDetails.description;
        itemDetailSection.Q<TextField>("Description").RegisterValueChangedCallback(evt =>
        {
            selectedItemDetails.description = evt.newValue;
        });
    }
}
