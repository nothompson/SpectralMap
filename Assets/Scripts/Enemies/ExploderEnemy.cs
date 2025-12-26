using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GamePhysics;
public class ExploderEnemy : Enemy
{

    [Header("Exploder References")]
    public LayerMask enemyMask;
    public LayerMask targetMask;
    public PlayerControlRigid pc;
    public Transform playerFeet;
    public BoxCollider headCollider;
    [Header("Exploder State")]
    public float explodeTimer = 0;
    public float explodeDur = 100f;
    public bool bounced = false;
    public bool exploded = false;
    
    [Header("Explosive Stats")]
    public float bounceHeight = 25f;
    public float explosionRadius = 20f;
    public float explosionForce = 30f;
    public float maximumDamage = 35f;

    public override void Start()
    {
        base.Start();
        playerFeet = player.Find("GroundCheck");
        pc = player.GetComponentInParent<PlayerControlRigid>();
        headCollider = gameObject.GetComponentInChildren<BoxCollider>();
    }
    public override void OnAttack()
    {
        Explode();
    }

    public override void Attack()
    {
        if (fov.canSeePlayer && !attacking)
        {
            fbx.SetTrigger("attacking");
            attacking = true;
        }
    }

    public override void TargetSpotted(Vector3 targetPosition)
    {
        distance = Vector3.Distance(transform.position, targetPosition);
        float angle = Vector3.Angle(transform.position, targetPosition);
        Vector3 adjusted = new Vector3(targetPosition.x, targetPosition.y + 0.33f, targetPosition.z);
        Vector3 direction = (adjusted - transform.position).normalized;

        if (direction != Vector3.zero && !critical)
        {
            Vector3 clamped = direction;
            clamped.y = 0;
            LookTowards(clamped);
        }
        if ((distance > attackDistance && !attacking && !nearLedge) || (jumpAcross && !critical))
        {
            fbx.SetTrigger("moving");
            MoveTowards(direction);
        }

        if (distance <= attackDistance && !attacking)
        {
            fbx.SetTrigger("stopped");
            if (explodeTimer < explodeDur)
            {
                explodeTimer += Time.deltaTime;
            }
            else
            {
                Attack();
            }
        }
        else
        {
            if (explodeTimer > 0)
            {
                explodeTimer -= Time.deltaTime;
            }
        }
        if (distance <= attackDistance * 0.25)
        {
            Flee(attackDistance * 0.5f, attackDistance);
        }
    }

    public override void Update()
    {
        base.Update();
        BounceCheck();
    }

    public void BounceCheck()
    {
        float radius = 0.75f;
        Collider[] hits = Physics.OverlapSphere(playerFeet.position, radius, enemyMask);
        foreach (Collider hit in hits)
        {
            if (hit == headCollider)
            {
                Bounce();
            }
        }
    }

    public void Bounce()
    {
        if (!bounced)
        {
            bounced = true;

            float ymag = Mathf.Abs(pc.playerVelocity.y);

            pc.playerVelocity = new Vector3(pc.playerVelocity.x, ymag + bounceHeight, pc.playerVelocity.z);

            hp.Damage(hp.currentHP);
        }
    }

    public void Explode()
    {
        exploded = true;
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, targetMask);
        HashSet<HP> damagedHP = new HashSet<HP>();

        foreach (Collider hit in hits)
        {
            HP targetHP = hit.GetComponentInParent<HP>();
            Enemy e = hit.GetComponentInParent<Enemy>();
            PlayerControlRigid p = hit.GetComponentInParent<PlayerControlRigid>();
            Vector3 force = GameFunctions.ExplosionForce(hit, transform.position, explosionRadius, explosionForce);

            float damage = GameFunctions.CalculateForceDamage(force, maximumDamage, damageMultiplier);

            Rigidbody rb = hit.attachedRigidbody;
            GameFunctions.ApplyForceToRigidbody(ref rb, e, force);

            if(targetHP != null && !damagedHP.Contains(targetHP))
            {
                if (e != null)
                {
                    e.enemyVelocity += force;
                }
                if(p != null)
                {
                    p.playerVelocity += force;
                }
                damagedHP.Add(targetHP);
                targetHP.Damage(damage);
            }

        }
        hp.Damage(hp.currentHP);
    }

    // public override void Death()
    // {
    //     if (bounced)
    //     {
    //         base.Death();
    //     }
    //     else
    //     {
    //         if(hp.currentHP <= 0f && !dead && !exploded)
    //         {
    //             Explode();
    //             dead = true;
    //             GibsManager.Instance.Gib(transform.position, Random.Range(minGibs, maxGibs));
    //             Destroy(gameObject, 0.1f);
    //         }
    //     }
    // }
}
