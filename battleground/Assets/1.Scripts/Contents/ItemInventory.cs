using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemInventory
{
    public ItemInventorySlot[] slots = new ItemInventorySlot[4];
    
    public void Clear()
    {
        foreach (ItemInventorySlot slot in slots)
        {           
            slot.RemoveItem();
        }
    }


    public bool IsContain(ItemObject itemObject)
    {
        //return Array.Find(slots, i => i.item.id == itemObject.data.id) != null;
        return IsContain(itemObject.data.id);
    }

    public bool IsContain(int id)
    {
        return slots.FirstOrDefault(i => i.item.id == id) != null;
    }
}
