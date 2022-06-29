using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertChecker : MonoBehaviour
{
    [Range(0, 50)] public float alertRadius; //경고 반경
    public int extraWaves = 1; //몇번 경고를 할지

    public LayerMask alertMask = TagAndLayer.LayerMasking.Enemy;
    private Vector3 current; //현재 위치
    private bool alert;

    private void Start()
    {
        InvokeRepeating("PingAlert", 1, 1); //1초마다 일정 주기로 반복
    }

    //특정 위치 주변으로 경고를 보낸다
    void AlertNearBy(Vector3 origin, Vector3 target, int wave = 0)
    {
        if (wave > this.extraWaves)
        {
            return;
        }
        Collider[] targetsInViewRadius = Physics.OverlapSphere(origin, alertRadius, alertMask);

        foreach (Collider obj in targetsInViewRadius)
        {
            obj.SendMessageUpwards("AlertCallback", target, SendMessageOptions.DontRequireReceiver);

            AlertNearBy(obj.transform.position, target, wave + 1);
        }
    }

    public void RootAlertNearBy(Vector3 origin)
    {
        current = origin;
        alert = true;
    }

    void PingAlert()
    {
        if (alert)
        {
            alert = false;
            AlertNearBy(current, current);
        }
    }
}
