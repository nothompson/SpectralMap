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
    
    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
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

            bool direct = layer == LayerMask.NameToLayer("Enemy");

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
            PlayerControlRigid pc = hit.GetComponentInParent<PlayerControlRigid>();
            Rigidbody rb = hit.attachedRigidbody;


            Vector3 force = GameFunctions.ExplosionForce(hit, transform.position, explosionRadius, explosionForce);
            damage = GameFunctions.CalculateForceDamage(force, maximumDamage, damageMultiplier);

            if (targetHP != null && !damagedHP.Contains(targetHP) && targetHP != playerHP)
            {
                if (e != null)
                {
                    Vector3 impact = force;

                    damagedHP.Add(targetHP);

                    if(playerControl.playerVelocity.y > 20f || playerControl.playerVelocity.y < -20f)
                    {
                        TrickManager.Instance.AddTrick("Bomb");
                    }

                    if (!e.grounded)
                    {
                        targetHP.Damage(damage * airshotMultiplier);
                        if (direct)
                        {
                            TrickManager.Instance.AddTrick("Airshot");
                        }
                        if(direct && playerControl.RocketJumped)
                        {
                            TrickManager.Instance.AddTrick("Air-Airshot");
                        }
                    }
                    else
                    {
                        targetHP.Damage(damage);
                        impact.y *= 2f;
                        if (direct)
                        {
                            TrickManager.Instance.AddTrick("Direct");
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
                        TrickManager.Instance.AddTrick("Kill");
                    }
                }
            }
            if (pc != null)
            {
                if(pc.playerVelocity.y < 0f)
                {
                    pc.playerVelocity.y = 0f;
                }
                pc.playerVelocity += force;

                if (!pc.CanPogo && !pc.StartSyncTimer)
                {
                    pc.StartSyncTimer = true;
                    pc.syncResult = true;
                    pc.syncTimer = 0f;
                    pc.syncHits = 0;
                }

                if (pc.CanPogo && impactAngle < 40f)
                {
                    TrickManager.Instance.AddTrick("Pogo");
                }

                if(pc.CanPogo && impactAngle >= 40f && !direct)
                {
                    if(!pc.surfing) TrickManager.Instance.AddTrick("Wall");
                }

                if(pc.StartSyncTimer)
                {
                    pc.syncHits++;
                }
            }

            if (rb != null)
            {
                GameFunctions.ApplyForceToRigidbody(ref rb, e, force);
            }
        }

        ProjectileParticleManager.Instance.fireballExplosionSmoke.transform.position = transform.position;

        ProjectileParticleManager.Instance.fireballExplosionSmoke.Play();
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = transform.position;

        ProjectileParticleManager.Instance.fireballExplode.Emit(emitParams, 5);
    
        //destroy on explosion
        ProjectileParticleManager.Instance.Delete(this);
        Destroy(gameObject);
    }

}
