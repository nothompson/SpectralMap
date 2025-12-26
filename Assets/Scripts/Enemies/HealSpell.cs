using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSpell : EnemyProjectile
{
    public override IEnumerator Hit()
    {
        if (collided && enemyHP != null)
        {
            enemyHP.Heal(0.25f);
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(gameObject);
    }
}
