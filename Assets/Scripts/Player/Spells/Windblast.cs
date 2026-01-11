using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Windblast : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    public Transform attackPoint;
    public LayerMask enemyMask;

    public LayerMask projectileMask;

    [Header("General")]
    public float blastRadius = 5f;
    public float windForce = 10f;

    float timer;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0.33f;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            transform.position = attackPoint.position + attackPoint.forward * 2f;

            Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius, enemyMask);
            foreach (Collider hit in hits)
            {
                IKnockback knockback = hit.GetComponentInParent<IKnockback>();
                if (knockback != null)
                {

                    Vector3 dir = (hit.transform.position - transform.position).normalized;

                    float dist = Vector3.Distance(hit.transform.position, transform.position);

                    float inverse = 1.0f - Mathf.Clamp01(dist / blastRadius);

                    Vector3 force = dir * windForce * inverse;

                    force.y *= 0.15f;

                    knockback.AddKnockback(force);
                }
            }

            Collider[] reflects = Physics.OverlapSphere(transform.position, blastRadius * 0.5f, projectileMask);
            foreach (Collider reflect in reflects)
            {
                IKnockback knockback = reflect.GetComponentInParent<IKnockback>();
                if (knockback != null)
                {

                    knockback.AddKnockback(attackPoint.position);
                }
            }

            timer -= Time.deltaTime;
        }
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

}
