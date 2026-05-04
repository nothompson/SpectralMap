using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GamePhysics;

public class SpiralBlast : Rocket
{
    [SerializeField] public float ConfuseDur = 5f;

    public override void Start()
    {
        base.Start();
        StartCoroutine(Emit());
    }

    IEnumerator Emit()
    {
        while(true){
            ProjectileParticleManager.Instance.SpawnSorcererCast(transform, 1);
            yield return new WaitForSeconds(0.25f); 
        }
    }
    public override void Explode()
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
                if (e != null)
                {
                    e.enemyVelocity += force;
                }
                if(p != null)
                {
                    EffectManager.Instance.Confuse(p.gameObject, ConfuseDur);
                    EffectManager.Instance.Guilt(p.gameObject, ConfuseDur * 2f);
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
        ProjectileParticleManager.Instance.SpawnSquidBlast(transform);
        Destroy(gameObject);
    }
}
