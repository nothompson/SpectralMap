using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementPhysics;

#region Init
public class Enemy : MonoBehaviour, IKnockback
{
    [Header("References")]
    public Rigidbody rb;

    public Transform player;

    public Launcher launcher;

    public GameObject attackPrefab;

    public LayerMask projectileMask;

    public LayerMask groundMask;
    public LayerMask resetMask;
    public Transform GroundCheck;
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
    public float minVariance = 1f;
    public float maxVariance = 1f;
    public bool variance = false;

    public float memory = 15f;

    public float moveSpeed;
    public float friction = 5f;
    [Range(0,2)]
    public float airSpeedFactor = 0.5f;
    public float deacelleration = 10f;
    public float jumpHeight = 10f;
    [Range(0,1)]
    public float knockbackResistance = 0f;
    public float GroundDistance = 0.4f;
    public float dodgeCooldown = 1f;
    public float dodgeSpeed = 20f;
    public float attackingCooldown = 1f;
    public float damage;
    public float damageMultiplier = 1f;
    public float forceMultiplier = 1f;
    public float projSpeedMultiplier = 1f;

    public int minGibs = 5;
    public int maxGibs = 20;

    [Header("State")]
    public bool grounded;
    public float airTimer;
    public bool engage = false;
    public bool dodged = false;
    public bool attacking = false;
    public bool beginAttacking = false;
    public bool critical;
    public bool debuffed;
    public Vector3 enemyVelocity;
    public Vector3 moveVelocity;
    public Vector3 knockVelocity;
    public float distance;
    public bool jumpAcross;
    public bool nearLedge;
    public bool dead = false;
    float distanceFromPlayer;
    //inner variables
    bool infront;
    float coyoteTime = 0.1f;
    float groundTimer = 0f;
    bool behind;
    float newRad;
    float oldRad;
    float newDist;
    float oldDist;
    Vector3 pickupPosition;
    Vector3 impact;
    float cooldown;
    float initSpeed;
    float airSpeed;

    Vector3 groundNormal;

    //audio triggers


    // Start is called before the first frame update
    public virtual void Start()
    {
        References();
        if(variance){
            Variance();
        }
        Init();
        Routines();
    }

    void Init()
    {
        cooldown = dodgeCooldown;

        newRad = fov.radius * 2f;
        oldRad = fov.radius;

        newDist = attackDistance * 2;
        oldDist = attackDistance;

        initSpeed = moveSpeed;
        airSpeed = moveSpeed * airSpeedFactor;

        // moveVelocity = new Vector3(0, 0, 0);
        // knockVelocity = new Vector3(0, 0, 0);

    }

    void Variance()
    {
        float rand = Random.Range(minVariance, maxVariance);
        float norm = rand / maxVariance;
        float bipolar = (norm * 2f) - 1f;
        transform.localScale *= rand;
        hp.maxHP *= rand;
        knockbackResistance *= rand;
        moveSpeed -= bipolar;
        attackDistance *= rand;
        if(attackDistance < 2f)
        {
            attackDistance = 2f;
        }
        damageMultiplier *= rand;
    }

    void References()
    {
        GameObject playerRef = GameObject.FindWithTag("Player");
        if (playerRef != null)
            player = playerRef.transform;

        GameObject launcherRef = GameObject.FindWithTag("Launcher");
        if (launcherRef != null)
            launcher = launcherRef.GetComponent<Launcher>();

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        fbx = GetComponentInChildren<Animator>();
    }
    
    void Routines()
    {
        StartCoroutine(DodgeRoutine());
        StartCoroutine(Leap());
        StartCoroutine(CriticalCheck());
    }

    #endregion

    // Update is called once per frame
    public virtual void Update()
    {
        DodgeCooldown();
        Death();
    }

    public virtual void FixedUpdate()
    {
        CalculateVelocity();
        MovementFunctions.ApplyVelocity(enemyVelocity, ref rb);
    }

    void CalculateVelocity()
    {
        Movement();
        Targeting();
        if (critical)
        {
            FindHealth();
        }

        if (!grounded)
        {
            MovementFunctions.ApplyGravity(ref enemyVelocity);
        }

        // knockVelocity = Vector3.Lerp(knockVelocity, Vector3.zero, Time.fixedDeltaTime * knockbackDecay);
        
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

            memory -= Time.fixedDeltaTime;
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

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotationClamped, Time.fixedDeltaTime * 5f);

        Quaternion desiredAttackRotation = Quaternion.Slerp(attackPoint.rotation, lookRotation, Time.fixedDeltaTime * 5f);

        attackPoint.localRotation = Quaternion.Inverse(transform.rotation) * desiredAttackRotation;
    }
    
    public void LookClamp()
    {
        
    }

    public void MoveTowards(Vector3 direction)
    {
        fbx.SetTrigger("moving");
        Vector3 wishDir = new Vector3(direction.x, 0, direction.z).normalized;
        float wishSpeed = moveSpeed;
        enemyVelocity = MovementFunctions.Accelerate(enemyVelocity, wishDir, wishSpeed, 10f);
    }

    #endregion

    #region Movement
    public void Movement()
    {
        GroundedCheck();
        LedgeCheck();
        if (!grounded)
        {
            // MovementFunctions.ApplyGravity(ref enemyVelocity);
            applyFriction(0.5f);
            moveSpeed = airSpeed;

        }
        else
        {
            applyFriction(1.0f);
            moveSpeed = initSpeed;
        }


        float enemymovement = rb.linearVelocity.magnitude;

        if (enemymovement < 1f)
        {
            fbx.SetTrigger("stopped");
        }

        // Debug.Log("x: " + transform.localRotation.x + " " + "y: " + transform.localRotation.y + " " + "z: " + transform.localRotation.z);

    }
    public void GroundedCheck()
    {
        bool reset; 
        grounded = MovementFunctions.GroundedCheck(
            GroundCheck,
            GroundDistance,
            groundMask,
            ref enemyVelocity,
            ref groundTimer,
            coyoteTime, ref groundNormal, out RaycastHit groundhit
        );
        reset = MovementFunctions.ResetCheck(
            GroundCheck,
            GroundDistance,
            resetMask
        );

        if (reset)
        {
            hp.Damage(hp.currentHP);
        }
    }

    public void AddKnockback(Vector3 force)
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
                if ((chance < 0.15 && !nearLedge && grounded) || jumpAcross && grounded)
                {
                    enemyVelocity.y = jumpHeight;
                    jumpAcross = false;
                }
            }
        }
    }

    public void LedgeCheck()
    {
        if (grounded)
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
            else if (infront && distance <= 5f)
            {
                fbx.SetTrigger("moving");
                jumpAcross = true;
            }
        }
    }

    public void DodgeCooldown()
    {
        if (cooldown > 0 && dodged)
        {
            cooldown -= Time.deltaTime;
        }
        if (cooldown <= 0)
        {
            dodged = false;
            cooldown = dodgeCooldown;
        }
    }

    public void Dodge()
    {
        Collider[] dodgeDetection = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 7.5f, projectileMask);
        if (dodgeDetection.Length != 0 && !dodged)
        {
            if (fov.canSeePlayer && grounded)
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

    public void Blast(Vector3 force)
    {
        float groundScaledY = Mathf.Clamp(force.y, -15f, 50f);
        float airScaledY = Mathf.Clamp(force.y, -15f, 15f);
        force.x *= 3f;
        force.z *= 3f;
        
        if (grounded)
        {
            force.y *= 10f;
            force.y = groundScaledY;
            enemyVelocity += force;
        }
        else
        {
            force.y = airScaledY;
            enemyVelocity += force;
        }
        // Vector3 knock = rb.velocity;
        // knock.y += impact.y;
        if (enemyVelocity.y > 20)
        {
            enemyVelocity.y = 20;
        }
        
        // rb.velocity = knock;


        if (launcher.spell == 2)
        {
            grounded = false;
            nearLedge = false;
        }

        if (!engage)
        {
            engage = true;
            memory = 15f;
        }

        //reset so it doesnt accumulate
        // impact = Vector3.zero;
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

                            if (dist < 3f)
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
            beginAttacking = true;
            fbx.SetTrigger("attacking");
        }
    }

    public virtual void OnAttack()
    {
        attacking = true;
        StartCoroutine(AttackCooldown());
        AttackManager.am.ChooseAttack(gameObject, attackType, attackPrefab, attackPoint, attackingCooldown, damage, forceMultiplier, projSpeedMultiplier);
    }

    public virtual IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackingCooldown);
        attacking = false;
        beginAttacking = false;
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