using UnityEngine;

public class SpawnerEnemy : Enemy
{
    [SerializeField] private SquashAndStretch squashAndStretch;
    public override void FixedUpdate()
    {
        GroundedCheck();
        Targeting();
        if (engage && !attacking)
        {
            Attack();
        }
    }

    public override void Update()
    {
    }

    public override void Routines()
    {
        //no movement
    }

    public override void Targeting()
    {
        if (fov.canSeePlayer)
        {
            memory = 10f;
            engage = true;
        }
        else if (engage && memory > 0)
        {
            fov.radius = newRad;

            memory -= Time.fixedDeltaTime;
            if (memory <= 0)
            {
                fov.radius = oldRad;
                engage = false;
                memory = 0;
            }
        }
    }

    public override void Attack()
    {
        if(attacking || pendingAttack != null)  return;

        AttackBehavior bestChoice = null;

        float bestRange = float.MaxValue;

        foreach(AttackBehavior b in Behaviors)
        {
            if(b.Ready(distance) && b.Range < bestRange)
            {
                bestChoice = b;
                bestRange = b.Range;
            }
        }

        if(bestChoice == null) return;

        beginAttacking = true;
        pendingAttack = bestChoice;

        squashAndStretch.Play();
    }


}
