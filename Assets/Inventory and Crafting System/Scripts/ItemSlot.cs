using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    // Event callbacks
    public UnityEvent<Item> onItemUse;
    public UnityEvent onRecipeCompletion;

    // flag to tell ItemSlot it needs to update itself after being changed
    private bool b_needsUpdate = true;

    // Declared with auto-property
    public Item ItemInSlot { get; private set; }
    public int ItemCount { get; private set; }


    // scene references
    [SerializeField]
    private TMPro.TextMeshProUGUI itemCountText;

    [SerializeField]
    private Image itemIcon;

    private void Update()
    {
        if(b_needsUpdate)
        {
            UpdateSlot();
        }
    }

    /// <summary>
    /// Returns true if there is an item in the slot
    /// </summary>
    /// <returns></returns>
    public bool HasItem()
    {
        return ItemInSlot != null;
    }

    /// <summary>
    /// Removes everything in the item slot
    /// </summary>
    /// <returns></returns>
    public void ClearSlot()
    {
        ItemInSlot = null;
        b_needsUpdate = true;
    }

    /// <summary>
    /// Attempts to remove a number of items. Returns number removed
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public int TryRemoveItems(int count)
    {
        if(count > ItemCount)
        {
            int numRemoved = ItemCount;
            ItemCount -= numRemoved;
            b_needsUpdate = true;
            return numRemoved;
        } else
        {
            ItemCount -= count;
            b_needsUpdate = true;
            return count;
        }
    }

    /// <summary>
    /// Sets what is contained in this slot
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public void SetContents(Item item, int count)
    {
        ItemInSlot = item;
        ItemCount = count;
        b_needsUpdate = true;
    }

    /// <summary>
    /// Activate the item currently held in the slot
    /// </summary>
    public void UseItem()
    {
      //  Debug.Log("1. Use item clicked!");
        if(ItemInSlot != null)
        {
          //  Debug.Log("2. Item Here!");
            if (ItemCount >= 1)
            {
                if (Inventory.mouseHeldItem == null) // PICKUP ITEM
                {
                    //  Debug.Log("3. Item Picked up!");
                    //Debug.Log(ItemCount + " " + ItemInSlot.name);
                    ItemInSlot.Use();
                    onItemUse.Invoke(ItemInSlot);
                    ItemCount--;
                    b_needsUpdate = true;
                    if(gameObject.name == "OutputSlot")
                    {
                        //Debug.Log(ItemCount + " " + ItemInSlot.name);
                        onRecipeCompletion.Invoke();
                        ItemCount = 0;
                    }
                }
                else if (Inventory.mouseHeldItem == ItemInSlot) // DROP ITEM INTO STACK
                {
                    //Debug.Log("3. Item dropped!");
                    ItemCount++;
                    onItemUse.Invoke(null);
                    b_needsUpdate = true;
                }
            }
            else
            {
              //  Debug.LogError("3. Item without Count!");
            }
        }
        else // DROP ITEM IN EMPTY SLOT
        {
          //  Debug.Log("2. NOTHING Here!");
            if (Inventory.mouseHeldItem != null && gameObject.name != "OutputSlot") 
            {
                ItemInSlot = Inventory.mouseHeldItem;                
                ItemCount++;
                b_needsUpdate = true;
                onItemUse.Invoke(null);
            }
        }
    }

    /// <summary>
    /// Update visuals of slot to match items contained
    /// </summary>
    private void UpdateSlot()
    {
        if(ItemCount == 0)
        {
            ItemInSlot = null;
        }

      if(ItemInSlot != null)
        {
            itemCountText.text = ItemCount.ToString();
            itemIcon.sprite = ItemInSlot.Icon;
            //itemIcon.gameObject.SetActive(true);
            //itemIcon.enabled = true;
            //itemIcon.GetComponent<Image>().enabled = true;
            itemIcon.GetComponent<Image>().color = new Color(1,1,1,1);
            itemIcon.GetComponentInChildren<TextMeshProUGUI>().alpha = 1;
        } else
        {
            //itemIcon.gameObject.SetActive(false);
            //itemIcon.enabled = false;
            //itemIcon.GetComponent<Image>().enabled = false;
            itemCountText.text = ItemCount.ToString();
            itemIcon.sprite = null;
            itemIcon.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            itemIcon.GetComponentInChildren<TextMeshProUGUI>().alpha = 0;
        }

        b_needsUpdate = false;
    }
}
