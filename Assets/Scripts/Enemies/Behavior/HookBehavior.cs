using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HookBehavior : PredictedRangedBehavior
{
    public override void Fire()
    {
        predictedTarget = PredictTarget();
        ShootHook(predictedTarget);
        StartCoroutine(CooldownRoutine());
    }

    void ShootHook(Vector3 target)
    {
        if (AttackPrefab == null || AttackPoint == null) return;

        Vector3 dir = (target - AttackPoint.position).normalized;

        GameObject hitbox = Instantiate(AttackPrefab, AttackPoint.position, Quaternion.LookRotation(dir));

        TongueHook hook = hitbox.GetComponent<TongueHook>();

        EnemyProjectile projectile = hitbox.GetComponent<EnemyProjectile>();

        if (projectile != null)
        {
            projectile.damage = Damage;
            projectile.thisEnemy = Owner;
            projectile.speed = projectileSpeed;
        }

        hook.attackPoint = AttackPoint;

    }
}
