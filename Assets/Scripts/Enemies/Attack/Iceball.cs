using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iceball : EnemyProjectile
{
    
    public override IEnumerator Hit()
    {
        if (!reflected)
        {
            if (player != null)
                EffectManager.effectManager.Weak(player, 5f, 0.6f);
        }
        else
        {
            if (enemy != null)  
            {
                EffectManager.effectManager.Weak(enemy, 5f, 0.75f);
            }
        }

        yield return base.Hit();
    }
}
