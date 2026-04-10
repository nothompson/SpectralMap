using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCollider : MonoBehaviour
{
    [Header("Reference")]
    public LayerMask playerMask;
    [Header("State")]
    bool collided = false;
    [Header("General")]
    public float damage;
    public float range = 5f;
    public float forceMultiplier = 1f;
      public Vector3 forceOffset = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, playerMask);
        if (hits.Length != 0 && !collided)
        {
                var capsule = hits[0].gameObject;
                var player = capsule.gameObject;
                var control = player.GetComponent<PlayerControlRigid>();
                var playerHP = player.GetComponent<HP>();

                Vector3 dir = (hits[0].transform.position - transform.position).normalized;

                float dist = Vector3.Distance(hits[0].transform.position, transform.position);

                float inverse = 1.0f - Mathf.Clamp01(dist / range);
                
                Vector3 force = dir * damage * forceMultiplier * inverse;

                force += forceOffset;

                collided = true;

                StartCoroutine(Collide(player, control, playerHP, force));
        }
    }

    public virtual IEnumerator Collide(GameObject player, PlayerControlRigid control, HP hp, Vector3 force)
    {
        yield return new WaitForSeconds(0.1f);

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
