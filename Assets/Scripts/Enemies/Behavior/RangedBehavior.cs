using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangedBehavior : AttackBehavior
{
    [SerializeField] private float ProjectileSpeed = 1f;

    [SerializeField] private float AutoTimer = 1f;
    public override bool Ready(float distance)
    {
        return distance <= Range;
    }

    public override float Begin()
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
            rocket.forceMultiplier = Force;
            rocket.maximumDamage = Damage;
        }

        if(grenade != null)
        {
            grenade.autoTimer = AutoTimer;
        }
        Debug.Log("shooting projectile");

        return Cooldown;
    }
    


}
