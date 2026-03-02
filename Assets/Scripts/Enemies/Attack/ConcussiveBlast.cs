using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcussiveBlast : MeleeCollider 
{
    public override IEnumerator Collide(GameObject player, PlayerControlRigid control, HP hp, Vector3 force)
    {
        if (player != null){
            EffectManager.effectManager.Confuse(player, 5f, 1f);
            }

        yield return base.Collide(player, control, hp, force);
    }
}
