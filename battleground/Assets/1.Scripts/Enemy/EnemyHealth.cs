using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyHealth : HealthBase
{
    public float health = 100f;
    public GameObject healthHUD;
    public GameObject bloodSample;
    public bool headShot;
   
    private float totalHealth;
    private Transform weapon;
    private Transform hud;
    private RectTransform healthBar;
    private float originalBarScale;
    private HealthHUD healthUI;
    private Animator anim;
    private StateController controller;
    private GameObject gameController;
    private GameObject player;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        hud = Instantiate(healthHUD, transform).transform;
        if (!hud.gameObject.activeSelf)
        {
            hud.gameObject.SetActive(true);
        }
        totalHealth = health;
        healthBar = hud.transform.Find("Bar").GetComponent<RectTransform>();
        healthUI = hud.GetComponent<HealthHUD>();
        originalBarScale = healthBar.sizeDelta.x;
        anim = GetComponent<Animator>();
        controller = GetComponent<StateController>();
        gameController = GameObject.FindGameObjectWithTag("GameController");
        player = GameObject.FindGameObjectWithTag("Player");

        foreach (Transform child in anim.GetBoneTransform(HumanBodyBones.RightHand))
        {
            weapon = child.Find("Muzzle");
            if (weapon != null)
            {
                break;
            }
        }
        weapon = weapon.parent;
    }

    private void UpdateHealthBar()
    {
        float scaleFactor = health / totalHealth;
        healthBar.sizeDelta = new Vector2(scaleFactor * originalBarScale, healthBar.sizeDelta.y);
    }

    private void RemoveAllForces()
    {
        foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>())
        {
            body.isKinematic = false;
            body.velocity = Vector3.zero;
        }
    }

    public void Kill()
    {
        foreach (MonoBehaviour mb in GetComponents<MonoBehaviour>())
        {
            if (this != mb)
            {
                Destroy(mb);
            }
        }
        playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.KillEnemy();
        Destroy(GetComponent<NavMeshAgent>());
        RemoveAllForces();
        controller.focusSight = false;
        anim.SetBool(FC.AnimatorKey.Aim, false);
        anim.SetBool(FC.AnimatorKey.Crouch, false);
        anim.enabled = false;
        Destroy(weapon.gameObject);
        Destroy(hud.gameObject);
        this.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        Destroy(this.gameObject, 120f);
        isDead = true;      
    }

    public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
    {
        if (!isDead && headShot && bodyPart.transform == anim.GetBoneTransform(HumanBodyBones.Head))
        {
            damage *= 10;
            gameController.SendMessage("HeadShotCallback", SendMessageOptions.DontRequireReceiver);
        }
        Instantiate(bloodSample, location, Quaternion.LookRotation(-direction), transform);
        health -= damage;
        if (!isDead)
        {
            anim.SetTrigger("Hit");
            healthUI.SetVisible();
            UpdateHealthBar();
            controller.variables.feelAlert = true;
            controller.personalTarget = controller.aimTarget.position;
        }
        if (health <= 0)
        {
            if (!isDead)
            {
                Kill();
            }
            Rigidbody rigid = bodyPart.GetComponent<Rigidbody>();
            rigid.mass = 40;
            rigid.AddForce(100f * direction.normalized, ForceMode.Impulse);
        }
    }
}
