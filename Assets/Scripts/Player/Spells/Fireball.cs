using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GamePhysics;
public class Fireball : MonoBehaviour
{

    [Header("References")]
    public LayerMask targetMask;

    public LayerMask groundMask;

    public LayerMask[] ignoreLayers;

    public Transform player;

    public HP playerHP;

    public FMODUnity.StudioEventEmitter impact;

    [Header("Rocket Params")]
    public float damage;
    public float damageMultiplier = 1f;
    public float airshotMultiplier = 3f;
    public float maximumDamage = 100f;
    public float speed;
    public float explosionRadius;
    public float explosionForce;
    public float forceMultiplier = 1f;
    private Rigidbody rb;

    public bool grappled;

    float impactAngle;

    private PlayerControlRigid playerControl;

    private void Start()
    {
        playerHP = player.GetComponentInParent<HP>();
        playerControl = player.GetComponentInParent<PlayerControlRigid>();

        //assign each prefab a rigid body   
        rb = GetComponent<Rigidbody>();
        //move based on forward direction and velocity param
        rb.linearVelocity = transform.forward * speed;

        ProjectileParticleManager.Instance.Register(this);

    }

    void Update()
    {
        if (grappled)
        {
            Destroy(gameObject, 2f);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Debug.Log(other);
        int layer = other.layer;

        if (GameFunctions.FilterLayers(layer, ignoreLayers))
        {
            ProjectileParticleManager.Instance.Delete(this);
            Destroy(gameObject);
            return;
        }

            Vector3 impactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;

            Vector3 impactNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : -rb.linearVelocity.normalized;

            impactAngle = Vector3.Angle(impactNormal, Vector3.up);

            impact.Play();

            bool direct = layer == LayerMask.NameToLayer("Enemy") || layer == LayerMask.NameToLayer("NPC");

            Explode(direct);

            if(layer != LayerMask.NameToLayer("Enemy"))
            {
                DecalManager.Instance.SpawnDecal(impactPoint,impactNormal, DecalManager.Instance.fireSplatter);
            }

    }

    private void Explode(bool direct = false)
    {
        //init array of collisions. check explosion radius overlapping with player bounding box
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, targetMask);
        HashSet<HP> damagedHP = new HashSet<HP>();

        foreach (Collider hit in hits)
        {
            HP targetHP = hit.GetComponentInParent<HP>();
            Enemy e = hit.GetComponentInParent<Enemy>();
            // Debug.Log(e);
            NPC npc = hit.GetComponentInParent<NPC>();
            PlayerControlRigid pc = hit.GetComponentInParent<PlayerControlRigid>();
            Rigidbody rb = hit.attachedRigidbody;


            Vector3 targetForce = GameFunctions.TargetedExplosionForce(hit, transform.position, explosionRadius, explosionForce, forceMultiplier);

            Vector3 selfForce = GameFunctions.SelfExplosionForce(hit, transform.position, explosionRadius, explosionForce, forceMultiplier);

            damage = GameFunctions.CalculateForceDamage(hit, transform.position, explosionRadius, maximumDamage, damageMultiplier, direct);


            if (targetHP != null && !damagedHP.Contains(targetHP) && targetHP != playerHP)
            {
                if (e != null)
                {
                    Vector3 impact = targetForce;

                    damagedHP.Add(targetHP);

                    if(playerControl.playerVelocity.y > 20f || playerControl.playerVelocity.y < -20f)
                    {
                        TrickManager.Instance.Bomb();
                    }

                    if (!e.grounded)
                    {
                        bool airshotted = false;
                        float airdamage = direct ? damage * airshotMultiplier : damage;
                        if (direct && (playerControl.RocketJumped || playerControl.playerVelocity.y >= 10f) && airshotted == false)
                        {
                            TrickManager.Instance.AirAirshot();
                            airshotted = true;
                        }
                        else if(direct && airshotted == false)
                        {
                            TrickManager.Instance.Airshot();
                        }
                        targetHP.Damage(airdamage);
                    }
                    else
                    {
                        e.grounded = false;
                        targetHP.Damage(damage);
                        impact.y += explosionForce;
                        impact.x *= 2f;
                        impact.z *= 2f;
                        if (direct)
                        {
                            TrickManager.Instance.Direct();
                        }

                    }

                    if (!e.engage)
                    {
                        e.engage = true;
                        e.memory = 15f;
                    }
                    float resistance = 1f - e.knockbackResistance;
                    e.enemyVelocity += impact * resistance;
                    
                    if(targetHP.currentHP <= 0f)
                    {
                        TrickManager.Instance.Kill(e.Points, e.Name);
                    }
                }
            }
            if (pc != null)
            {
                if(pc.playerVelocity.y < 0f)
                {
                    pc.playerVelocity.y = 0f;
                }

                pc.AddKnockback(selfForce);

                if (!pc.CanPogo && !pc.StartSyncTimer)
                {
                    pc.StartSyncTimer = true;
                    pc.syncResult = true;
                    pc.syncTimer = 0f;
                    pc.syncHits = 0;
                }

                if (pc.CanPogo && impactAngle < 40f)
                {
                    TrickManager.Instance.Pogo();
                }

                if(pc.CanWall && impactAngle >= 40f && !direct)
                {
                    TrickManager.Instance.Wall();
                }

                if(pc.StartSyncTimer)
                {
                    pc.syncHits++;
                }
            }

            if(npc != null)
            {
                if (direct)
                {
                    damagedHP.Add(targetHP);
                    targetHP.Damage(targetHP.maxHP);
                }
            }

            // if (rb != null)
            // {
            //     GameFunctions.ApplyForceToRigidbody(ref rb, e, targetForce);
            //     Debug.Log("force to rigidbody");
            // }
        }



        // ProjectileParticleManager.Instance.fireballExplode.Emit(emitParams, 5);
        // Debug.Log(ProjectileParticleManager.Instance.fireballExplode);

        var ps = ProjectileParticleManager.Instance.fireballExplode;
        ps.transform.position = transform.position;
        // ps.Simulate(0f, true, true); 
        ps.Emit(5);
        ps.Play();
    
        ProjectileParticleManager.Instance.fireballExplosionSmoke.Play();
        
        //destroy on explosion
        ProjectileParticleManager.Instance.Delete(this);
        Destroy(gameObject);
    }

}
