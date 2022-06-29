using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가깝거나, 시야안에 있거나 엄폐물이 사라졌거나 다른 엄폐물을 찾거나 하기위한 거리체크 Decision
/// 인지 타입에 따라 특정 거리로부터 가깝지 않거나, 시야는 막히지 않았지만 위험요소를 감지했거나
/// 너무 가까운 거리에 타겟(플레이어)이 있는지 판단
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Focus")]
public class FocusDecision : Decision
{
    public enum Sense
    {
        NEAR,
        PERCEPTION,
        VIEW,
    }
    [Tooltip("어떤 크기로 위험요소 감지를 하겠습니까?")]
    public Sense sense;
    [Tooltip("현재 엄폐물을 해제 할까요?")]
    public bool invalidateCoverSpot;

    private float radius; //sense에 따른 range 범위

    public override void OnEnableDecision(StateController controller)
    {   //Decision이 시작하기전 초기화하는 함수
        switch (sense)
        {
            case Sense.NEAR:
                radius = controller.nearRadius;
                break;
            case Sense.PERCEPTION:
                radius = controller.perceptionRadius;
                break;
            case Sense.VIEW:
                radius = controller.viewRadius;
                break;
            default:
                radius = controller.nearRadius;
                break;
        }
    }

    private bool MyHandleTargets(StateController controller, bool hasTarget, Collider[] targetsInHearRadius)
    {
        //타겟이 존재하고, 시야가 막히지 않았다면
        if (hasTarget && !controller.BlockedSight())
        {
            if (invalidateCoverSpot)
            {
                controller.CoverSpot = Vector3.positiveInfinity;
            }
            controller.targetInSight = true;
            controller.personalTarget = controller.aimTarget.position;

            return true;
        }

        return false;
    }

    public override bool Decide(StateController controller)
    {
        return (sense != Sense.NEAR && controller.variables.feelAlert && !controller.BlockedSight() || CheckTargetsInRadius(controller, radius, MyHandleTargets));
    }
}
