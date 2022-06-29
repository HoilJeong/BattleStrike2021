using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// 현재 동작, 기본 동작, 오버라이딩 동작, 잠긴 동작, 마우스 이동값, 
/// 땅에 서있는지, GenericBehaviour를 상속받은 동작들을 업데이트 시켜준다.
/// </summary>
public class BehaviourController : MonoBehaviour
{
    private List<GenericBehaviour> behaviours; //동작들
    private List<GenericBehaviour> overrideBehaviours; //우선시 되는 동작
    private int currentBehaviour; //현재 동작 해시코드
    private int defaultBehaviour; //기본 동작 해시코드
    private int behaviourLocked; //잠긴 동작 해시코드

    //캐싱
    public Transform playerCamera;
    private Animator myAnimator;
    private Rigidbody myRigidbody;
    private ThirdPersonOrbitCam camScript;
    private Transform myTransform; 
    public GameObject player;
    public PlayerHealth playerHealth;
    [SerializeField]
    private Inventory inventory;  

    //
    private float h; //horizontal axis
    private float v; //vertical axis
    public float turnSmoothing = 0.06f; //카메라를 향하도록 움직일 때 회전속도   
    private Vector3 lastDirection; //마지막 향했던 방향
    private bool sprint; //달리기 중인가?
    private int hFloat; //애니메이터 관련 가로축 값
    private int vFloat; //애니메이터 관련 세로축 값
    private int groundedBool; //애니메이터 지상에 있는가
    private Vector3 colExtents; //땅과의 충돌체크를 위한 충돌체 영역

    [SerializeField]
    private ItemInventoryObject itemInventory; 

    public float GetH 
    {
        get
        {
            return h;
        }
    }

    public float GetV
    {
        get
        {
            return v;
        }
    }

    public ThirdPersonOrbitCam GetCamScript
    {
        get
        {
            return camScript;
        }
    }

    public Rigidbody GetRigidbody
    {
        get
        {
            return myRigidbody;
        }
    }

    public Animator GetAnimator
    {
        get
        {
            return myAnimator;
        }
    }

    public int GetDefaultBehaviour
    {
        get
        {
            return defaultBehaviour;
        }
    }

    private void Awake()
    {     
        behaviours = new List<GenericBehaviour>();
        overrideBehaviours = new List<GenericBehaviour>();
        myAnimator = GetComponent<Animator>();
        hFloat = Animator.StringToHash(FC.AnimatorKey.Horizontal);
        vFloat = Animator.StringToHash(FC.AnimatorKey.Vertical);
        camScript = playerCamera.GetComponent<ThirdPersonOrbitCam>();
        myRigidbody = GetComponent<Rigidbody>();
        myTransform = transform;
        playerHealth = GetComponent<PlayerHealth>();
        inventory = player.GetComponent<Inventory>();
        //ground?
        groundedBool = Animator.StringToHash(FC.AnimatorKey.Grounded);
        colExtents = GetComponent<Collider>().bounds.extents;
    }

    private void Start()
    {
        itemInventory.OnUseItem = OnUseItem;
    }

    private void OnUseItem(ItemObject itemObject)
    {     
        foreach (ItemBuff buff in itemObject.data.buffs)
        {          
            if (buff.state == CharacterAttribute.CurrentHP)
            {
                if (playerHealth.health >= 100)
                {
                    return;
                }

                playerHealth.health += 30;
                playerHealth.OnChangedStats();
                Debug.Log("HEAL");
            }
            if (buff.state == CharacterAttribute.Bullet)
            {               
                if (inventory.weapon == null)
                {
                    return;
                }

                inventory.weapon.ResetBullet();
                inventory.weapon.UpdateHUD();
                Debug.Log("Bullet");
            }
        }
    }

    public bool IsMoving()
    {
        //return (h != 0) || (v != 0); 매우 위험한 코드
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon;
    }

    public bool IsHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }

    public bool CanSprint()
    {
        foreach (GenericBehaviour behaviour in behaviours)
        {
            if (!behaviour.AllowSprint)
            {
                return false;
            }
        }

        foreach (GenericBehaviour genericBehaviour in overrideBehaviours)
        {
            if (!genericBehaviour.AllowSprint)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsSprinting()
    {
        return sprint && IsMoving() && CanSprint();
    }

    public bool IsGrounded()
    {
        Ray ray = new Ray(myTransform.position + Vector3.up * 2 * colExtents.x, Vector3.down);
        return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
    }

    private void Update()
    {
        bool isOnUI = EventSystem.current.IsPointerOverGameObject();

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        myAnimator.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
        myAnimator.SetFloat(vFloat, v, 0.1f, Time.deltaTime);

        sprint = Input.GetButton(ButtonName.Sprint);

        myAnimator.SetBool(groundedBool, IsGrounded());
    }

    

    public bool PickUpItem(PickUpItem pickUpItem, int amount = 1)
    {

        if (pickUpItem.itemObject != null && itemInventory.AddItem(new Item(pickUpItem.itemObject), amount))
        {
            Destroy(pickUpItem.gameObject);
            return true;
        }

        return false;
    }

    public void Repositioning()
    {
        if (lastDirection != Vector3.zero)
        {
            lastDirection.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(lastDirection);
            Quaternion newRotation = Quaternion.Slerp(myRigidbody.rotation, targetRotation, turnSmoothing);
            myRigidbody.MoveRotation(newRotation);
        }
    }

    private void FixedUpdate()
    {
        bool isAnyBehabiourActive = false;
        if (behaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            foreach(GenericBehaviour behaviour in behaviours)
            {
                if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                {
                    isAnyBehabiourActive = true;
                    behaviour.LocalFixedUpdate();
                }
            }
        }
        else
        {
            foreach(GenericBehaviour behaviour in overrideBehaviours)
            {
                behaviour.LocalFixedUpdate();
            }
        }

        if (!isAnyBehabiourActive && overrideBehaviours.Count == 0)
        {
            myRigidbody.useGravity = true;
            Repositioning();
        }

        
    }

    private void LateUpdate()
    {
        if (behaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            foreach (GenericBehaviour behaviour in behaviours)
            {
                if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                {                 
                    behaviour.LocalLateUpdate();
                }
            }
        }
        else
        {
            foreach (GenericBehaviour behaviour in overrideBehaviours)
            {
                behaviour.LocalLateUpdate();
            }
        }       
    }

    public void SupScribeBehaviour(GenericBehaviour behaviour)
    {
        behaviours.Add(behaviour);
    }

    public void RegisterDefaultBehavior(int behaviourCode)
    {
        defaultBehaviour = behaviourCode;
        currentBehaviour = behaviourCode;
    }

    public void RegisterBehaviour(int behaviourCode)
    {
        if (currentBehaviour == defaultBehaviour)
        {
            currentBehaviour = behaviourCode;
        }
    }

    public void UnRegisterBehaviour(int behaviourCode)
    {
        if (currentBehaviour == behaviourCode)
        {
            currentBehaviour = defaultBehaviour;
        }
    }

    public bool OverrideWithBehaviour(GenericBehaviour behaviour)
    {
        if (!overrideBehaviours.Contains(behaviour))
        {
            if(overrideBehaviours.Count == 0)
            {
                foreach(GenericBehaviour behaviour1 in behaviours)
                {
                    if (behaviour1.isActiveAndEnabled && currentBehaviour == behaviour1.GetBehaviourCode)
                    {
                        behaviour1.OnOverride();
                        break;
                    }
                }
            }

            overrideBehaviours.Add(behaviour);
            return true;
        }

        return false;
    }

    public bool RevokeOverridingBehaviour(GenericBehaviour behaviour)
    {
        if(overrideBehaviours.Contains(behaviour))
        {
            overrideBehaviours.Remove(behaviour);
            return true;
        }

        return false;
    }

    public bool IsOverriding(GenericBehaviour behaviour = null)
    {
        if (behaviour == null)
        {
            return overrideBehaviours.Count > 0;
        }

        return overrideBehaviours.Contains(behaviour);
    }

    public bool IsCurrentBehaviour(int behaviourCode)
    {
        return this.currentBehaviour == behaviourCode;
    }

    public bool GetTempLockStatus(int behaviourCode = 0)
    {
        return (behaviourLocked != 0 && behaviourLocked != behaviourCode);
    }

    public void LockTempBehaviour(int behaviourCode)
    {
        if (behaviourLocked == 0)
        {
            behaviourLocked = behaviourCode;
        }
    }

    public void UnLockTempBehaviour(int behaviourCode)
    {
        if (behaviourLocked == behaviourCode)
        {
            behaviourLocked = 0;
        }
    }

    public Vector3 GetLastDirection()
    {
        return lastDirection;
    }

    public void SetLastDirection(Vector3 direction)
    {
        lastDirection = direction;
    }
}

public abstract class GenericBehaviour : MonoBehaviour
{
    protected int speedFloat;
    public BehaviourController behaviourController;
    protected int behaviourCode;
    protected bool canSprint;

    private void Awake()
    {
        this.behaviourController = GetComponent<BehaviourController>();
        speedFloat = Animator.StringToHash(FC.AnimatorKey.Speed);
        canSprint = true;
        //동작 타입을 해시코드로 가지고 있다가 추후에 구별용으로 사용
        behaviourCode = this.GetType().GetHashCode();
    }

    public int GetBehaviourCode
    {
        get
        {
            return behaviourCode;
        }
    }

    public bool AllowSprint
    {
        get
        {
            return canSprint;
        }
    }

    public virtual void LocalLateUpdate()
    {

    }

    public virtual void LocalFixedUpdate()
    {
    
    }

    public virtual void OnOverride()
    {

    }
}

