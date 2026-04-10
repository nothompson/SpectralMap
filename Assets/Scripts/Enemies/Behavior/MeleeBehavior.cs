using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeBehavior : AttackBehavior
{
    [SerializeField] public float HitBoxLife;
    [SerializeField] public float MeleeRadius;
    [SerializeField] public Vector3 ForceOffset;

    public override void Fire()
    {
        StartCoroutine(Melee());
        StartCoroutine(CooldownRoutine());
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
            melee.forceOffset = ForceOffset;
        }

        Destroy(hitbox, HitBoxLife);
        yield break;
    }
}
