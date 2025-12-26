using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : EnemyProjectile
{
    // Start is called before the first frame update
    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            Destroy(gameObject);
        }

        base.OnTriggerEnter(other);
    }
}
