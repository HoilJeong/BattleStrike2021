using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사격 기능: 사격이 가능한지 여부를 체크하는 기능
/// 발사 키 입력을 받아서 애니메이션 재생, 이펙트 생성, 충돌 체크 기능
/// UI 관련해서 십자선 표시 기능
/// 발사 속도 조정
/// 캐릭터 상체를 IK를 이용해서 조준 시점에 맞춰 회전
/// 벽이나 충돌체 총알이 피격되었을 경우 피탄 이펙트를 생성
/// 인벤토리 역할. 무기를 소지하고 있는지 확인용
/// 재장전과 무기 교체 기능
/// </summary>
public class ShootBehaviour : GenericBehaviour
{
    public Texture2D aimCrossHair, shootCrossHair;
    public GameObject muzzleFlash, shot, sparks; //총 사격시 필요한 이펙트
    public Material bulletHole; //총알 피탄 자국
    public int MaxBulletHoles = 50;
    public float shootErrorRate = 0.01f; //오발률
    public float shootRateFactor = 1f; //발사 속도

    public float armsRotation = 8f; //팔 회전

    //레이어를 체크해서 해당 오브젝트는 충돌체크 제외
    public LayerMask shotMask = ~(TagAndLayer.LayerMasking.IgnoreRayCast | TagAndLayer.LayerMasking.IgnoreShot | TagAndLayer.LayerMasking.CoverInvisible | TagAndLayer.LayerMasking.Player);
    //레이어를 체크해서 생명체 피탄 자국 제외
    public LayerMask organicMask = TagAndLayer.LayerMasking.Player | TagAndLayer.LayerMasking.Enemy;
    //짧은 총, 피스톨 같은 총을 들었을 때 조준시 왼팔의 위치 보정
    public Vector3 leftArmShortAim = new Vector3(-4.0f, 0.0f, 2.0f);

    //private int activeWeapon = 0;

    //animator value
    public int weaponType;
    public int changeWeaponTrigger;
    private int shootingTrigger;
    private int aimBool, blockedAimBool, reloadBool;

    //private List<InteractiveWeapon> weapons; //소지하고 있는 무기들
    //private Dictionary<InteractiveWeapon.WeaponType, int> slotMap;
    private Inventory inventory;

    private bool isAiming, isAimBlocked;

    public Transform gunMuzzle;
    private float distToHand;

    private Vector3 castRelativeOrigin; //앞에 막혀있는지 확인

    public Transform hips, spine, chest, rightHand, leftArm;
    private Vector3 initialRootRotation;
    private Vector3 initialHipsRotation;
    private Vector3 initialSpineRotation;
    private Vector3 initialChestRotation;

    private float shotInterval, originalShotInterval = 0.5f; //총알 수명
    private List<GameObject> bulletHoles;
    private int bulletHoleSlot = 0;
    private int burstShotCount = 0; //피탄낼 수 있는 총알의 갯수
    private AimBehaviour aimBehaviour;
    private Texture2D originalCrossHair;
    private bool isShooting = false;
    private bool isChangingWeapon = false;
    private bool isShotAlive = false;

    private void Start()
    {
        weaponType = Animator.StringToHash(FC.AnimatorKey.Weapon);
        aimBool = Animator.StringToHash(FC.AnimatorKey.Aim);
        blockedAimBool = Animator.StringToHash(FC.AnimatorKey.BlockedAim);
        changeWeaponTrigger = Animator.StringToHash(FC.AnimatorKey.ChangeWeapon);
        shootingTrigger = Animator.StringToHash(FC.AnimatorKey.Shooting);
        reloadBool = Animator.StringToHash(FC.AnimatorKey.Reload);
        //weapons = new List<InteractiveWeapon>(new InteractiveWeapon[3]);
        aimBehaviour = GetComponent<AimBehaviour>();
        bulletHoles = new List<GameObject>();
        inventory = GetComponent<Inventory>();

        muzzleFlash.SetActive(false);
        shot.SetActive(false);
        sparks.SetActive(false);
        /*
        weaponSlotMap = new Dictionary<InteractiveWeapon.WeaponType, int>
        {
            {InteractiveWeapon.WeaponType.SHORT, 1 },
            {InteractiveWeapon.WeaponType.LONG, 2 }
        };
        */

        Transform neck = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Neck);
        if (!neck)
        {
            neck = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Head).parent;
        }
        hips = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        spine = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
        chest = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Chest);
        rightHand = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        leftArm = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipsRotation = hips.localEulerAngles;
        initialSpineRotation = spine.localEulerAngles;
        initialChestRotation = chest.localEulerAngles;
        originalCrossHair = aimBehaviour.crossHair;
        shotInterval = originalShotInterval;
        castRelativeOrigin = neck.position - transform.position;
        distToHand = (rightHand.position - neck.position).magnitude * 1.5f;
    }

    //발사 비주얼 담당
    private void DrawShoot(GameObject weapon, Vector3 destination, Vector3 targetNormal, Transform parent, bool placeSparks= true, bool placeBulletHole = true)
    {
        Vector3 origin = gunMuzzle.position - gunMuzzle.right * 0.5f;

        muzzleFlash.SetActive(true);
        muzzleFlash.transform.SetParent(gunMuzzle);
        muzzleFlash.transform.localPosition = Vector3.zero;
        muzzleFlash.transform.localEulerAngles = Vector3.back * 90f;

        GameObject instantShot = EffectManager.Instance.EffectOneShot((int)EffectList.tracer, origin);
        instantShot.SetActive(true);
        instantShot.transform.rotation = Quaternion.LookRotation(destination - origin);
        instantShot.transform.parent = shot.transform.parent;

        if (placeSparks)
        {
            GameObject instantSparks = EffectManager.Instance.EffectOneShot((int)EffectList.sparks, destination);
            instantSparks.SetActive(true);
            instantSparks.transform.parent = sparks.transform.parent;
        }

        if (placeBulletHole)
        {
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, targetNormal);
            GameObject bullet = null;
            if (bulletHoles.Count < MaxBulletHoles)
            {
                bullet = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bullet.GetComponent<MeshRenderer>().material = bulletHole;
                bullet.GetComponent<Collider>().enabled = false;
                bullet.transform.localScale = Vector3.one * 0.07f;
                bullet.name = "BulletHole";
                bulletHoles.Add(bullet);
            }
            else
            {
                bullet = bulletHoles[bulletHoleSlot];
                bulletHoleSlot++;
                bulletHoleSlot %= MaxBulletHoles;
            }
            bullet.transform.position = destination + 0.01f * targetNormal;
            bullet.transform.rotation = hitRotation;
            bullet.transform.SetParent(parent);
        }
    }

    private void ShootWeapon(int weapon, bool firstShot = true)
    {
        if (!isAiming || isAimBlocked || behaviourController.GetAnimator.GetBool(reloadBool) || !inventory.weapons[weapon].Shoot(firstShot))
        {
            return;
        }
        else
        {
            this.burstShotCount++;
            behaviourController.GetAnimator.SetTrigger(shootingTrigger);
            aimBehaviour.crossHair = shootCrossHair;
            behaviourController.GetCamScript.BounceVertical(inventory.weapons[weapon].recoilAngle);

            //살짝 못맞추게 만든다
            Vector3 imprecision = Random.Range(-shootErrorRate, shootErrorRate) * behaviourController.playerCamera.forward;
            Ray ray = new Ray(behaviourController.playerCamera.position, behaviourController.playerCamera.forward + imprecision);
            RaycastHit hit = default(RaycastHit);
            if (Physics.Raycast(ray, out hit, 500f, shotMask))
            {
                if (hit.collider.transform != transform)
                {
                    bool isOrganic = (organicMask == (organicMask | (1 << hit.transform.root.gameObject.layer)));
                    DrawShoot(inventory.weapons[weapon].gameObject, hit.point, hit.normal, hit.collider.transform, !isOrganic, !isOrganic);
                    if (hit.collider)
                    {
                        hit.collider.SendMessageUpwards("HitCallBack", new HealthBase.DamageInfo(hit.point, ray.direction, inventory.weapons[weapon].bulletDamage, hit.collider), SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
            else
            {
                Vector3 destination = (ray.direction * 500f) - ray.origin;
                DrawShoot(inventory.weapons[weapon].gameObject, destination, Vector3.up, null, false, false);
            }

            SoundManager.Instance.PlayOneShotEffect((int)inventory.weapons[weapon].shotSound, gunMuzzle.position, 5f);
            GameObject gameController = GameObject.FindGameObjectWithTag(TagAndLayer.TagName.GameController);
            gameController.SendMessage("RootAlertNearBy", ray.origin, SendMessageOptions.DontRequireReceiver);
            shotInterval = originalShotInterval;
            isShotAlive = true;
        }
    }

    public void EndReloadWeapon()
    {
        behaviourController.GetAnimator.SetBool(reloadBool, false);
        inventory.weapons[inventory.activeWeapon].EndReload();
    }

    public void SetWeaponCrossHair(bool armed)
    {
        if (armed)
        {
            aimBehaviour.crossHair = aimCrossHair;
        }
        else
        {
            aimBehaviour.crossHair = originalCrossHair;
        }
    }

    private void ShotProgress()
    {
        if (shotInterval > 0.2f)
        {
            shotInterval -= shootRateFactor * Time.deltaTime;
            if (shotInterval <= 0.4f)
            {
                SetWeaponCrossHair(inventory.activeWeapon > 0);
                muzzleFlash.SetActive(false);
                if (inventory.activeWeapon > 0)
                {
                    behaviourController.GetCamScript.BounceVertical(-inventory.weapons[inventory.activeWeapon].recoilAngle * 0.1f);

                    if (shotInterval <= (0.4f - 2f * Time.deltaTime))
                    {
                        if (inventory.weapons[inventory.activeWeapon].weaponMode == InteractiveWeapon.WeaponMode.AUTO && Input.GetAxisRaw(ButtonName.Shoot) != 0)
                        {
                            ShootWeapon(inventory.activeWeapon, false);
                        }
                        else if (inventory.weapons[inventory.activeWeapon].weaponMode == InteractiveWeapon.WeaponMode.BURST && burstShotCount < inventory.weapons[inventory.activeWeapon].burstSize)
                        {
                            ShootWeapon(inventory.activeWeapon, false);
                        }
                        else if (inventory.weapons[inventory.activeWeapon].weaponMode != InteractiveWeapon.WeaponMode.BURST)
                        {
                            burstShotCount = 0;
                        }
                    }
                }
            }
        }
        else
        {
            isShotAlive = false;
            behaviourController.GetCamScript.BounceVertical(0);
            burstShotCount = 0;
        }
    }

    /*
    public void ChangeWeapon(int oldWeapon, int newWeapon)
    {
        if (oldWeapon > 0)
        {
            inventory.weapons[oldWeapon].gameObject.SetActive(false);
            gunMuzzle = null;
            inventory.weapons[oldWeapon].Toggle(false);
        }

        //빈곳을 찾는다
        while (inventory.weapons[newWeapon] == null && newWeapon > 0)
        {
            newWeapon = (newWeapon + 1) % inventory.weapons.Count;
        }
        if (newWeapon > 0)
        {
            inventory.weapons[newWeapon].gameObject.SetActive(true);
            gunMuzzle = inventory.weapons[newWeapon].transform.Find("Muzzle");
            inventory.weapons[newWeapon].Toggle(true);
        }
        inventory.activeWeapon = newWeapon;
        if (oldWeapon != newWeapon)
        {
            behaviourController.GetAnimator.SetTrigger(changeWeaponTrigger);
            behaviourController.GetAnimator.SetInteger(weaponType, inventory.weapons[newWeapon] ? (int)inventory.weapons[newWeapon].weaponType : 0);
        }
        SetWeaponCrossHair(newWeapon > 0);
    }
    */

    private void Update()
    {
        float shootTrigger = Mathf.Abs(Input.GetAxisRaw(ButtonName.Shoot));
        if (shootTrigger > Mathf.Epsilon && !isShooting && inventory.activeWeapon > 0 && burstShotCount == 0)
        {
            isShooting = true;
            ShootWeapon(inventory.activeWeapon);
        }
        else if (isShooting && shootTrigger < Mathf.Epsilon)
        {
            isShooting = false;
        }
        else if (Input.GetButtonUp(ButtonName.Reload) && inventory.activeWeapon > 0)
        {
            if (inventory.weapons[inventory.activeWeapon].StartReload())
            {
                SoundManager.Instance.PlayOneShotEffect((int)inventory.weapons[inventory.activeWeapon].reloadSound, gunMuzzle.position, 0.5f);
                behaviourController.GetAnimator.SetBool(reloadBool, true);
            }
        }     
        else if (Input.GetButtonDown(ButtonName.Drop) && inventory.activeWeapon > 0)
        {
            EndReloadWeapon();
            int weaponToDrop = inventory.activeWeapon;
            inventory.ChangeWeapon(inventory.activeWeapon, 0);
            inventory.weapons[weaponToDrop].Drop();
            inventory.weapons[weaponToDrop] = null;
        }       
        else
        {
            if ((Mathf.Abs(Input.GetAxisRaw(ButtonName.Change)) > Mathf.Epsilon && !isChangingWeapon))
            {
                isChangingWeapon = true;
                int nextWeapon = inventory.activeWeapon;
                inventory.ChangeWeapon(inventory.activeWeapon, nextWeapon % inventory.weapons.Count);
            }
            else if (Mathf.Abs(Input.GetAxisRaw(ButtonName.Change)) < Mathf.Epsilon)
            {
                isChangingWeapon = false;
            }
        }
        if(isShotAlive)
        {
            ShotProgress();
        }
        isAiming = behaviourController.GetAnimator.GetBool(aimBool);
    }

    /*
    /// <summary>
    /// 인벤토리 역할을 하게될 함수
    /// </summary>   
    public void AddWeapon(InteractiveWeapon newWeapon)
    {
        newWeapon.gameObject.transform.SetParent(rightHand);
        newWeapon.transform.localPosition = newWeapon.rigthHandPosition;
        newWeapon.transform.localRotation = Quaternion.Euler(newWeapon.relativeRotation);

        if (weapons[slotMap[newWeapon.weaponType]])
        {
            if  (weapons[slotMap[newWeapon.weaponType]].label_weaponName == newWeapon.label_weaponName)
            {
                weapons[slotMap[newWeapon.weaponType]].ResetBullet();
                ChangeWeapon(activeWeapon, slotMap[newWeapon.weaponType]);
                Destroy(newWeapon.gameObject);

                return;
            }
            else
            {
                weapons[slotMap[newWeapon.weaponType]].Drop();
            }
        }

        weapons[slotMap[newWeapon.weaponType]] = newWeapon;
        ChangeWeapon(activeWeapon, slotMap[newWeapon.weaponType]);
    }
    */

    private bool CheckforBlockedAim()
    {
        isAimBlocked = Physics.SphereCast(transform.position + castRelativeOrigin, 0.1f, behaviourController.GetCamScript.transform.forward, out RaycastHit hit, distToHand - 0.1f);
        isAimBlocked = isAimBlocked && hit.collider.transform != transform;
        behaviourController.GetAnimator.SetBool(blockedAimBool, isAimBlocked);
        Debug.DrawRay(transform.position + castRelativeOrigin, behaviourController.GetCamScript.transform.forward * distToHand, isAimBlocked ? Color.red : Color.cyan);

        return isAimBlocked;
    }

    public void OnAnimatorIK(int layerIndex)
    {
        if (isAiming && inventory.activeWeapon > 0)
        {
            if (CheckforBlockedAim())
            {
                return;
            }
            Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            targetRot *= Quaternion.Euler(initialRootRotation);
            targetRot *= Quaternion.Euler(initialHipsRotation);
            targetRot *= Quaternion.Euler(initialSpineRotation);
            behaviourController.GetAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Inverse(hips.rotation) * targetRot);

            float xCamRot = Quaternion.LookRotation(behaviourController.playerCamera.forward).eulerAngles.x;
            targetRot = Quaternion.AngleAxis(xCamRot + armsRotation, this.transform.right);
            if (inventory.weapons[inventory.activeWeapon] && inventory.weapons[inventory.activeWeapon].weaponType == InteractiveWeapon.WeaponType.LONG)
            {
                targetRot *= Quaternion.AngleAxis(9f, transform.right);
                targetRot *= Quaternion.AngleAxis(20f, transform.up);
            }
            targetRot *= spine.rotation;
            targetRot *= Quaternion.Euler(initialChestRotation);
            behaviourController.GetAnimator.SetBoneLocalRotation(HumanBodyBones.Chest, Quaternion.Inverse(spine.rotation) * targetRot);
        }
    }

    private void LateUpdate()
    {
        if (isAiming && inventory.weapons[inventory.activeWeapon] && inventory.weapons[inventory.activeWeapon].weaponType == InteractiveWeapon.WeaponType.SHORT)
        {
            leftArm.localEulerAngles = leftArm.localEulerAngles + leftArmShortAim;
        }
    }
}
