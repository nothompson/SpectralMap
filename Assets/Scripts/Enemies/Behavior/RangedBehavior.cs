using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangedBehavior : AttackBehavior
{
    [SerializeField] private float ProjectileSpeed = 1f;

    [SerializeField] private float AutoTimer = 1f;

    [SerializeField] private float ExplosionRadius = 1f;

    public override void Fire()
    {

        GameObject hitbox = Instantiate(AttackPrefab, AttackPoint.position, AttackPoint.rotation);

        EnemyProjectile projectile = hitbox.GetComponent<EnemyProjectile>();

        Rocket rocket = hitbox.GetComponent<Rocket>();

        Grenade grenade = hitbox.GetComponent<Grenade>();

        if(projectile != null)
        {
            projectile.damage = Damage;
            projectile.thisEnemy = Owner;
            projectile.speed *= ProjectileSpeed;
        }

        if(rocket != null)
        {
            rocket.explosionForce = Force;
            rocket.maximumDamage = Damage;
            rocket.explosionRadius = ExplosionRadius;
        }

        if(grenade != null)
        {
            grenade.autoTimer = AutoTimer;
        }

        StartCoroutine(CooldownRoutine());
    }
    


}
