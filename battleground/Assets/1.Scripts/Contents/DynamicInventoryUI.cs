using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DynamicInventoryUI : ItemInventoryUI
{
    #region Variables
    [SerializeField]
    protected GameObject slotPrefab;

    [SerializeField]
    protected Vector2 start;

    [SerializeField]
    protected Vector2 size;

    [SerializeField]
    protected Vector2 space;


    [Min(1), SerializeField]
    protected int numberOfColumn = 4;

    #endregion Variables  

    #region Methods

    public override void CreateSlots()
    {
        slotUIs = new Dictionary<GameObject, ItemInventorySlot>();

        for (int i = 0; i < inventoryObject.slots.Length; ++i)
        {
            GameObject go = Instantiate(slotPrefab, Vector3.zero, Quaternion.identity, transform);
            go.GetComponent<RectTransform>().anchoredPosition = CalculatePosition(i);

            AddEvent(go, EventTriggerType.PointerEnter, delegate { OnEnter(go); });
            AddEvent(go, EventTriggerType.PointerExit, delegate { OnExit(go); });
            AddEvent(go, EventTriggerType.BeginDrag, delegate { OnBeginDrag(go); });
            AddEvent(go, EventTriggerType.EndDrag, delegate { OnEndDrag(go); });
            AddEvent(go, EventTriggerType.Drag, delegate { OnDrag(go); });
            AddEvent(go, EventTriggerType.PointerClick, (data) => { OnClick(go, (PointerEventData)data); });

            inventoryObject.slots[i].slotUI = go;
            slotUIs.Add(go, inventoryObject.slots[i]);
            go.name += ": " + i;
        }
    }
    

    /*
    private void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            if (gameObject.activeSelf == true)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }    
    }
    */

    public Vector3 CalculatePosition(int i)
    {
        float x = start.x + ((space.x + size.x) * (i % numberOfColumn));
        float y = start.y + (-(space.y + size.y) * (i / numberOfColumn));

        return new Vector3(x, y, 0f);
    }

    protected override void OnRightClick(ItemInventorySlot slot)
    {
        inventoryObject.UseItem(slot);
    }

    #endregion Methods
}
