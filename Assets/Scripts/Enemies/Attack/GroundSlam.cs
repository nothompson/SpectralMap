using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSlam : MeleeCollider
{
    public override void Update()
    {
        // Collider[] hits = Physics.OverlapSphere(transform.position, range, playerMask);
        // if (hits.Length != 0 && !collided)
        // {
        //         var capsule = hits[0].gameObject;
        //         var player = capsule.gameObject;
        //         var control = player.GetComponent<PlayerControlRigid>();
        //         var playerHP = player.GetComponent<HP>();

        //         Vector3 pos = hits[0].transform.position;

        //         Transform t = hits[0].transform;

        //         Vector3 dir = (pos - transform.position).normalized;

        //         float dist = Vector3.Distance(pos, transform.position);

        //         float inverse = 1.0f - Mathf.Clamp01(dist / range);
                
        //         Vector3 force = dir * damage * forceMultiplier * inverse;

        //         force += forceOffset;

        //         collided = true;

        //         StartCoroutine(Collide(player, control, playerHP, force, t));
        // }
    }
}