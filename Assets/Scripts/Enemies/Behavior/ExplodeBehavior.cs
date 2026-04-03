using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePhysics;

public class ExplodeBehavior : AttackBehavior
{
    [SerializeField] private float ExplosionRadius = 1f;
    [SerializeField] private LayerMask targetMask;

    private HP ownerHP;
    private Enemy ownerScript;

    public override void InitBehavior(GameObject enemy, Transform point)
    {
        base.InitBehavior(enemy, point);

        ownerHP = enemy.GetComponent<HP>();
        ownerScript = enemy.GetComponentInParent<Enemy>();
        if(ownerScript == null) Debug.Log("enemy is null");
        if(ownerHP == null) Debug.Log("HP on enemy is null");
    }
    public override float Fire()
    {
        StartCoroutine(Explode());
        return Cooldown;
    }

    public IEnumerator Explode()
    {
        ownerScript.attacking = true;
        Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRadius, targetMask);
        HashSet<HP> damagedHP = new HashSet<HP>();

        foreach (Collider hit in hits)
        {
            HP targetHP = hit.GetComponentInParent<HP>();
            Enemy e = hit.GetComponentInParent<Enemy>();
            PlayerControlRigid p = hit.GetComponentInParent<PlayerControlRigid>();
            Vector3 force = GameFunctions.TargetedExplosionForce(hit, transform.position, ExplosionRadius, Force);

            float damage = GameFunctions.CalculateForceDamage(hit, transform.position, ExplosionRadius, Damage, 1.0f);

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
        
        ownerHP.Damage(ownerHP.currentHP);

        yield break;
    }

}
