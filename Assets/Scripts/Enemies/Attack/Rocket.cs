using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePhysics;

public class Rocket : EnemyProjectile
{
    [Header("Explosive Stats")]
    public float explosionRadius;
    public float explosionForce;
    public float maximumDamage;
    public float damageMultiplier;
    public float forceMultiplier;

    [HideInInspector] public bool direct = false;

    public override void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 11 || other.gameObject.layer == 24) return;

        if(other.gameObject.layer == 3) direct = true;
        collided = true;
        Explode();
    }

    public override IEnumerator Hit()
    {
        if (collided)
        {
            Explode();
        }
        yield return new WaitForSeconds(0.25f);
        collided = false;
    }
    public virtual void Explode()
    {
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

    void OnDrawGizmosSelected()
{
    Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
    Gizmos.DrawSphere(transform.position, explosionRadius);
    Gizmos.color = new Color(1f, 0.3f, 0f, 1f);
    Gizmos.DrawWireSphere(transform.position, explosionRadius);
}
}
