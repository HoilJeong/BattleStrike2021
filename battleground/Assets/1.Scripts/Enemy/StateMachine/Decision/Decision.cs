using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 조건을 체크하는 클래스
/// 조건 체크를 위해 특정 위치로부터 원하는 검색 반경에 있는 충돌체를 찾아서 그 안에 타겟이 있는지 확인
/// </summary>
public abstract class Decision : ScriptableObject
{
    public abstract bool Decide(StateController controller);

    public virtual void OnEnableDecision(StateController controller)
    {

    }

    public delegate bool HandleTargets(StateController controller, bool hasTargets, Collider[] targetInRadius);

    public static bool CheckTargetsInRadius(StateController controller, float radius, HandleTargets handleTargets)
    {   //모든 decision에서 활용
        //타겟된 상대가 죽었으면 false리턴
        if (controller.aimTarget.root.GetComponent<HealthBase>().isDead)
        {
            return false;
        }
        else
        {
            //컨트롤러의 위치를 중심으로 해서 내가 원하는 반경에서 내 타겟 마스크를 넘긴다. (Enemy일 경우 player가 타겟)
            Collider[] targetsInRadius = Physics.OverlapSphere(controller.transform.position, radius, controller.generalStats.targetMask);

            return handleTargets(controller, targetsInRadius.Length > 0, targetsInRadius);
        }
    }
}
