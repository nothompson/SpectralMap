using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportEnemy : Enemy
{
    // Start is called before the first frame update

    [Header("Support Class")]
    public LayerMask enemyMask;

    Vector3 allyPosition;

    bool allyFound;
    public override void Start()
    {
        support = true;
        StartCoroutine(FindAllies());
        base.Start();
    }

    public override void Targeting()
    {
        if (allyFound)
        {
            TargetSpotted(allyPosition);

        }
        else
        {
            Flee(10f,30f);
        }
    }

    public IEnumerator FindAllies()
    {
        while (true)
        {
            Collider[] allies = Physics.OverlapSphere(transform.position, 45f, enemyMask);

            if (allies.Length > 0)
            {
                float distThreshold = 45f;

                Vector3 closest = Vector3.positiveInfinity;

                allyFound = false;


                foreach (Collider a in allies)
                {
                    float dist = Vector3.Distance(transform.position, a.transform.position);

                    float YDist = Mathf.Abs(transform.position.y - a.transform.position.y);

                    Enemy e = a.GetComponentInParent<Enemy>();

                    if (YDist < 5 && dist < distThreshold && e.gameObject != this.gameObject && !e.support)
                    {
                        distThreshold = dist;

                        closest = a.transform.position;

                        allyFound = true;
                    }
                }
                allyPosition = closest;

            }

            else
            {
                allyFound = false;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public override void Attack()
    {
        if (allyFound && fov.playerInRange)
        {
            AttackManager.am.ChooseAttack(gameObject, attackType, attackPrefab, attackPoint, attackingCooldown, damage, forceMultiplier, projSpeedMultiplier);
        }
    }
}
