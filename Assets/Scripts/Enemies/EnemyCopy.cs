using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementPhysics;

#region Init
public class EnemyCopy : MonoBehaviour, IKnockback
{
    [Header("References")]
    public CharacterController controller = null;

    public Transform player;

    public Launcher launcher;

    public GameObject attackPrefab;

    public LayerMask projectileMask;

    public LayerMask groundMask;
    public LayerMask pickupMask;

    public Transform attackPoint;

    public FOV fov;

    public HP hp;

    public Animator fbx; 

    [Header("General")]
    [Range(1, 2)]
    public int attackType;
    public bool support;
    public float attackDistance;

    public float memory = 15f;

    public float moveSpeed;
    public float friction = 5f;
    public float deacelleration = 10f;
    public float jumpHeight = 10f;

    public float dodgeCooldown = 1f;
    public float dodgeSpeed = 40f;
    public float attackingCooldown = 1f;
    public float damage;
    public float forceMultiplier = 1f;
    public float projSpeedMultiplier = 1f;

    public int minGibs = 5;
    public int maxGibs = 20;

    [Header("State")]
    public bool air;
    public float airTimer;
    public bool engage = false;
    public bool dodged = false;
    public bool attacking = false;
    public bool critical;
    public bool debuffed;
    public Vector3 enemyVelocity;
    public float distance;
    public bool jumpAcross;
    public bool nearLedge;
    public bool dead = false;
    float distanceFromPlayer;
    //inner variables
    bool infront;
    bool behind;
    float newRad;
    float oldRad;
    float newDist;
    float oldDist;
    Vector3 pickupPosition;
    Vector3 impact;

    //audio triggers


    // Start is called before the first frame update
    public virtual void Start()
    {
        newRad = fov.radius * 2f;
        oldRad = fov.radius;

        newDist = attackDistance * 2;
        oldDist = attackDistance;

        GameObject playerRef = GameObject.FindWithTag("Player");
        if (playerRef != null)
            player = playerRef.transform;

        GameObject launcherRef = GameObject.FindWithTag("Launcher");
        if (launcherRef != null)
            launcher = launcherRef.GetComponent<Launcher>();

        controller = GetComponent<CharacterController>();
        enemyVelocity = new Vector3(0, 0, 0);

        StartCoroutine(DodgeRoutine());
        StartCoroutine(Leap());
        StartCoroutine(CriticalCheck());

        fbx = GetComponentInChildren<Animator>();

    }

    #endregion

    // Update is called once per frame
    public virtual void Update()
    {
        Movement();
        Targeting();
        DodgeCooldown();

        if (critical)
        {
            FindHealth();
        }

        Death();
    }

    #region Targeting Behavior

    public virtual void Targeting()
    {
        if (fov.canSeePlayer && !critical)
        {
            memory = 10f;
            engage = true;
            TargetSpotted(player.position);
        }
        else if (engage && memory > 0)
        {
            fov.radius = newRad;
            if (attackType == 2)
            {
                attackDistance = newDist;
            }

            memory -= Time.deltaTime;
            TargetSpotted(player.position);
            if (memory <= 0)
            {
                fov.radius = oldRad;
                engage = false;
                memory = 0;
                if (attackType == 2)
                {
                    attackDistance = oldDist;
                }
            }
        }
        if (!engage)
        {
             fbx.SetTrigger("stopped");
        }
    }

    public virtual void TargetSpotted(Vector3 targetPosition)
    {
        distance = Vector3.Distance(transform.position, targetPosition);
        float angle = Vector3.Angle(transform.position, targetPosition);
        Vector3 adjusted = new Vector3(targetPosition.x, targetPosition.y + 0.33f, targetPosition.z);
        Vector3 direction = (adjusted - transform.position).normalized;

        if (direction != Vector3.zero && !critical)
        {
            LookTowards(direction);
        }
        if ((distance > attackDistance && !attacking && !nearLedge) || (jumpAcross && !critical))
        {
            MoveTowards(direction);
        }

        if (distance <= attackDistance && !attacking)
        {
            Attack();
        }
        if (distance <= attackDistance * 0.25)
        {
            Flee(attackDistance * 0.5f, attackDistance);
        }
    }

    public void LookTowards(Vector3 direction)
    {
        Vector3 clamped = direction;
        clamped.y = Mathf.Clamp(direction.y, -0.15f, 0.15f);

        Quaternion lookRotationClamped = Quaternion.LookRotation(clamped);
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotationClamped, Time.deltaTime * 5f);

        Quaternion desiredAttackRotation = Quaternion.Slerp(attackPoint.rotation, lookRotation, Time.deltaTime * 5f);

        attackPoint.localRotation = Quaternion.Inverse(transform.rotation) * desiredAttackRotation;
    }

    public void MoveTowards(Vector3 direction)
    {
        fbx.SetTrigger("moving");
        enemyVelocity.x = Mathf.Lerp(enemyVelocity.x, direction.x * moveSpeed, 5f * Time.deltaTime);
        enemyVelocity.z = Mathf.Lerp(enemyVelocity.z, direction.z * moveSpeed, 5f * Time.deltaTime);
    }

    #endregion

    #region Movement
    public void Movement()
    {
        inAir();
        LedgeCheck();
        if (air)
        {
            applyFriction(0.75f);
        }
        else
            applyFriction(1.0f);

        if (impact.magnitude > 5.0f)
        {
            Blast();
        }

        if (!controller.isGrounded || enemyVelocity.y > 0)
        {
            enemyVelocity.y += -20f * Time.deltaTime;
        }
        else if (controller.isGrounded || enemyVelocity.y < 0)
        {
            enemyVelocity.y = 0;
        }

        float enemymovement = enemyVelocity.magnitude;

        if(enemymovement < 1f)
        {
            fbx.SetTrigger("stopped");
        }
        

        controller.Move(enemyVelocity * Time.deltaTime);

    }
    public void inAir()
    {
        if (!controller.isGrounded)
        {
            if (airTimer <= 0f && !air)
            {
                airTimer = 0.15f;
            }

            if (airTimer > 0f)
            {
                airTimer -= Time.deltaTime;
                if (airTimer <= 0f)
                {
                    air = true;
                }
            }
        }
        else if (controller.isGrounded)
        {
            air = false;
            airTimer = 0f;
        }
    }

    public void addKnockback(Vector3 force)
    {
        impact += force;
    }

    public void applyFriction(float t)
    {
        enemyVelocity = MovementFunctions.ApplyFriction(t, enemyVelocity, friction, deacelleration);
    }

    public IEnumerator DodgeRoutine()
    {
        float del = 0.5f;
        WaitForSeconds wait = new WaitForSeconds(del);
        while (true)
        {
            yield return wait;
            Dodge();
        }
    }

    public IEnumerator Leap()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);

        while (true)
        {
            yield return wait;

            if (!attacking && engage && distance > attackDistance || critical)
            {
                float chance = Random.value;
                if ((chance < 0.15 && !nearLedge && !air) || jumpAcross && !air)
                {
                    Vector3 jump = transform.forward *= 2f;
                    jump.y = jumpHeight;
                    enemyVelocity += jump;
                    jumpAcross = false;
                }
            }
        }
    }

    public void LedgeCheck()
    {
        if (!air)
        {
            Vector3 frontledgePosition = transform.position + transform.forward * 1.5f;
            Vector3 backledgePosition = transform.position + transform.forward * -2.0f;

            infront = !Physics.Raycast(frontledgePosition, Vector3.down, 5f, groundMask);
            behind = !Physics.Raycast(backledgePosition, Vector3.down, 5f, groundMask);

            if (infront || behind)
            {
                fbx.SetTrigger("stopped");
                nearLedge = true;
            }
            else
            {
                nearLedge = false;
            }

            // Debug.Log("near ledge? " + nearLedge + ", in front?: " + infront + ", behind?: " + behind);

            float jumping = Mathf.Abs(enemyVelocity.y);

            if (infront && distance > 8f && jumping <= 0.05f)
            {
                jumpAcross = false;
            }
            else if (infront && distance <= 8f)
            {
                fbx.SetTrigger("moving");
                jumpAcross = true;
            }
        }
    }

    public void DodgeCooldown()
    {
        if (dodgeCooldown > 0 && dodged)
        {
            dodgeCooldown -= Time.deltaTime;
        }
        if (dodgeCooldown <= 0)
        {
            dodged = false;
            dodgeCooldown = 1f;
        }
    }

    public void Dodge()
    {
        Collider[] dodgeDetection = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 7.5f, projectileMask);
        if (dodgeDetection.Length != 0 && !dodged)
        {
            if (fov.canSeePlayer && !air)
            {
                Vector3 directionFromPlayer = (player.position - transform.position).normalized;
                directionFromPlayer.y = 0;

                Vector3 dodgeDirection = Vector3.Cross(Vector3.up, directionFromPlayer).normalized;

                if (Random.value > 0.5f)
                {
                    dodgeDirection *= -1f;
                }

                enemyVelocity += dodgeDirection * dodgeSpeed;

                dodged = true;
            }
        }
    }

    public void Blast()
    {
        float groundScaledY = Mathf.Clamp(impact.y, -15f, 50f);
        float airScaledY = Mathf.Clamp(impact.y, -15f, 15f);
        impact.x *= 2.5f;
        impact.z *= 2.5f;
        // impact.y *= 2.5f;

        if (!air)
            impact.y = groundScaledY;
        else
            impact.y = airScaledY;

        enemyVelocity += impact;

        if (launcher.spell == 2)
        {
            air = true;
            nearLedge = false;
        }

        if (!engage)
        {
            engage = true;
            memory = 15f;
        }

        //reset so it doesnt accumulate
        impact = Vector3.zero;
    }

    #endregion

    #region Disengage
    public void Flee(float min, float max)
    {
        fbx.SetTrigger("moving");
        Vector3 fleeDir = (player.position - transform.position).normalized;
        distanceFromPlayer = Vector3.Distance(transform.position, player.position);
        fleeDir.y = Mathf.Clamp(fleeDir.y, -0.2f, 0.25f);
        if (!nearLedge) {
            if (distanceFromPlayer <= min)
            {
                LookTowards(fleeDir * 1f);
                MoveTowards(fleeDir * -1f);
            }
            else if (distanceFromPlayer > min && distanceFromPlayer < max)
            {
                LookTowards(fleeDir * -1f);
                MoveTowards(fleeDir * -1f);
            }
        }
    }

    public IEnumerator CriticalCheck()
    {
        while (true)
        {
            if (hp.Critical() || debuffed)
            {
                Collider[] pickup = Physics.OverlapSphere(transform.position, 20f, pickupMask);

                if (pickup.Length > 0)
                {
                    float distThreshold = 20f;

                    Vector3 closest = transform.position;


                    foreach (Collider p in pickup)
                    {
                        float dist = Vector3.Distance(transform.position, p.transform.position);

                        float YDist = Mathf.Abs(transform.position.y - p.transform.position.y);

                        if (YDist >= 2f)
                        {
                            critical = false;
                            engage = true;
                        }
                        else if (YDist < 2 && dist < distThreshold)
                        {
                            distThreshold = dist;
                            closest = p.transform.position;

                            critical = true;
                            engage = false;

                            if (dist < 5f)
                            {
                                jumpAcross = true;
                            }

                        }
                    }
                    pickupPosition = closest;

                }
            }

            else
            {
                critical = false;
            }

            yield return new WaitForSeconds(8f);

        }
    }

    public void FindHealth()
    {
        Vector3 dir = (pickupPosition - transform.position).normalized;
        MoveTowards(dir);
        LookTowards(dir);
    }
    #endregion

    #region Combat

    public virtual void Attack()
    {
        if (fov.canSeePlayer && !attacking)
        {
            fbx.SetTrigger("attacking");
            attacking = true;
        }
    }

    public virtual void OnAttack()
    {
        StartCoroutine(AttackCooldown());
        AttackManager.am.ChooseAttack(gameObject, attackType, attackPrefab, attackPoint, attackingCooldown, damage, forceMultiplier, projSpeedMultiplier);
    }

    public virtual IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackingCooldown);
        attacking = false;
    }

    public virtual void Death()
    {
        if(hp.currentHP <= 0f)
        {
            if (!dead)
            {
                dead = true;
                GibsManager.Instance.Gib(transform.position, Random.Range(minGibs,maxGibs));
                Destroy(gameObject);
            }
        }
    }
    #endregion
}
