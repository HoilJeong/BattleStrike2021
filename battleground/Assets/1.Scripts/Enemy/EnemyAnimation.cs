using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimation : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public float currentAimingAngleGap;
    [HideInInspector] public Transform gunMuzzle;
    [HideInInspector] public float angularSpeed; //Enemy가 너무 조준을 잘하면 어려우므로 조정

    private StateController controller;
    private NavMeshAgent nav;
    private bool pendingAim; //조준을 기다리는 시간 (자연스러운 IK 회전을 위함)
    private Transform hips, spine; //bone transform
    private Vector3 initialRootRotation;
    private Vector3 initialHipsRotation;
    private Vector3 initialSpineRotation;
    private Quaternion lastRotation;
    private float timeCountAim, timeCountGuard; //원하는 회전값으로 돌리기 위한 타임카운트 (현재 rotation값이랑 원하는 rotation값을 얻기위해)
    private readonly float turnSpeed = 25f; //strafing turn speed

    private void Awake()
    {
        //setup
        controller = GetComponent<StateController>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        nav.updateRotation = false; //회전은 우리가 제어한다.

        hips = anim.GetBoneTransform(HumanBodyBones.Hips);
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);

        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipsRotation = hips.localEulerAngles;
        initialSpineRotation = spine.localEulerAngles;

        anim.SetTrigger(FC.AnimatorKey.ChangeWeapon);
        anim.SetInteger(FC.AnimatorKey.Weapon, (int)System.Enum.Parse(typeof(WeaponType), controller.classStats.WeaponType)); //무기타입을 얻어온다.

        //총구 셋팅
        foreach (Transform child in anim.GetBoneTransform(HumanBodyBones.RightHand))
        {
            gunMuzzle = child.Find("Muzzle");
            if (gunMuzzle != null)
            {
                break;
            }
        }
        //가끔 장착하는 무기에 rigidbody가 있는 경우가 있다. 그럴땐 꺼준다.
        foreach (Rigidbody member in GetComponentsInChildren<Rigidbody>())
        {
            member.isKinematic = true;
        }
    }

    //애니메이터의 여러가지 파라미터를 셋업
    void Setup(float speed, float angle, Vector3 strafeDirection)
    {
        //각속도 구하기
        angle *= Mathf.Deg2Rad; 
        angularSpeed = angle / controller.generalStats.angleRespawnTime;

        anim.SetFloat(FC.AnimatorKey.Speed, speed, controller.generalStats.speedDampTime, Time.deltaTime);
        anim.SetFloat(FC.AnimatorKey.AngularSpeed, angularSpeed, controller.generalStats.angularSpeedDampTime, Time.deltaTime);

        anim.SetFloat(FC.AnimatorKey.Horizontal, strafeDirection.x, controller.generalStats.speedDampTime, Time.deltaTime);
        anim.SetFloat(FC.AnimatorKey.Vertical, strafeDirection.z, controller.generalStats.speedDampTime, Time.deltaTime);
    }

    //NavMeshAgent에서 Animator가 셋업되는 함수
    void NavAnimSetup()
    {
        float speed;
        float angle;
        speed = Vector3.Project(nav.desiredVelocity, transform.forward).magnitude;
        if (controller.focusSight)
        {
            Vector3 dest = (controller.personalTarget - transform.position);
            dest.y = 0.0f;
            angle = Vector3.SignedAngle(transform.forward, dest, transform.up); //앵글을 구해 왼쪽인지 오른쪽인지 알 수 있다.
            if (controller.Strafing)
            {
                dest = dest.normalized;
                Quaternion targetStrafeRotation = Quaternion.LookRotation(dest);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetStrafeRotation, turnSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (nav.desiredVelocity == Vector3.zero)
            {
                angle = 0.0f;
            }
            else
            {
                angle = Vector3.SignedAngle(transform.forward, nav.desiredVelocity, transform.up);
            }
        }
        //플레이어를 향하러 할때 깜빡거리지 않도록 각도 데드존을 적용
        if (!controller.Strafing && Mathf.Abs(angle) < controller.generalStats.angleDeadZone)
        {
            transform.LookAt(transform.position + nav.desiredVelocity);
            angle = 0f;
            if (pendingAim && controller.focusSight)
            {
                controller.Aiming = true;
                pendingAim = false;
            }
        }

        //Strage direction
        Vector3 direction = nav.desiredVelocity;
        direction.y = 0.0f;
        direction = direction.normalized;
        direction = Quaternion.Inverse(transform.rotation) * direction;
        Setup(speed, angle, direction);
    }

    private void Update()
    {
        NavAnimSetup();
    }

    private void OnAnimatorMove()
    {
        if (Time.timeScale > 0 && Time.deltaTime > 0)
        {
            nav.velocity = anim.deltaPosition / Time.deltaTime;
            if (!controller.Strafing)
            {
                transform.rotation = anim.rootRotation;
            }
        }
    }

    private void LateUpdate()
    {
        //조준 보정
        if (controller.Aiming)
        {
            Vector3 direction = controller.personalTarget - spine.position;
            if (direction.magnitude < 0.01f || direction.magnitude > 1000000.0f)
            {
                return;
            }
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(initialRootRotation);
            targetRotation *= Quaternion.Euler(initialHipsRotation);         
            targetRotation *= Quaternion.Euler(initialSpineRotation);

            targetRotation *= Quaternion.Euler(FC.VectorHelper.ToVector(controller.classStats.AimOffset));
            Quaternion frameRotation = Quaternion.Slerp(lastRotation, targetRotation, timeCountAim);

            //엉덩이를 기준으로 척추 회전이 60도 이하인 경우는 계속 조준이 가능
            if (Quaternion.Angle(frameRotation, hips.rotation) <= 60.0f)
            {
                spine.rotation = frameRotation;
                timeCountAim += Time.deltaTime;
            }
            else
            {   //비현실적인 행동을 막기 위함
                if (timeCountAim == 0 && Quaternion.Angle(frameRotation, hips.rotation) > 70.0f)
                {
                    StartCoroutine(controller.UnstuckAim(2f)); //1초 기다렸다가 행동
                }
                spine.rotation = lastRotation;
                timeCountAim = 0;
            }

            lastRotation = spine.rotation;
            Vector3 target = controller.personalTarget - gunMuzzle.position;
            Vector3 forward = gunMuzzle.forward;
            currentAimingAngleGap = Vector3.Angle(target, forward);

            timeCountGuard = 0;
        }
        else
        {   //조준중이 아닐땐 원래의 각도로 되돌아온다.
            lastRotation = spine.rotation;
            spine.rotation *= Quaternion.Slerp(Quaternion.Euler(FC.VectorHelper.ToVector(controller.classStats.AimOffset)), Quaternion.identity, timeCountGuard);
            timeCountGuard += Time.deltaTime;
        }
    }

    public void ActivatePendingAim()
    {
        pendingAim = true;
    }

    public void AbortPendingAim()
    {
        pendingAim = false;
        controller.Aiming = false;
    }
}
