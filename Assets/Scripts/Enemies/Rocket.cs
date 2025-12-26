using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : EnemyProjectile
{
    [Header("Explosive Stats")]
    public float explosionRadius;
    public float explosionForce;
    public float maximumDamage;
    public float damageMultiplier;
    public float forceMultiplier;

    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            collided = true;
            StartCoroutine(Hit());
        }

        base.OnTriggerEnter(other);
    }

    public override IEnumerator Hit()
    {
        if (collided)
        {
            Explode();
        }
        yield return new WaitForSeconds(0.25f);
        Destroy(gameObject);
        collided = false;
    }
    public void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, targetMask);
        HashSet<IKnockback> processed = new HashSet<IKnockback>();
    
        foreach (Collider hit in hits)
        {
            HP hp = hit.GetComponentInParent<HP>();
            IKnockback knockback = hit.GetComponentInParent<IKnockback>();
            if (knockback != null && !processed.Contains(knockback))
            {
                processed.Add(knockback);

                //get angle of rocket explosion from player
                Vector3 dir = (hit.transform.position - transform.position).normalized;

                //distance from explosion radius origin to player origin
                float dist = Vector3.Distance(hit.transform.position, transform.position);

                //inversely proportional magnitude (so player gets blasted away from rockets instead of the direction they were shot)
                float inverse = 1.0f - Mathf.Clamp01(dist / explosionRadius);

                //calculate force 
                Vector3 force = dir * explosionForce * inverse * forceMultiplier;

                force.y += 1.0f * inverse;

                float mag = force.magnitude;

                float dmg = Mathf.Clamp(mag, 0, maximumDamage);

                dmg *= damageMultiplier;

                //call player function for adding velocity to player
                if (reflected)
                {

                    knockback.addKnockback(force);
                    hp.Damage(dmg * 2f);
                }
                else
                    knockback.addKnockback(force);
                    hp.Damage(dmg);
            }
        }
        Destroy(gameObject);
    }
}
