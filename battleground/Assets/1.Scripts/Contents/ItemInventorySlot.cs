using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[Serializable]
public class ItemInventorySlot
{
    public ItemType[] allowedItems = new ItemType[0];

    [NonSerialized]
    public ItemInventoryObject parent;
    [NonSerialized]
    public GameObject slotUI;

    [NonSerialized]
    public Action<ItemInventorySlot> OnPreUpdate;
    [NonSerialized]
    public Action<ItemInventorySlot> OnPostUpdate;

    public Item item;
    public int amount;

    public ItemObject ItemObject
    {
        get
        {
            return item.id >= 0 ? parent.dataBase.itemObjects[item.id] : null;
        }
    }

    public ItemInventorySlot() => UpdateSlot(new Item(), 0);
    public ItemInventorySlot(Item item, int amount) => UpdateSlot(item, amount);
    
    public void RemoveItem() => UpdateSlot(new Item(), 0);

    public void AddAmount(int value) => UpdateSlot(item, amount += value);

    public void UpdateSlot(Item item, int amount)
    {
        if (amount <= 0)
        {
            item = new Item();
        }

        OnPreUpdate?.Invoke(this);

        this.item = item;
        this.amount = amount;

        OnPostUpdate?.Invoke(this);
    }

    public bool CanPlaceInSlot(ItemObject itemObject)
    {
        if (allowedItems.Length <= 0 || itemObject == null || itemObject.data.id < 0)
        {
            return true;
        }

        foreach (ItemType type in allowedItems)
        {
            if (itemObject.type == type)
            {
                return true;
            }
        }

        return false;
    }
}
