using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Tag, Layer를 자동으로 설정해주는 에디터
public class NewBehaviourScript : Editor
{
    [MenuItem("GameObject/Enemy AI/ Setup Tag and Layers", false, 11)]
    static void Init()
    {
        GameObject go = Selection.activeGameObject;
        go.tag = "Enemy";
        go.layer = LayerMask.NameToLayer("Enemy");
        GameObject hips = go.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).gameObject;
        if (!hips.GetComponent<Collider>())
        {
            hips = hips.transform.GetChild(0).gameObject;
        }
        hips.layer = LayerMask.NameToLayer("Enemy");
        //가끔 collider가 붙혀지는 것을 막기 위해
        go.GetComponentInChildren<SkinnedMeshRenderer>().gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Transform child in go.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand))
        {
            Transform gunMuzzle = child.Find("Muzzle");
            if (gunMuzzle != null)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                foreach (Transform part in child)
                {
                    part.gameObject.layer = child.gameObject.layer;
                }
            }
        }
    }
}

//Editor가 시작하자마자 Enemy 레이어가 있는지 없는지 확인
[InitializeOnLoad]
public class StartUp
{ 
    static StartUp()
    {
        if (LayerMask.NameToLayer("Enemy") != 12)
        {
            Debug.LogWarning("Enemy Layer Missing! 추가해주세요");
        }
    }
}

