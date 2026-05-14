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
        OnFire?.Invoke(AttackPoint);
        StartCoroutine(Melee());
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator Melee()
    {
        GameObject hitbox = Instantiate(AttackPrefab, AttackPoint.position, AttackPoint.rotation);

        MeleeCollider melee = hitbox.GetComponent<MeleeCollider>();

        GroundSlam slam = hitbox.GetComponent<GroundSlam>();
        

        if(melee != null)
        {
            melee.damage = Damage;
            melee.range = MeleeRadius;
            melee.forceMultiplier = Force;
            melee.forceOffset = ForceOffset;
        }

        if(slam != null)
        {
            slam.owner = Owner;
        }

        Destroy(hitbox, HitBoxLife);
        yield break;
    }
}
