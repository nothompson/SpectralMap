using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementPhysics;

public class Pickup : MonoBehaviour
{
    [HideInInspector] public HP health;
    [HideInInspector] public MagicManagement magic;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private LayerMask CollisionLayer;

    public FMODUnity.StudioEventEmitter pickupSound;

    private Vector3 groundNormal;
    private RaycastHit groundHit;
    private bool onPlatform = false;
    public Vector3 platformVelocity;
    private Vector3 lastGroundCheckPos;

    bool grounded = false;

    private float gt = 0.1f;

    public bool Respawning = false;

    public bool floating = false;

    public BoxCollider collider;
    public GameObject container;

    public enum PickupType
    {
        Health,
        Magic,
        Greed,
        FleshSuit,
    }

    public PickupType Type;
    
    [Range(0, 1)]
    public float size;

    bool consumed = false;

    public void OnSpawn()
    {
        consumed = false;
        transform.localEulerAngles = new Vector3(0f,0f,0f);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if(!floating){
            rb.useGravity = true;
        }
        else
        {
            rb.useGravity = false;
        }
        grounded = false;
        gt = 0.1f;
        lastGroundCheckPos = GroundCheck.position;
    }

    void FixedUpdate()
    {
        if(floating) return;
        if(grounded) return;

        Vector3 vel = rb.linearVelocity;

        grounded = MovementFunctions.GroundedCheck(GroundCheck, 0.2f, CollisionLayer, ref vel, ref gt, 0.1f, ref groundNormal, out groundHit, ref onPlatform, ref platformVelocity, ref lastGroundCheckPos, transform);

        rb.linearVelocity = vel;

        if (grounded)
        {
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {

        //     if (type > 2)
        //     {
        //         // float roll = Random.Range(0f, 1f);
        //         EffectManager.effectManager.Boost(player, 20f, 3.0f);
        //         pickupSound.Play();
        //         Destroy(gameObject);
        //     }
        // }

        if(other.gameObject.layer == 3 || other.gameObject.layer == 11)
        {
            HandlePickup(other);
        }

        // else if (other.gameObject.layer == 11)
        // {
        //     HandleEnemy(other);
        // }

        //     if (type > 2)
        //     {
        //         // float roll = Random.Range(0f, 1f);
        //         EffectManager.effectManager.Weak(enemy, 20f, 0.5f);
        //         Destroy(gameObject);
        //     }
        // }
    }

    private void HandlePickup(Collider other)
    {

        health = other.GetComponentInParent<HP>();
        magic = other.GetComponentInParent<MagicManagement>();

        switch (Type)
        {
            case PickupType.Health:
                if(health == null) break;
                if(health.currentHP < health.maxHP)
                {
                    health.Heal(size);
                    consumed = true;
                };
                break;

            case PickupType.Magic:
                if(magic == null) break;
                if(magic.magicPoints < magic.maximumMagic)
                {
                    float regen = magic.maximumMagic * size;
                    magic.magicPoints += regen;

                    if(magic.magicPoints >= magic.maximumMagic)
                    {
                        magic.magicPoints = magic.maximumMagic;
                    }
                    consumed = true;
                    HitNumberManager.Instance.DisplayHitNumber(regen, transform, HitNumber.HitType.Magic);
                }
                break;
            case PickupType.Greed:
                if(other.gameObject.layer == 11) break;
                EffectManager.Instance.PotOfGreed(other.gameObject);
                
                consumed = true;
                
                break;
            case PickupType.FleshSuit:
                if(other.gameObject.layer == 11) break;
                consumed = true;
                EffectManager.Instance.FleshSuit(other.gameObject, 100f);
                break;
        }

        if (consumed)
        {
            pickupSound.Play();
            if(!Respawning){
            PickupPool.Instance.Return(this);
            }
            else if (Respawning)
            {
                container.SetActive(false);
                collider.enabled = false;
                StartCoroutine(Respawn());
            }
        }
    }

    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2.5f);

        container.SetActive(true);
        collider.enabled = true;
        consumed = false;
    }

    private void HandleEnemy(Collider other)
    {
        Enemy ai = other.GetComponentInParent<Enemy>();
        HP enemyHP = other.GetComponentInParent<HP>();

        switch (Type)
        {
            case PickupType.Health:
                if(ai != null)
                {
                    ai.critical = false;
                    ai.engage = true;

                    if(enemyHP != null && enemyHP.currentHP < enemyHP.maxHP)
                    {
                        enemyHP.Heal(size);
                        PickupPool.Instance.Return(this);
                    }
                }
                break;
        }
    }
}
