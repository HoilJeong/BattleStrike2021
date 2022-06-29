using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickUpItem : MonoBehaviour
{
    public float distance = 3.0f;
    public float Distance => distance;

    public string label_itemName;
    private Rigidbody itemRigidbody;
    private Transform pickHUD;
    public Text pickupHUD_Label;
    private GameObject player, gameController;
    private bool pickable;
    private BoxCollider itemCollider;
    public ItemObject itemObject;
    public ItemInventoryObject itemInventoryObject;
    private SphereCollider interactiveRadius;

    private void Awake()
    {
        gameObject.name = this.label_itemName;
        gameObject.layer = LayerMask.NameToLayer(TagAndLayer.LayerName.IgnoreRayCast);
        foreach (Transform tr in transform)
        {
            tr.gameObject.layer = LayerMask.NameToLayer(TagAndLayer.LayerName.IgnoreRayCast);
        }

        player = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.Player);
        
        gameController = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.GameController);

        if (pickHUD == null)
        {
            pickHUD = gameController.transform.Find("PickupHUD");
        }

        itemCollider = transform.gameObject.AddComponent<BoxCollider>();
        CreateInteractiveRadius(itemCollider.center);
        itemRigidbody = gameObject.AddComponent<Rigidbody>();

        pickHUD.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (this.pickable && Input.GetButtonDown(ButtonName.Pick))
        {      
            itemRigidbody.isKinematic = true;
            itemCollider.enabled = false;
            itemInventoryObject.AddItem(new Item(this.itemObject), 1);
            Destroy(this.gameObject);         
            this.pickable = false;
            TogglePickHUD(false);
        }
    }

    private void CreateInteractiveRadius(Vector3 center)
    {
        interactiveRadius = gameObject.AddComponent<SphereCollider>();
        interactiveRadius.center = center;
        interactiveRadius.radius = 1;
        interactiveRadius.isTrigger = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distance);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            pickable = false;
            TogglePickHUD(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player && itemInventoryObject)
        {
            pickable = true;
            TogglePickHUD(true);
        }
    }

    private void TogglePickHUD(bool toggle)
    {
        pickHUD.gameObject.SetActive(toggle);
        if (toggle)
        {
            pickHUD.position = this.transform.position + Vector3.up * 0.5f;
            Vector3 direction = player.GetComponent<BehaviourController>().playerCamera.forward;
            direction.y = 0;
            pickHUD.rotation = Quaternion.LookRotation(direction);
            pickupHUD_Label.text = "Pick " + this.gameObject.name;
        }
    }

    
}
