using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class MouseData
{
    public static ItemInventoryUI interfaceMouseIsOver;
    public static GameObject slotHoveredOver;
    public static GameObject tempItemBeingDragged;
}

[RequireComponent(typeof(EventTrigger))]
public abstract class ItemInventoryUI : MonoBehaviour
{
    #region Variables

    public ItemInventoryObject inventoryObject; 

    public Dictionary<GameObject, ItemInventorySlot> slotUIs = new Dictionary<GameObject, ItemInventorySlot>();

    #endregion Variables

    #region Unity Methods

    private void Awake()
    {
        CreateSlots();

        for (int i = 0; i < inventoryObject.slots.Length; i++)
        {
            inventoryObject.slots[i].parent = inventoryObject;
            inventoryObject.slots[i].OnPostUpdate += OnPostUpdate;
        }

        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
    }

    protected virtual void Start()
    {
        for (int i = 0; i < inventoryObject.slots.Length; ++i)
        {
            inventoryObject.slots[i].UpdateSlot(inventoryObject.slots[i].item, inventoryObject.slots[i].amount);
        }
    }

    #endregion Unity Methods

    #region Methods

    public abstract void CreateSlots();

    public void OnPostUpdate(ItemInventorySlot slot)
    {
        if (slot == null || slot.slotUI == null)
        {
            return;
        }

        slot.slotUI.transform.GetChild(0).GetComponent<Image>().sprite = slot.item.id < 0 ? null : slot.ItemObject.icon;
        slot.slotUI.transform.GetChild(0).GetComponent<Image>().color = slot.item.id < 0 ? new Color(1, 1, 1, 0) : new Color(1, 1, 1, 1);
        slot.slotUI.GetComponentInChildren<TextMeshProUGUI>().text = slot.item.id < 0 ? string.Empty : (slot.amount == 1 ? string.Empty : slot.amount.ToString("n0"));
    }

    protected void AddEvent(GameObject go, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = go.GetComponent<EventTrigger>();
        if (!trigger)
        {
            Debug.LogWarning("No EventTrigger component found!");
            return;
        }

        EventTrigger.Entry eventTrigger = new EventTrigger.Entry { eventID = type };
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnterInterface(GameObject go)
    {
        MouseData.interfaceMouseIsOver = go.GetComponent<ItemInventoryUI>();
    }
    public void OnExitInterface(GameObject go)
    {
        MouseData.interfaceMouseIsOver = null;
    }


    public void OnEnter(GameObject go)
    {
        MouseData.slotHoveredOver = go;
        MouseData.interfaceMouseIsOver = GetComponentInParent<ItemInventoryUI>();
    }

    public void OnExit(GameObject go)
    {
        MouseData.slotHoveredOver = null;
    }


    public void OnBeginDrag(GameObject go)
    {
        MouseData.tempItemBeingDragged = CreateDragImage(go);
    }

    private GameObject CreateDragImage(GameObject go)
    {
        if (slotUIs[go].item.id < 0)
        {
            return null;
        }

        GameObject dragImage = new GameObject();

        RectTransform rectTransform = dragImage.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);
        dragImage.transform.SetParent(transform.parent);
        Image image = dragImage.AddComponent<Image>();
        image.sprite = slotUIs[go].ItemObject.icon;
        image.raycastTarget = false;

        dragImage.name = "Drag Image";

        return dragImage;
    }

    public void OnDrag(GameObject go)
    {
        if (MouseData.tempItemBeingDragged == null)
        {
            return;
        }

        MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
    }

    public void OnEndDrag(GameObject go)
    {
        Destroy(MouseData.tempItemBeingDragged);

        if (MouseData.interfaceMouseIsOver == null)
        {
            slotUIs[go].RemoveItem();
        }
        else if (MouseData.slotHoveredOver)
        {
            ItemInventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotUIs[MouseData.slotHoveredOver];
            inventoryObject.SwapItems(slotUIs[go], mouseHoverSlotData);
        }
    }

    public void OnClick(GameObject go, PointerEventData data)
    {
        ItemInventorySlot slot = slotUIs[go];
        if (slot == null)
        {
            return;
        }

        if (data.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick(slot);
        }
        else if (data.button == PointerEventData.InputButton.Right)
        {
            OnRightClick(slot);
        }
    }

    protected virtual void OnRightClick(ItemInventorySlot slot)
    {
    }

    protected virtual void OnLeftClick(ItemInventorySlot slot)
    {
    }

    #endregion Methods
}
