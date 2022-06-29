using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBase : MonoBehaviour
{
    public class DamageInfo
    {
        public Vector3 location, direction;
        public float damage;
        public Collider bodyPart; //어느 부위에 맞았는가?
        public GameObject origin; //데미지 이펙트

        public DamageInfo(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
        {
            this.location = location;
            this.direction = direction;
            this.damage = damage;
            this.bodyPart = bodyPart;
            this.origin = origin;
        }
    }

    [HideInInspector] public bool isDead;
    protected Animator myAnimator;

    public virtual void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
    {

    }

    //콜백을 받고, 메세지를 받는다. 다른 데이터 객체에서 TakeDamage 데이터를 쓰게 하기위함
    public void HitCallBack(DamageInfo damageInfo)
    {
        this.TakeDamage(damageInfo.location, damageInfo.direction, damageInfo.damage, damageInfo.bodyPart, damageInfo.origin);
    }

}
