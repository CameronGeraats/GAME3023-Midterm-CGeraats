using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Assertions;
using System.Linq; // For finding all gameObjects with name
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour, ISaveHandler
{
    [Tooltip("Reference to the master item table")]
    [SerializeField]
    private ItemTable masterItemTable;
    
    [Tooltip("Reference to the master item table")]
    [SerializeField]
    private RecipeTable masterRecipeTable;

    [Tooltip("The object which will hold Item Slots as its direct children")]
    [SerializeField]
    private GameObject inventoryPanel;    
    
    [Tooltip("The object which will hold Item Slots as its direct children")]
    [SerializeField]
    private GameObject craftingPanel;

    [Tooltip("List size determines how many slots there will be. Contents will replaced by copies of the first element")]
    [SerializeField]
    private List<ItemSlot> itemSlots;

    [Tooltip("List size determines how many slots there will be. Contents will replaced by copies of the first element")]
    [SerializeField]
    private List<ItemSlot> craftSlots;

    [Tooltip("Items to add on Start for testing purposes")]
    [SerializeField]
    private List<Item> startingItems;

    public static Item mouseHeldItem;

    [Tooltip("The object which will hold the mouse Item")]
    [SerializeField]
    private GameObject mouseItem;

    [Tooltip("The object which will hold the mouse Item")]
    [SerializeField]
    private GameObject outputItem;


    /// <summary>
    /// Private key used for saving with playerprefs
    /// </summary>
    private string saveKey = "";

    // Start is called before the first frame update
    void Start()
    {
        mouseHeldItem = null;
        mouseItem.SetActive(false);
        InitItemSlots();
        InitSaveInfo();

        // init starting items for testing
        for (int i = 0; i < startingItems.Count && i < itemSlots.Count; i++)
        {
            if(startingItems[i] != null)
                itemSlots[i].SetContents(startingItems[i], 16);
            else
                itemSlots[i].SetContents(null, 0);
        }
        
        for (int i = 0; i < craftSlots.Count; i++)
        {
            craftSlots[i].SetContents(null, 0);
        }
    }

    private void InitItemSlots()
    {
        Assert.IsTrue(itemSlots.Count > 0, "itemSlots was empty");
        Assert.IsNotNull(itemSlots[0], "Inventory is missing a prefab for itemSlots. Add it as the first element of its itemSlot list");

        // init item slots
        for (int i = 1; i < itemSlots.Count; i++)
        {
            GameObject newObject = Instantiate(itemSlots[0].gameObject, inventoryPanel.transform);
            ItemSlot newSlot = newObject.GetComponent<ItemSlot>();
            itemSlots[i] = newSlot;
        }

        foreach (ItemSlot item in itemSlots)
        {
            item.onItemUse.AddListener(OnItemUsed);
        }

        for (int i = 1; i < craftSlots.Count; i++)
        {
            GameObject newObject = Instantiate(craftSlots[0].gameObject, craftingPanel.transform);
            ItemSlot newSlot = newObject.GetComponent<ItemSlot>();
            craftSlots[i] = newSlot;
        }

        foreach (ItemSlot item in craftSlots)
        {
            item.onItemUse.AddListener(OnItemUsed);
        }

        outputItem.GetComponent<ItemSlot>().onItemUse.AddListener(OnItemUsed);
        outputItem.GetComponent<ItemSlot>().onRecipeCompletion.AddListener(CompletedRecipe);
    }
    private void InitSaveInfo()
    {
        // init save info
        //assert only one object with the same name, or else we can have key collisions on PlayerPrefs
        Assert.AreEqual(
            Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(gameObArg => gameObArg.name == gameObject.name).Count(),
            1,
            "More than one gameObject have the same name, therefore there may be save key collisions in PlayerPrefs"
            );

        // set a key to use for saving/loading
        saveKey = gameObject.name + this.GetType().Name;

        //Subscribe to save events on start so we are listening
        GameSaver.OnLoad.AddListener(OnLoad);
        GameSaver.OnSave.AddListener(OnSave);
    }

    private void OnDestroy()
    {
        // Remove listeners on destroy
        GameSaver.OnLoad.RemoveListener(OnLoad);
        GameSaver.OnSave.RemoveListener(OnSave);

        foreach (ItemSlot item in itemSlots)
        {
            item.onItemUse.RemoveListener(OnItemUsed);
        }
    }

    //////// Event callbacks ////////

    void OnItemUsed(Item itemUsed)
    {
        // Debug.Log("Inventory: item used of category " + itemUsed.category);        
        if (itemUsed != null)
        {
           // Debug.Log("Picked up item");
            mouseHeldItem = itemUsed;
            mouseItem.SetActive(true);
            mouseItem.GetComponent<Image>().sprite = itemUsed.Icon;
            mouseItem.GetComponentInChildren<TextMeshProUGUI>().SetText("1");
        }
        else
        {
          //  Debug.Log("Dropped item");
            mouseHeldItem = null;            
            mouseItem.GetComponent<Image>().sprite = null;
            mouseItem.GetComponentInChildren<TextMeshProUGUI>().SetText("0");
            mouseItem.SetActive(false);
        }
    }

    public void OnSave()
    {
        //Make empty string
        //For each item slot
        //Get its current item
        //If there is an item, write its id, and its count to the end of the string
        //If there is not an item, write -1 and 0 

        //File format:
        //ID,Count,ID,Count,ID,Count

        string saveStr = "";

        foreach(ItemSlot itemSlot in itemSlots)
        {
            int id = -1;
            int count = 0;

            if(itemSlot.HasItem())
            {
                id = itemSlot.ItemInSlot.ItemID;
                count = itemSlot.ItemCount;
            }

            saveStr += id.ToString() + ',' + count.ToString() + ',';
        }

        PlayerPrefs.SetString(saveKey, saveStr);
    }

    public void OnLoad()
    {
        //Get save string
        //Split save string
        //For each itemSlot, grab a pair of entried (ID, count) and parse them to int
        //If ID is -1, replace itemSlot's item with null
        //Otherwise, replace itemSlot with the corresponding item from the itemTable, and set its count to the parsed count

        string loadedData = PlayerPrefs.GetString(saveKey, "");

        Debug.Log(loadedData);

        char[] delimiters = new char[] { ',' };
        string[] splitData = loadedData.Split(delimiters);

        for(int i = 0; i < itemSlots.Count; i++)
        {
            int dataIdx = i * 2;

            int id = int.Parse(splitData[dataIdx]);
            int count = int.Parse(splitData[dataIdx + 1]);

            if(id < 0)
            {
                itemSlots[i].ClearSlot();
            } else
            {
                itemSlots[i].SetContents(masterItemTable.GetItem(id), count);
            }
        }
    }

    public void CheckForRecipe()
    {
        // Loop through craftSlots and recipes, coroutine?
        int recipeID = 0;
        bool validRecipe = true;
        if(masterRecipeTable.GetRecipeTableSize() > 0)
        {
            for(; recipeID < masterRecipeTable.GetRecipeTableSize(); recipeID++)
            {
                validRecipe = true;
                for(int j = 0; j < masterRecipeTable.GetRecipe(recipeID).GridItems.Length; j++)
                {
                    if (masterRecipeTable.GetRecipe(recipeID).GridItems[j].count == craftSlots[j].ItemCount
                        && masterRecipeTable.GetRecipe(recipeID).GridItems[j].item == craftSlots[j].ItemInSlot) ;
                    else
                        validRecipe = false;
                }
                if (validRecipe)
                    break;
            }
            if(validRecipe)
            {
                outputItem.GetComponent<ItemSlot>().SetContents(masterRecipeTable.GetRecipe(recipeID).OutputItem, 1);
            }
            else
            {
                outputItem.GetComponent<ItemSlot>().SetContents(null, 0);
            }
        }
        else
        {
            outputItem.GetComponent<ItemSlot>().SetContents(null, 0);
        }
    }

    public void CompletedRecipe()
    {
        foreach (ItemSlot item in craftSlots)
        {
            item.SetContents(null, 0);
        }
    }

    private void Update()
    {
        CheckForRecipe();    
    }
}
