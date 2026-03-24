using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementPhysics;

public class Pickup : MonoBehaviour
{
    [HideInInspector] public HP playerHealth;
    [HideInInspector] public MagicManagement playerMagic;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private LayerMask CollisionLayer;

    public FMODUnity.StudioEventEmitter pickupSound;

    private Vector3 groundNormal;
    private RaycastHit groundHit;

    bool grounded = false;

    private float gt = 0.1f;

    public enum PickupType
    {
        Health,
        Magic,
        Greed
    }

    public PickupType Type;
    
    [Range(0, 1)]
    public float size;

    private float spinSpeed = 80f;

    void Update()
    {
        // transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);

        float spin = Time.deltaTime * spinSpeed;
        Vector3 angle = transform.localEulerAngles;
        angle.y += spin;
        transform.localEulerAngles = new Vector3(angle.x, angle.y, angle.z);
    }

    public void OnSpawn()
    {
        transform.localEulerAngles = new Vector3(0f,0f,0f);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = true;
        grounded = false;
        gt = 0.1f;
    }

    void FixedUpdate()
    {
        if(grounded) return;

        Vector3 vel = rb.linearVelocity;

        grounded = MovementFunctions.GroundedCheck(GroundCheck, 0.2f, CollisionLayer, ref vel, ref gt, 0.1f, ref groundNormal, out groundHit);

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

        if(other.gameObject.layer == 3)
        {
            HandlePlayer(other);
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

    private void HandlePlayer(Collider other)
    {

        playerHealth = other.GetComponentInParent<HP>();
        playerMagic = other.GetComponentInParent<MagicManagement>();

        Debug.Log(playerHealth);
        Debug.Log(playerMagic);

        bool consumed = false;

        switch (Type)
        {
            case PickupType.Health:
                if(playerHealth.currentHP < playerHealth.maxHP)
                {
                    playerHealth.Heal(size);
                    consumed = true;
                };
                break;

            case PickupType.Magic:
                if(playerMagic.magicPoints < playerMagic.maximumMagic)
                {
                    float regen = playerMagic.maximumMagic * size;
                    playerMagic.magicPoints += regen;

                    if(playerMagic.magicPoints >= playerMagic.maximumMagic)
                    {
                        playerMagic.magicPoints = playerMagic.maximumMagic;
                    }
                    consumed = true;
                }
                break;
        }

        if (consumed)
        {
            pickupSound.Play();
            PickupPool.Instance.Return(this);
        }
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
