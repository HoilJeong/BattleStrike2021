using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// state -> actions update -> transition (decision) check
/// state에 필요한 기능들. 에니메이션 콜백들
/// 시야 체크, 찾아놓은 엄페물 장소중 가장 가까운 위치를 찾는 기능
/// </summary>
public class StateController : MonoBehaviour
{
    public GeneralStats generalStats;
    public ClassStats statData;
    public string classID; //PISTOL, RIFLE, AK,

    public ClassStats.Param classStats
    {
        get
        {
            //프로퍼티를 사용해 데이터를 직접 불러온다.
            foreach(ClassStats.Sheet sheet in statData.sheets)
            {
                foreach(ClassStats.Param parm in sheet.list)
                {
                    if (parm.ID.Equals(this.classID))
                    {
                        return parm;
                    }
                }
            }
            return null;
        }
    }

    public State currentState;
    public State remainState;

    public Transform aimTarget;

    public List<Transform> patrolWaypoints;

    public int bullets;
    [Range(0,50)]
    public float viewRadius; //캐릭터 시야
    [Range(0,360)]
    public float viewAngle;
    [Range(0,25)]
    public float perceptionRadius;

    [HideInInspector] public float nearRadius;
    [HideInInspector] public NavMeshAgent nav;
    [HideInInspector] public int wayPointIndex;
    [HideInInspector] public int maximumBurst = 7;
    [HideInInspector] public float blindEngageTime = 30f;

    [HideInInspector] public bool targetInSight;
    [HideInInspector] public bool focusSight;
    [HideInInspector] public bool reloading;
    [HideInInspector] public bool hadClearShot; //타겟까지 장애물이 있었는가 없었는가?
    [HideInInspector] public bool haveClearShot; //지금 타켓까지 장애물이 있는가?
    [HideInInspector] public int coverHash = -1; //하나의 장애물에 적이 뭉쳐있지 않도록

    [HideInInspector] public EnemyVariables variables;
    [HideInInspector] public Vector3 personalTarget = Vector3.zero; //각 적의 타겟의 위치

    private int magBullets;
    private bool aiActive;
    private static Dictionary<int, Vector3> coverSpot; //static
    private bool strafing; //A플레이어는 움직이면서 B플레이어를 맞추지만, B플레이어는 움직이는 A플레이어를 못맞추는것
    private bool aiming;
    private bool checkedOnLoop, blockedSight;

    [HideInInspector] public EnemyAnimation enemyAnimation;
    [HideInInspector] public CoverLookUp coverLookUp;

    public Vector3 CoverSpot
    {
        get
        {
            return coverSpot[this.GetHashCode()];
        }
        set
        {
            //Enemy마다 instanceID가 다르므로, 각 장애물마다 Enemy가 한명씩 숨을 수 있다.
            coverSpot[this.GetHashCode()] = value;
        }
    }

    public void TransitionToState(State nextState, Decision decision)
    {
        //remainState라는건 State가 바뀌지않고 현재 State를 유지한다는 뜻
        if (nextState != remainState)
        {
            currentState = nextState;
        }
    }

    public bool Strafing
    {
        get
        {
            return strafing;
        }
        set
        {
            enemyAnimation.anim.SetBool("Strafe", value);
            strafing = value;
        }
    }

    public bool Aiming
    {
        get
        {
            return aiming;
        }
        set
        {
            if(aiming != value)
            {
                enemyAnimation.anim.SetBool("Aim", value);
                aiming = value;
            }
        }
    }


    public IEnumerator UnstuckAim(float delay)
    {
        yield return new WaitForSeconds(delay * 0.5f);
        Aiming = false;
        yield return new WaitForSeconds(delay * 0.5f);
        Aiming = true;
    }

    private void Awake()
    {
        if (coverSpot == null)
        {
            coverSpot = new Dictionary<int, Vector3>();
        }
        coverSpot[this.GetHashCode()] = Vector3.positiveInfinity;
        nav = GetComponent<NavMeshAgent>();
        aiActive = true;
        enemyAnimation = gameObject.AddComponent<EnemyAnimation>();
        magBullets = bullets;
        variables.shotsInRounds = maximumBurst;

        nearRadius = perceptionRadius * 0.5f;

        GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
        coverLookUp = gameController.GetComponent<CoverLookUp>();
        if (coverLookUp == null)
        {
            coverLookUp = gameController.AddComponent<CoverLookUp>();
            coverLookUp.Setup(generalStats.coverMask);
        }

        Debug.Assert(aimTarget.root.GetComponent<HealthBase>(), "반드시 타겟에는 생명력관련 컴포넌트를" + "붙여주어야한다.");
    }

    private void Start()
    {
        currentState.OnEnableActions(this);
    }

    private void Update()
    {
        checkedOnLoop = false;

        if (!aiActive)
        {
            return;
        }

        currentState.DoActions(this);
        currentState.CheckTransitions(this);
    }

    private void OnDrawGizmos()
    {
        if (currentState != null)
        {
            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 2f);
        }
    }

    public void EndReloadWeapon()
    {
        reloading = false;
        bullets = magBullets;
    }

    public void AlertCallback(Vector3 target)
    {
        if (!aimTarget.root.GetComponent<HealthBase>().isDead)
        {
            this.variables.hearAlert = true;
            this.personalTarget = target;
        }
    }

    public bool IsNearOtherSpot(Vector3 spot, float margin = 1f)
    {
        foreach (KeyValuePair<int, Vector3> usedSpot in coverSpot)
        {   //거의 도착했다
            if (usedSpot.Key != gameObject.GetHashCode() && Vector3.Distance(spot, usedSpot.Value) <= margin)
            {
                return true;
            }
        }

        return false;
    }

    public bool BlockedSight()
    {
        if (!checkedOnLoop)
        {
            checkedOnLoop = true;
            Vector3 target = default;
            try
            {
                target = aimTarget.position;
            }
            catch (UnassignedReferenceException)
            {
                Debug.LogError("조준 타겟을 지정해주세요: " + transform.name);
            }

            Vector3 castOrigin = transform.position + Vector3.up * generalStats.aboveCoverHeight;
            Vector3 dirToTarget = target - castOrigin;

            blockedSight = Physics.Raycast(castOrigin, dirToTarget, out RaycastHit hit, dirToTarget.magnitude, generalStats.coverMask | generalStats.obstacleMask);
        }

        return blockedSight;
    }

    private void OnDestroy()
    {
        coverSpot.Remove(this.GetHashCode());
    }
}
