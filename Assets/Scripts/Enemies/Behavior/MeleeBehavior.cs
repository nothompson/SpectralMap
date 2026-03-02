using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeBehavior : AttackBehavior
{
    [SerializeField] public float HitBoxLife;

    public override bool Ready(float distance)
    {
        return distance <= Range;
    }

    public override float Begin()
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
            melee.range = Range;
            melee.forceMultiplier = Force;
        }

        Debug.Log("melee!");

        Destroy(hitbox, HitBoxLife);
        yield break;
    }
}
