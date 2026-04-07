using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementPhysics;

#region Init
public class Enemy : MonoBehaviour
{
    [SerializeField] NamedEntity NamedEntity;
    [SerializeField] public int Points = 500;
    [SerializeField] public string Name;
    [Range(0f,1f)]
    [SerializeField] public float ChanceToDropPickup;

    [Header("References")]
    [HideInInspector] public Rigidbody rb;

    [HideInInspector] public Transform player;

    [HideInInspector] public Launcher launcher;

    public GameObject attackPrefab;

    public LayerMask projectileMask;

    public LayerMask groundMask;
    public LayerMask resetMask;
    public Transform GroundCheck;
    public LayerMask pickupMask;

    public Transform attackPoint;
    private bool onPlatform = false;

    public FOV fov;

    public HP hp;

    [HideInInspector] public Animator fbx; 

    [Header("General")]
    [SerializeField] public string EnemyID;

    [SerializeField] public PersonalityType Personality;

    [HideInInspector] public AttackBehavior[] Behaviors;


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

    [SerializeField] public float movementChaos = 2.5f;
    [SerializeField] public float chaosFrequency = 0.6f;
    [Range(0f,1f)]
    [SerializeField] public float chaosBlend = 0.6f;

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
    [HideInInspector] public float newRad;
    [HideInInspector] public float oldRad;
    float newDist;
    float oldDist;
    Vector3 pickupPosition;
    Vector3 impact;
    float cooldown;
    float initSpeed;
    float airSpeed;

    Vector3 groundNormal;

    public Vector3 platformVelocity;

    private Vector3 lastGroundCheckPos;

    bool reset; 

    [HideInInspector] public float noiseOffset;

    [HideInInspector] public AttackBehavior pendingAttack;

    [HideInInspector] public float MaxRange;
    [HideInInspector] public float MinRange;

    [HideInInspector] public float preferredRange;

    AttackBehavior MaxRangeAttack;
    AttackBehavior MinRangeAttack;

    [HideInInspector] public bool stationaryAttack = false;

    float strafeDir = 0f;
    float strafeTimer = 0f;
    float strafeDuration = 0f;


    public enum PersonalityType
    {
        Reckless,
        Tactical,
        Cowardly
    }

    public void Awake()
    {
        if(NamedEntity != null)
        {
            EnemyID = NamedEntity.Name;
        }
    }

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
        if (!string.IsNullOrEmpty(EnemyID) && DeathManager.Instance.CheckIfDead(EnemyID))
        {
            Destroy(gameObject);
        }

        cooldown = dodgeCooldown;

        newRad = fov.radius * 2f;
        oldRad = fov.radius;

        newDist = attackDistance * 2;
        oldDist = attackDistance;

        initSpeed = moveSpeed;
        airSpeed = moveSpeed * airSpeedFactor;

        noiseOffset = Random.Range(0f,100f);

        GetBehavior();

        preferredRange = GetPreferredRange();

        lastGroundCheckPos = GroundCheck.position;

    }

    void GetBehavior()
    {
        Behaviors = GetComponents<AttackBehavior>();

        MaxRangeAttack = Behaviors[0];
        MinRangeAttack = Behaviors[0];
        foreach(AttackBehavior b in Behaviors)
        {
            b.InitBehavior(gameObject, attackPoint);

            if(b.Range < MinRangeAttack.Range) MinRangeAttack = b;
            if(b.Range > MaxRangeAttack.Range) MaxRangeAttack = b;
        }

        MaxRange = MaxRangeAttack.Range;

        MinRange = MinRangeAttack.Range;
    }

    float GetPreferredRange()
    {
        switch (Personality)
        {
            case PersonalityType.Reckless:
                return MinRange;

            case PersonalityType.Tactical:
                return GetAverageRange();
            
            case PersonalityType.Cowardly:
            default:
                return MaxRange;
        }
    }

    float GetAverageRange()
    {
        float sum = 0f;
        foreach(AttackBehavior b in Behaviors) sum += b.Range;
        float avg = sum / Behaviors.Length;
        return avg;
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

    public virtual void References()
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
    
    public virtual void Routines()
    {
        StartCoroutine(DodgeRoutine());
        StartCoroutine(Leap());
        StartCoroutine(CriticalCheck());
    }

    #endregion

    // Update is called once per frame
    public virtual void Update()
    {
        if(DeathManager.PlayerDead)
        {
            engage = false;
        }
        DodgeCooldown();
    }

    public virtual void FixedUpdate()
    {
        CalculateVelocity();
        MovementFunctions.ApplyVelocity(enemyVelocity, ref rb);
    }

    public virtual void CalculateVelocity()
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
        if(DeathManager.PlayerDead) return; 
        if (fov.canSeePlayer && !critical)
        {
            memory = 10f;
            engage = true;
            TargetSpotted(player.position);
        }
        else if (engage && memory > 0)
        {
            fov.radius = newRad;
            // if (attackType == 2)
            // {
            //     attackDistance = newDist;
            // }

            memory -= Time.fixedDeltaTime;
            TargetSpotted(player.position);
            if (memory <= 0)
            {
                fov.radius = oldRad;
                engage = false;
                memory = 0;
                strafeDir = 0f;
                strafeTimer = 0f;
                // if (attackType == 2)
                // {
                //     attackDistance = oldDist;
                // }
            }
        }
        if (!engage)
        {
            // if(fbx == null) return;
            // fbx.SetTrigger("stopped");
        }
    }

    public virtual void TargetSpotted(Vector3 targetPosition)
    {
        if(DeathManager.PlayerDead) return;
        distance = Vector3.Distance(transform.position, targetPosition);
        float angle = Vector3.Angle(transform.position, targetPosition);
        Vector3 adjusted = new Vector3(targetPosition.x, targetPosition.y + 0.33f, targetPosition.z);
        Vector3 direction = (adjusted - transform.position).normalized;

        if (direction != Vector3.zero && !critical)
        {
            LookTowards(direction);
        }

        // if ((distance > preferredRange) || (jumpAcross && !critical))
        if (distance > preferredRange && !stationaryAttack)
        {
            MoveTowards(direction);
        }
        else if(distance <= preferredRange && !stationaryAttack)
        {
            switch (Personality)
                {
                    case PersonalityType.Cowardly:
                        UpdateStrafe();
                        if(distance < preferredRange * 0.8f)
                        {
                            MoveTowards(-direction);
                        }
                        break;
                    case PersonalityType.Tactical:
                        UpdateStrafe();
                        if(distance < preferredRange * 0.6f && distance >= MinRange)
                        {
                            MoveTowards(direction);
                        }
                        break;
                    case PersonalityType.Reckless:
                    default:
                        MoveTowards(direction);
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

        float fleeDist = GetAverageRange() * 0.5f;
        switch (Personality)
        {
            case PersonalityType.Cowardly:
                if(distance <= fleeDist)
                {
                    Flee(fleeDist * 0.1f, fleeDist);
                }
                break;
            case PersonalityType.Tactical:
                if(distance <= fleeDist && hp.Critical())
                {
                    Flee(fleeDist * 0.25f, fleeDist);
                }
                break;
            case PersonalityType.Reckless:
            default:
                break;
        }

        // if (distance <= attackDistance * 0.25)
        // {
        //     Flee(attackDistance * 0.5f, attackDistance);
        // }

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

    public void MoveTowards(Vector3 direction)
    {
        float noise = Mathf.PerlinNoise(Time.time * chaosFrequency + noiseOffset, 0f);
        float bipolar = (noise * 2f - 1f) * movementChaos;

        Vector3 flattened = new Vector3(direction.x, 0f, direction.z).normalized;
        Vector3 horiz = Vector3.Cross(Vector3.up, flattened);

        Vector3 chaosDir = (flattened + horiz * bipolar).normalized;

        Vector3 wishDir = Vector3.Lerp(flattened, chaosDir, chaosBlend).normalized;

        float wishSpeed = moveSpeed;
        enemyVelocity = MovementFunctions.Accelerate(enemyVelocity, wishDir, wishSpeed, 10f);
    }

    #endregion

    #region Movement
    public void Movement()
    {
 

        GroundedCheck();
        // LedgeCheck();
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
            if(enemyVelocity.y < 0f)
            {
                enemyVelocity.y = 0f;
            }
        }
       float enemymovement = rb.linearVelocity.magnitude;

        if (enemymovement < 0.25f)
        {
            if(fbx == null) return;
            // float log = enemymovement > 0.1f ? enemymovement : 0f;
            // Debug.Log("stopping: " + log);
            fbx.SetTrigger("stopped");
        }
        else
        {
            if(fbx == null) return;
            // Debug.Log("moving: " + enemymovement);
            fbx.SetTrigger("moving");
        }

     

        // Debug.Log("x: " + transform.localRotation.x + " " + "y: " + transform.localRotation.y + " " + "z: " + transform.localRotation.z);

    }
    void OnCollisionEnter(Collision collision)
    {
        reset = MovementFunctions.CollisionHandler.ResetCollision(this, collision, resetMask);
    }
    public void GroundedCheck()
    {
        grounded = MovementFunctions.GroundedCheck(
            GroundCheck,
            GroundDistance,
            groundMask,
            ref enemyVelocity,
            ref groundTimer,
            coyoteTime, ref groundNormal, out RaycastHit groundhit, ref onPlatform, ref platformVelocity, ref lastGroundCheckPos, transform
        );

        if (reset)
        {
            hp.Damage(hp.currentHP);
            reset = false;
        }
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
                nearLedge = true;
                if(fbx != null) fbx.SetTrigger("stopped");
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
                jumpAcross = true;
            }
        }
    }

        void UpdateStrafe()
    {
        strafeTimer -= Time.fixedDeltaTime;
        if(strafeTimer <= 0f)
        {
            float roll = Random.value;
            if(roll <= 0.3f) strafeDir = 0f;
            else if (roll < 0.65f) strafeDir = 1f;
            else strafeDir = -1f;

            strafeDuration = Random.Range(0.4f,1.2f);
            strafeTimer = strafeDuration;
        }
        if(strafeDir != 0f)
        {
            Vector3 right = Vector3.Cross(Vector3.up, new Vector3(transform.forward.x, 0f, transform.forward.z).normalized);
            Vector3 wishDir = right * strafeDir;
            enemyVelocity = MovementFunctions.Accelerate(enemyVelocity, wishDir, moveSpeed * 0.70f, 8f);
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
        if(dodged || !fov.canSeePlayer || !grounded) return;

        Collider[] dodgeDetection = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 7.5f, projectileMask);
        if (dodgeDetection.Length == 0) return;

        Collider closest = null;
        float closestDist = float.MaxValue;
        foreach(Collider c in dodgeDetection)
        {
            float d = Vector3.Distance(transform.position, c.transform.position);
            if(d < closestDist)
            {
                closestDist = d;
                closest = c;
            }
        }
        if(closest == null) return;

        Vector3 toProjectile = (closest.transform.position - transform.position);
        toProjectile.y = 0f;
        float offset = Vector3.Dot(toProjectile, transform.right);

        Vector3 toProjectileDir = toProjectile.normalized;
        float forward = Vector3.Dot(toProjectileDir, transform.forward);

        if(forward < 0.25f) return;

        float centerThreshold = 0.25f;

        if(Mathf.Abs(offset) <= centerThreshold * toProjectile.magnitude)
        {
            if(Random.value < 0.4f)
            {
                enemyVelocity.y = jumpHeight;
            }
            else
            {
                Vector3 dodgeDir = Random.value > 0.5f ? transform.right : -transform.right;
                enemyVelocity += dodgeDir * dodgeSpeed;
            }
        }
        else
        {
            Vector3 dodgeDir = offset > 0f ? -transform.right : transform.right;
            enemyVelocity += dodgeDir * dodgeSpeed;
        }
        
        dodged = true;
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
        Vector3 fleeDir = (player.position - transform.position).normalized;
        distanceFromPlayer = Vector3.Distance(transform.position, player.position);
        fleeDir.y = Mathf.Clamp(fleeDir.y, -0.2f, 0.25f);
        if (!nearLedge) {
            // if (distanceFromPlayer <= min)
            // {
            //     LookTowards(fleeDir * 1f);
            // }
            MoveTowards(fleeDir * -1f);
            // else if (distanceFromPlayer > min && distanceFromPlayer < max)
            // {
            //     LookTowards(fleeDir * -1f);
            //     MoveTowards(fleeDir * -1f);
            // }
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
        if(!fov.canSeePlayer || attacking || pendingAttack != null)  return;

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

        if (bestChoice.Stationary)
        {
            stationaryAttack = true;
        }

        if(fbx == null) return;
        fbx.SetTrigger(pendingAttack.AnimationEvent);

        StartCoroutine(AttackTimeout(bestChoice));

    }

    public IEnumerator AttackTimeout(AttackBehavior expected)
    {
        yield return new WaitForSeconds(2f);
        if(pendingAttack == expected)
        {
            pendingAttack = null;
            beginAttacking = false;
            stationaryAttack = false;
        }
    }

    public virtual void OnAttack()
    {
        if(pendingAttack == null) return;

        attacking = true;
        float cooldown = pendingAttack.Fire();
        pendingAttack = null;
        StartCoroutine(AttackCooldown(cooldown));
    }

    public virtual void EndAttack()
    {
        stationaryAttack = false;
    }

    public virtual IEnumerator AttackCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        attacking = false;
        beginAttacking = false;
    }

    #endregion
}