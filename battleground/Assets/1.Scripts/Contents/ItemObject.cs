using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName ="New Item", menuName = "Inventory System/Items/New Item")]
public class ItemObject : ScriptableObject
{  
    public ItemType type;
    public bool stackable; //겹쳐서 표시되는 아이템

    public Sprite icon;
    public GameObject modelPrefab; //캐릭터에게 부착되거나 같이 표시되어야할 3D오브젝트

    public Item data = new Item();

    public List<string> boneNames = new List<string>();

    [TextArea(15, 20)]
    public string description;
  
    private void OnValidate()
    {
        boneNames.Clear();
        if (modelPrefab == null || modelPrefab.GetComponentInChildren<SkinnedMeshRenderer>() == null)
        {
            return;
        }

        SkinnedMeshRenderer renderer = modelPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
        Transform[] bones = renderer.bones;

        foreach (Transform t in bones)
        {
            boneNames.Add(t.name);
        }
    }   
    
    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }  
}
