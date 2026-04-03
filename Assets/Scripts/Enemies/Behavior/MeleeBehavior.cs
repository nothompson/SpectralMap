using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeBehavior : AttackBehavior
{
    [SerializeField] public float HitBoxLife;
    [SerializeField] public float MeleeRadius;

    public override float Fire()
    {
        StartCoroutine(Melee());
        return Cooldown;
    }

    IEnumerator Melee()
    {
        GameObject hitbox = Instantiate(AttackPrefab, AttackPoint.position, AttackPoint.rotation);

        MeleeCollider melee = hitbox.GetComponent<MeleeCollider>();

        if(melee != null)
        {
            melee.damage = Damage;
            melee.range = MeleeRadius;
            melee.forceMultiplier = Force;
        }

        Destroy(hitbox, HitBoxLife);
        yield break;
    }
}
