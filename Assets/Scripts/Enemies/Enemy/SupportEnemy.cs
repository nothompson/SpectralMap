using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportEnemy : Enemy
{
    // Start is called before the first frame update

    [Header("Support Class")]
    public LayerMask enemyMask;

    public float supportRadius = 45f;
    public float supportThreshold = 0.85f;

    Vector3 allyPosition;

    bool allyFound;

    bool hasLowHealthAlly;
    public override void Start()
    {
        support = true;
        StartCoroutine(FindAllies());
        base.Start();
    }

    public override void Targeting()
    {
        if(DeathManager.PlayerDead) return;
        if(fov.canSeePlayer)
        {
            memory = 10f;
            distance = Vector3.Distance(transform.position, player.position);
        }

        if (allyFound)
        {
            Vector3 dir = (allyPosition - transform.position).normalized;
            LookTowards(dir);

            float dist = Vector3.Distance(transform.position, allyPosition);

            if(dist > MinRange) MoveTowards(dir);
            
            else
            {
                enemyVelocity.x = Mathf.Lerp(enemyVelocity.x, 0f, Time.fixedDeltaTime * 5f);
                enemyVelocity.z = Mathf.Lerp(enemyVelocity.z, 0f, Time.fixedDeltaTime * 5f);
            }
            if(!attacking && hasLowHealthAlly)
            {
                Attack();
            }
        }
        else
        {
            base.Targeting();
        }
    
    }

    public IEnumerator FindAllies()
    {
        while (true)
        {
            Collider[] allies = Physics.OverlapSphere(transform.position, 45f, enemyMask);

            allyFound = false;
            hasLowHealthAlly = false;

            if (allies.Length > 0)
            {
                float lowestHP = float.MaxValue;
                float closestDistance = float.MaxValue;
                Vector3 bestTarget = Vector3.positiveInfinity;

                foreach (Collider a in allies)
                {
                    Enemy e = a.GetComponentInParent<Enemy>();
                    HP allyHP = a.GetComponentInParent<HP>();
                    if(e == null || allyHP == null) continue;
                    if(e.gameObject == gameObject) continue;
                    if(e.support) continue;

                    float YDist = Mathf.Abs(transform.position.y - a.transform.position.y);

                    float dist = Vector3.Distance(transform.position, a.transform.position);
                    float healthPercent = allyHP.currentHP / allyHP.maxHP;

                    if(healthPercent < supportThreshold)
                    {
                        if(!hasLowHealthAlly || healthPercent < lowestHP)
                        {
                            lowestHP = healthPercent;
                            bestTarget = a.transform.position;
                            hasLowHealthAlly = true;
                            allyFound = true;
                        }
                    }
                    else if(!hasLowHealthAlly && dist < closestDistance)
                    {
                        closestDistance = dist;
                        bestTarget = a.transform.position;
                        allyFound = true;
                    }
                }
                if (allyFound)
                {
                    allyPosition = bestTarget;
                }

            }
            yield return new WaitForSeconds(1f);
        }
    }

    public override void Attack()
    {
        if(attacking || pendingAttack != null) return;

        if(allyFound)
        {
            if(hasLowHealthAlly) SupportAlly();
        }
        else if (fov.canSeePlayer)
        {
            AttackPlayer();
        }
    }

    void SupportAlly()
    {
        AttackBehavior bestChoice = null;
        float bestRange = float.MaxValue;
        float dist = Vector3.Distance(transform.position, allyPosition);

        Debug.Log("trying to support");

        foreach(AttackBehavior b in Behaviors)
        {
            if(!b.Support) continue;
            Debug.Log("distance: " + dist);
            if(b.Ready(dist) && b.Range < bestRange)
            {
                bestChoice = b;
                bestRange = b.Range;
            }
        }

        Debug.Log("chosen: " + bestChoice);
        if(bestChoice == null) return;

        StartCoroutine(Support(bestChoice));
    }

    IEnumerator Support(AttackBehavior behavior)
    {
        beginAttacking = true;
        pendingAttack = behavior;

        if(behavior.Stationary) stationaryAttack = true;

        Vector3 dir = (allyPosition - attackPoint.position).normalized;
        attackPoint.rotation = Quaternion.LookRotation(dir);
        fbx?.SetTrigger(behavior.AnimationEvent);
        // StartCoroutine(AttackTimeout(behavior));

        yield return null;
    }

    void AttackPlayer()
    {
        if(!fov.canSeePlayer) return;

        AttackBehavior bestChoice = null;
        float bestRange = float.MaxValue;

        foreach(AttackBehavior b in Behaviors)
        {
            if(b.Support) continue;
            if(b.Ready(distance) && b.Range < bestRange)
            {
                bestChoice = b;
                bestRange = b.Range;
            }
        }

        if(bestChoice == null) return;

        beginAttacking = true;
        pendingAttack = bestChoice;
        if(bestChoice.Stationary) stationaryAttack = true;
        fbx?.SetTrigger(pendingAttack.AnimationEvent);
        // StartCoroutine(AttackTimeout(bestChoice));
    }

}
