using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItems : MonoBehaviour
{
    public ItemInventoryObject itemInventoryObject;
    public ItemObjectDataBase itemObjectDataBase;

    public void AddNewItem()
    {
        if (itemObjectDataBase.itemObjects.Length > 0)
        {
            //ItemObject newItemObject = itemObjectDataBase.itemObjects[Random.Range(0, itemObjectDataBase.itemObjects.Length - 1)];
            ItemObject newItemObject = itemObjectDataBase.itemObjects[itemObjectDataBase.itemObjects.Length - 1];
            Item newItem = new Item(newItemObject);

            itemInventoryObject.AddItem(newItem, 1);
            Debug.Log("New Item");
        }
    }
}
