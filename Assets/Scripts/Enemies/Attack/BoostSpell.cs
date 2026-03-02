using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostSpell : EnemyProjectile
{
    public override IEnumerator Hit()
    {
        if (collided && enemy != null)
        {
            EffectManager.effectManager.Boost(enemy, 10f, 2f);
            yield return new WaitForSeconds(0.05f);
        }
        else if (collided && player != null)
        {
            EffectManager.effectManager.Boost(player, 2f, 1.5f);
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(gameObject);
    }

    public override void Update()
    {
        base.Update();
    }
    
}
