using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GamePhysics;

public class Grenade : Rocket
{
    [Header("Grenade Params")]
    public float arc = 5f;
    public override void Start()
    {
        autoTimer -= 1f;
        base.Start();
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y + arc, rb.linearVelocity.z);
    }
    public override void Update()
    {
            autoTimer -= Time.deltaTime;

            if(autoTimer <= 0)
            {
                Explode();
            }
    }

    public void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.layer == 3) Explode();
    }

    public override void Explode()
    {
        ProjectileParticleManager.Instance.SpawnSkulls(transform);
        ProjectileParticleManager.Instance.SpawnPollutantBlast(transform);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, targetMask);
        HashSet<HP> damagedHP = new HashSet<HP>();
    
        foreach (Collider hit in hits)
        {
            HP targetHP = hit.GetComponentInParent<HP>();
            Enemy e = hit.GetComponentInParent<Enemy>();
            PlayerControlRigid p = hit.GetComponentInParent<PlayerControlRigid>();
            Vector3 force = GameFunctions.TargetedExplosionForce(hit, transform.position, explosionRadius, explosionForce);

            float damage = GameFunctions.CalculateForceDamage(hit, transform.position, explosionRadius, maximumDamage, damageMultiplier);

            Rigidbody rb = hit.attachedRigidbody;
            GameFunctions.ApplyForceToRigidbody(ref rb, e, force);

            if(targetHP != null && !damagedHP.Contains(targetHP))
            {
                Debug.Log(p);
                if (e != null)
                {
                    e.enemyVelocity += force;
                }
                if(p != null)
                {
                    p.playerVelocity += force;
                    float rand = Random.Range(0f,1f);
                    if(rand <= 0.1f)
                    {
                    EffectManager.Instance.Infected(p.gameObject, 5f);
                    }
                    else if(rand >= 0.9f)
                    {
                    EffectManager.Instance.Polluted(p.gameObject, 5f);
                    }
                }
                damagedHP.Add(targetHP);
                if (direct)
                {
                        targetHP.Damage(maximumDamage);
                }
                else {
                targetHP.Damage(damage);
                }
            }
        }
        Destroy(gameObject);
    }
}
