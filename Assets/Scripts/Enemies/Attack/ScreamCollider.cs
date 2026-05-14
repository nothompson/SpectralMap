using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamCollider : MeleeCollider
{
    public override IEnumerator Collide(GameObject player, PlayerControlRigid control, HP hp, Vector3 force, Transform t)
    {
        yield return new WaitForSeconds(0.1f);

        EffectManager.Instance.Guilt(player, 5f);
        
        if (hp != null)
        {
            hp.Damage(damage);
        }

        if(control != null){
            control.AddKnockback(force);
        }

        yield return new WaitForSeconds(1.0f);
        collided = false;
    }
}