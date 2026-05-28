using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovementPhysics;

public class FlyingEnemy : Enemy
{
    [Header("Flying")]
 
    public float hoverOffset = 2f;
 
    public float verticalSpeed = 5f;
 
    public float maxVerticalSpeed = 8f;
 
    public float airDrag = 3f;
    public override void Start()
    {
        base.Start();
        grounded = false;
    }

    public override void Routines()
    {
        StartCoroutine(DodgeRoutine());
    }

    public override void CalculateVelocity()
    {
        Movement();
        Targeting();
        ApplyDrag();
    }

    public override void Movement()
    {
        float speed = new Vector3(enemyVelocity.x, 0f, enemyVelocity.z).magnitude;
 
        if (fbx == null) return;
 
        if (speed < 0.5f)
            fbx.SetTrigger("stopped");
        else
            fbx.SetTrigger("moving");
    }   

    void ApplyDrag()
    {
        enemyVelocity = Vector3.Lerp(enemyVelocity, Vector3.zero, airDrag * Time.fixedDeltaTime);
    }

    public override void MoveTowards(Vector3 direction, Vector3 target)
    {
        float noise   = Mathf.PerlinNoise(Time.time * chaosFrequency + noiseOffset, 0f);
        float bipolar = (noise * 2f - 1f) * movementChaos;
 
        Vector3 flattened = new Vector3(direction.x, 0f, direction.z).normalized;
        Vector3 horiz     = Vector3.Cross(Vector3.up, flattened);
        Vector3 chaosDir  = (flattened + horiz * bipolar).normalized;
        Vector3 wishHoriz = Vector3.Lerp(flattened, chaosDir, chaosBlend).normalized;
 
        Vector3 wishDir = new Vector3(wishHoriz.x, direction.y, wishHoriz.z).normalized;
 
        enemyVelocity = MovementFunctions.Accelerate(enemyVelocity, wishDir, moveSpeed, 10f);
 
        enemyVelocity.y = Mathf.Clamp(enemyVelocity.y, -maxVerticalSpeed, maxVerticalSpeed);
    }

    public override void TargetSpotted(Vector3 targetPosition)
    {
        if(DeathManager.PlayerDead) return;
        if(critical) return;
        
     float heightDiff = transform.position.y - targetPosition.y;
    float blendedOffset = Mathf.Clamp01(heightDiff / hoverOffset) * hoverOffset;
    Vector3 hoverTarget = targetPosition + Vector3.up * blendedOffset;

    distance = Vector3.Distance(transform.position, hoverTarget);
    Vector3 direction = (hoverTarget - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            LookTowards(direction);
        }

        // if ((distance > preferredRange) || (jumpAcross && !critical))
        if (distance > preferredRange && !stationaryAttack)
        {
            MoveTowards(direction, targetPosition);
        }
        else if(distance <= preferredRange && !stationaryAttack)
        {
            switch (Personality)
                {
                    case PersonalityType.Cowardly:
                        UpdateStrafe();
                        if(distance < preferredRange * 0.8f)
                        {
                            MoveTowards(-direction, targetPosition);
                        }
                        break;
                    case PersonalityType.Tactical:
                        UpdateStrafe();
                        if(distance < preferredRange * 0.6f && distance >= MinRange)
                        {
                            MoveTowards(direction, targetPosition);
                        }
                        break;
                    case PersonalityType.Reckless:
                    default:
                        MoveTowards(direction, targetPosition);
                        break;
                }
        }

        if(strafeDir == 0f)
        {
            enemyVelocity.x = Mathf.Lerp(enemyVelocity.x, 0f, Time.fixedDeltaTime * 5f);
            enemyVelocity.z = Mathf.Lerp(enemyVelocity.z, 0f, Time.fixedDeltaTime * 5f);
        }

        if (distance <= MaxRange && !attacking)
        {
            Attack();
        }

        float fleeDist = GetFlyingAvgRange() * 0.5f;
        switch (Personality)
        {
            case PersonalityType.Cowardly:
                if(distance <= fleeDist)
                {
                    FlyingFlee();
                }
                break;
            case PersonalityType.Tactical:
                if(distance <= fleeDist && hp.Critical())
                {
                    FlyingFlee();
                }
                break;
            case PersonalityType.Reckless:
            default:
                break;
        }

    }

    void FlyingFlee()
    {
        Vector3 fleeDir = (transform.position - player.position).normalized;
        MoveTowards(fleeDir, Vector3.zero);
    }

    float GetFlyingAvgRange()
    {
        if(Behaviors == null || Behaviors.Length == 0) return MaxRange;
        float sum = 0f;
        foreach(AttackBehavior b in Behaviors) sum += b.Range;
        return sum / Behaviors.Length;
    }

    protected override void Dodge()
    {
        if (dodged || !fov.canSeePlayer) return;

        Collider[] dodgeDetection = Physics.OverlapSphere(
            transform.position + transform.forward * 1.5f, 7.5f, projectileMask);
        if (dodgeDetection.Length == 0) return;

        Collider closest = null;
        float closestDist = float.MaxValue;
        foreach (Collider c in dodgeDetection)
        {
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < closestDist) { closestDist = d; closest = c; }
        }
        if (closest == null) return;

        Vector3 toProjectile = closest.transform.position - transform.position;
        toProjectile.y = 0f;
        float offset = Vector3.Dot(toProjectile, transform.right);

        float forward = Vector3.Dot(toProjectile.normalized, transform.forward);
        if (forward < 0.25f) return;

        float centerThreshold = 0.25f;

        if (Mathf.Abs(offset) <= centerThreshold * toProjectile.magnitude)
        {
            // Heading straight for us — go up or down
            Vector3 dodgeDir = Random.value > 0.5f ? Vector3.up : Vector3.down;
            enemyVelocity += dodgeDir * dodgeSpeed;
        }
        else
        {
            Vector3 dodgeDir = offset > 0f ? -transform.right : transform.right;
            enemyVelocity += dodgeDir * dodgeSpeed;
        }

        enemyVelocity.y = Mathf.Clamp(enemyVelocity.y, -maxVerticalSpeed, maxVerticalSpeed);
        dodged = true;
    }

    public override void LookTowards(Vector3 direction)
    {
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5f);

        Quaternion desiredAttackRotation = Quaternion.Slerp(attackPoint.rotation, lookRotation, Time.fixedDeltaTime * 5f);

        attackPoint.localRotation = Quaternion.Inverse(transform.rotation) * desiredAttackRotation;
    }

}
