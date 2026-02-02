using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    [SerializeField] private GameObject Grid;
    [SerializeField] public GameObject Display;
     [SerializeField] public Image ItemToDisplay;
    [SerializeField] public SpriteAnimate DisplayAnimate;
    [SerializeField] public SpriteText TitleText;
    [SerializeField] public SpriteText DescriptionText;

    public GameObject Container;
    public GameObject SubContainer;

    public SpriteAnimate bagSprite;

    [SerializeField] private AnimationCurve transitionCurve;


    public Item[] AllItems;

    [SerializeField] private DraggableItem draggableItemPrefab;

    private Dictionary<string, Item> ItemLookup = new();
    public List <Item> InventoryItems = new();

    private Vector2Int DefaultGridPosition = new Vector2Int(0,0);
    private Vector2Int NextAvailableGridPosition;


    public bool animating = false;
    Coroutine transitionRoutine;
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadAllItems();

    }

    void InitGrid()
    {
        var slots = Grid.GetComponentsInChildren<ItemSlot>();
        var gridLayout = Grid.GetComponent<GridLayoutGroup>();

        int columns = gridLayout.constraintCount;

        for(int i = 0; i < slots.Length; i++)
        {
            slots[i].GridPosition = new Vector2Int(i % columns, i / columns);
            GridManager.Instance.RegisterSlot(slots[i]);
        }
    }

    void LoadAllItems()
    {
        AllItems = Resources.LoadAll<Item>("Items");

        ItemLookup = AllItems.ToDictionary(item => item.ID, item => item);
    }

    void Start()
    {        
        InitGrid();

        LoadInventory();    

        AddItem("testitem", new Vector2Int(3,3));
        AddItem("testitem2", new Vector2Int(0,0));
    }
    
    public void Open()
    {
        if(animating) return;

        if(JournalManager.Instance.animating || SettingsMenu.Instance.animating) return;

        SubContainer.SetActive(false);

        Container.SetActive(true);

        bagSprite.index = bagSprite.sprites.Length - 1;

        StartTransition(true);

        StartCoroutine(bagSprite.AnimateToTarget(0, null, () =>
        {
            SubContainer.SetActive(true);

            animating = false;
        }));
        animating = true;
    }

    public void Close()
    {
        if(animating) return;

        bagSprite.index = 0;

        StartTransition(false);

        SubContainer.SetActive(false);
        StartCoroutine(bagSprite.AnimateToTarget(bagSprite.sprites.Length - 1, null, () =>
        {
            Container.SetActive(false);
            animating = false;
        }));
        animating = true;
    }

    public void StartTransition(bool intro)
    {
        if(transitionRoutine != null) {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
            }
        transitionRoutine = StartCoroutine(Transition(intro));
    }

    IEnumerator Transition(bool intro)
    {
        float t = 0f;
        float dur = intro ? 0.25f : 0.75f;

        Vector3 target = intro ? Vector3.one : Vector3.zero;
        RectTransform rect = Container.GetComponent<RectTransform>();
        Vector3 starting;

        if (intro)
        {
            rect.localScale = Vector3.zero;
            starting = Vector3.one;
        }
        else
        {
            starting = rect.localScale;
        }
        while(t < dur)
        {
            t += Time.unscaledDeltaTime;
            float time = Mathf.Clamp01(t / dur);
            float elapsed = intro ? time : 1f - time;
            float value = transitionCurve.Evaluate(elapsed);

            rect.localScale = starting * value;
            yield return null;
        }
        rect.localScale = target;
        transitionRoutine = null;
    }

    public void AddItem(string itemID, Vector2Int GridPosition)
    {
        //pickup, event, npc interaction, etc
        if(!ItemLookup.TryGetValue(itemID, out var item))
        {
            //invalid id
            Debug.Log("invalid id");
            return;
        }
        
        //no duplicates
        if(item.IsInInventory) return;

        if (!InventoryItems.Contains(item))
        {
            Debug.Log("Add item to list of InventoryItems");
            InventoryItems.Add(item);
        }

        if(!GridManager.Instance.TryGetSlot(GridPosition, out var slot))
        {
            Debug.Log("No available slot at given position");
            return;
        }

        DraggableItem ItemToAdd = Instantiate(draggableItemPrefab, slot.transform);

        if(ItemToAdd == null)
        {
            Debug.Log("item to add could not be instantiated");
        }

        item.IsInInventory = true;
        item.PositionOnGrid = GridPosition;
        
        ItemToAdd.rect = ItemToAdd.GetComponent<RectTransform>();
        ItemToAdd.ItemData = item;

        slot.SetItem(ItemToAdd);

        SaveInventory();

        //update grid positioning stuff

    }

    public void SaveInventory()
    {
        InventoryData data = new InventoryData();

        foreach(var item in AllItems)
        {
            if(!item.IsInInventory) continue;

            ItemData d = new ItemData
            {
                ID = item.ID,
                IsInInventory = item.IsInInventory,
                PositionOnGrid = item.PositionOnGrid
            };

            data.InventoryItems.Add(d);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);

        //save items player has in inventory and their position on grid
    }
    public void LoadInventory()
    {
        if(!File.Exists(GetSavePath())) return;

        string json = File.ReadAllText(GetSavePath());
        InventoryData data = JsonUtility.FromJson<InventoryData>(json);

        foreach(var item in AllItems)
        {
            item.IsInInventory = false;
            item.PositionOnGrid = Vector2Int.zero;
        }
        InventoryItems.Clear();

        foreach(var d in data.InventoryItems)
        {
            if(!ItemLookup.TryGetValue(d.ID, out var item)) continue;

            item.IsInInventory = d.IsInInventory;
            item.PositionOnGrid = d.PositionOnGrid;

            if(item.IsInInventory) InventoryItems.Add(item);

            //add to grid based on positionOnGrid
        }
        // Get items player should have in inventory and their 
        // associated positions/status effects etc

        RefreshInventory();
    }

    public void RemoveItem(string itemID)
    {
        //when giving to npc or destroying removing     

        if(!ItemLookup.TryGetValue(itemID, out var item)) return;

        item.IsInInventory = false;
        item.PositionOnGrid = Vector2Int.zero;
        InventoryItems.Remove(item);

        SaveInventory();

    }

    public void RefreshInventory()
    {
        GridManager.Instance.ClearAll();

        foreach(Transform child in SubContainer.transform)
        {
            if(child.GetComponent<DraggableItem>()) Destroy(child.gameObject);
        }

        foreach(var item in InventoryItems)
        {
            if(!GridManager.Instance.TryGetSlot(item.PositionOnGrid, out var slot)) continue;

            DraggableItem draggedItem = Instantiate(draggableItemPrefab, slot.transform);

            draggedItem.ItemData = item;
            draggedItem.rect = draggedItem.GetComponent<RectTransform>();

            slot.SetItem(draggedItem);
        }
    }

    public void DisplayItemDescription()
    {
        //when hovering over grid rect with item in it,
        //show item inspect sprite and description

    }

    public void SelectItem()
    {
        //rightclick to open small list of options
        //use, set active/unactive, remove etc
    }

    public void SetItemActive()
    {
        // if active gain benefit/ unique function of item, otherwise remove
    }

    public void UseItem()
    {
        //if one time use item, trigger activation then remove from inventory
    }


    string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "Inventory.json");
    }
}
