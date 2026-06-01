    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    namespace MovementPhysics
    {
        public static class MovementFunctions
        {
        public static float SlopeLimit = 45f;
        public static float MaxAirSpeed = 100f;
        public static float MaxFallSpeed = -50f;
        public static float airControl = 0.3f;

        public static int MaxClipPlanes = 4;

        public static Vector3 Gravity = new Vector3(0f,-20f,0f);

            public static Vector3 ApplyFriction(float t, Vector3 velocity, float friction, float runDeacceleration)
            {
                Vector3 vel = velocity;
                vel.y = 0;
                float speed = vel.magnitude;
                float drop = 0f;

                float control = speed < runDeacceleration ? runDeacceleration : speed;
                drop = control * friction * Time.fixedDeltaTime * t;

                float newSpeed = speed - drop;

                if (newSpeed < 0)
                {
                    newSpeed = 0;
                }
                if (speed > 0)
                {
                    newSpeed /= speed;
                    velocity.x *= newSpeed;
                    velocity.z *= newSpeed;
                }
                return velocity;
            }

            public static Vector3 Accelerate(Vector3 velocity, Vector3 wishdir, float wishSpeed, float accel)
            {

                float addspeed, accelspeed, currentspeed;

                currentspeed = Vector3.Dot(velocity,wishdir);

                addspeed = wishSpeed - currentspeed;

                if (addspeed <= 0) return velocity;

                accelspeed = accel * wishSpeed * Time.fixedDeltaTime;

                if (accelspeed > addspeed) accelspeed = addspeed;
                
                velocity += accelspeed * wishdir;

                return velocity;
            }

            public static Vector3 AirAccelerate(Vector3 velocity, Vector3 wishdir, float wishspeed, float accel)
            {
                float addspeed, accelspeed, currentspeed;
                float wishspd;

                wishspd = wishspeed;

                if(wishspd > MaxAirSpeed) wishspd = MaxAirSpeed;

                currentspeed = Vector3.Dot(velocity, wishdir);

                addspeed = wishspd - currentspeed;

                if(addspeed <= 0) return velocity;

                accelspeed = accel * wishspeed * Time.fixedDeltaTime;

                if(accelspeed > addspeed) accelspeed = addspeed;

                velocity += accelspeed * wishdir;

                return velocity;
            }

            public static Vector3 AirMovement(Vector3 velocity, Transform cam, float forwardmove, float sidemove, float speed, float accel)
            {                
                float fmove, smove;
                fmove = forwardmove;
                smove = sidemove;

                Vector3 forward = cam.transform.forward;
                Vector3 right = cam.transform.right;

                forward.y = 0;
                right.y = 0;

                forward.Normalize();
                right.Normalize();

                Vector3 wishvel;
                wishvel.x = forward.x * fmove + right.x * smove;
                wishvel.z = forward.z * fmove + right.z * smove;
                wishvel.y = 0;

                float wishspeed = wishvel.magnitude * speed;
                Vector3 wishdir = wishspeed > 0 ? wishvel / wishspeed: Vector3.zero;

                if(wishspeed != 0 && wishspeed > MaxAirSpeed)
                {
                    wishspeed = MaxAirSpeed;
                }

                velocity = AirAccelerate(velocity, wishdir, wishspeed, accel);
                
                return velocity;

            }
            public static Vector3 GroundMovement(Vector3 velocity, Transform main, float fmove, float smove, float speed, float accel, Vector3 groundNormal)
            {

                Vector3 wishDir = new Vector3(smove, 0, fmove);
                wishDir = main.TransformDirection(wishDir);
                if(velocity.y <= 0.1f)
                {
                    wishDir = Vector3.ProjectOnPlane(wishDir, groundNormal).normalized;
                }
    
                float wishSpeed = wishDir.magnitude;
                wishSpeed *= speed;

                velocity = Accelerate(velocity, wishDir, wishSpeed, accel);

                return velocity;
            }

            public static void GetCapsule(Vector3 position, float height, float radius, out Vector3 bottom, out Vector3 top)
            {
                bottom = position + Vector3.up * radius;
                top = position + Vector3.up * (height - radius);
            }

              public static bool GroundedCheck(
                Transform GroundCheck,
                float GroundDistance,
                LayerMask GroundMask,
                ref Vector3 velocity,
                ref float groundTimer,
                float coyoteTime, ref Vector3 groundNormal, out RaycastHit groundhit,
                ref bool onPlatform, ref Vector3 platformVelocity, ref Vector3 lastGroundCheckPos,
                Transform OwnerTransform = null
                )
            {

                groundNormal = Vector3.up;
                bool grounded = false;
                //test to see if touching any collisions given ground mask
                bool sphereGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);
                //now see what the normals are of the surface
                //raycast to find normals of surface
                //in order, 1. check origin point of ray, 2. choose which component of vector to test, 3. out will be filled with data of raycast if hit (how we get normals)
                //4. maximum distance of ray, and 5. what to check ray against
                bool rayGrounded = Physics.Raycast(GroundCheck.position, Vector3.down, out groundhit, GroundDistance, GroundMask);

                float slopeAngle = 0f;

                if (rayGrounded)
                {
                    //if Raycast hits we can get normal, otherwise just assume its flat
                    slopeAngle = Vector3.Angle(groundhit.normal, Vector3.up);
                    if (slopeAngle <= SlopeLimit && !CanSurf(groundhit))
                    {
                        groundNormal = groundhit.normal;
                    }
                }

            bool ground = (sphereGrounded || rayGrounded) && slopeAngle <= SlopeLimit && !CanSurf(groundhit);

            if (ground)
            {
                grounded    = true;
                groundTimer = coyoteTime;

                //messy moving platform parenting 
                if (OwnerTransform != null)
                {
                    MovingPlatform platform = null;

                    // Bouncer bouncer = null;

                    if (rayGrounded
                        && groundhit.collider != null
                        && groundhit.collider.CompareTag("MovingPlatform"))
                    {
                        platform = groundhit.collider.GetComponentInParent<MovingPlatform>();
                    }

                    // if (rayGrounded
                    //     && groundhit.collider != null
                    //     && groundhit.collider.CompareTag("Bouncer"))
                    // {
                    //     bouncer = groundhit.collider.GetComponentInParent<Bouncer>();
                    //     Debug.Log(bouncer);
                    // }

                    if (platform == null)
                    {
                        // wide sphere to help with tuneling
                        Collider[] cols = Physics.OverlapSphere(
                            GroundCheck.position,
                            GroundDistance * 2f,
                            GroundMask);

                        foreach (Collider c in cols)
                        {
                            if (!c.CompareTag("MovingPlatform")) continue;
                            //below feet
                            if (c.bounds.max.y > OwnerTransform.position.y + GroundDistance) continue;
                            platform = c.GetComponentInParent<MovingPlatform>();
                            if (platform != null) break;
                        }
                    }

                    if (platform != null)
                    {
                        if(platform.Ridable){
                        onPlatform = true;

                        // update stored velocity so we can add it on next frame if leaving platform
                        platformVelocity = platform.PlatformVelocity;

                        //positions updated with parent transform
                        if(OwnerTransform.parent != platform.collider.transform)
                        {
                            OwnerTransform.SetParent(platform.collider.transform, true);
                        }
                        }
                    }

                    // if (bouncer != null)
                    // {
                    //     grounded = false;

                    //     bouncer.Bounce(ref velocity);
                    // }
                }
            }
            else
            {
                if (onPlatform){
                    if(OwnerTransform != null && OwnerTransform.parent != null)
                    {
                        OwnerTransform.SetParent(null, true);
                    }
                    //dont get extra down y
                    Vector3 momentum = new Vector3(platformVelocity.x,Mathf.Max(platformVelocity.y, 0f),platformVelocity.z);
                    velocity += momentum;
                    }

                onPlatform = false;
                groundTimer -= Time.fixedDeltaTime;
                if (groundTimer < 0f) groundTimer = 0f;
            }

                return grounded;
            }

            public static bool GroundedCheckPlayer(
                Vector3 position,
                float GroundDistance,
                LayerMask GroundMask,
                ref Vector3 velocity,
                ref float groundTimer,
                float coyoteTime, ref Vector3 groundNormal, out RaycastHit groundhit,
                ref bool onPlatform, ref Vector3 platformVelocity, ref Vector3 lastGroundCheckPos,
                Transform OwnerTransform = null
                )
            {

                groundNormal = Vector3.up;
                bool grounded = false;
                
                GetCapsule(position, 2f, 0.5f, out Vector3 bottom, out Vector3 top);

                bool hit = Physics.CapsuleCast(
                    bottom, top, 0.5f, Vector3.down, out groundhit, 0.65f, GroundMask
                );

                float slopeAngle = 0f;



                if (hit)
                {
                    //if Raycast hits we can get normal, otherwise just assume its flat
                    slopeAngle = Vector3.Angle(groundhit.normal, Vector3.up);
                    if (slopeAngle <= SlopeLimit && !CanSurf(groundhit))
                    {
                        groundNormal = groundhit.normal;
                    }
                }

            bool ground = hit && slopeAngle <= SlopeLimit && !CanSurf(groundhit);

            if (ground)
            {
                grounded    = true;
                groundTimer = coyoteTime;

                //messy moving platform parenting 
                if (OwnerTransform != null)
                {
                    MovingPlatform platform = null;

                    Bouncer bouncer = null;

                    if (hit
                        && groundhit.collider != null
                        && groundhit.collider.CompareTag("MovingPlatform"))
                    {
                        platform = groundhit.collider.GetComponentInParent<MovingPlatform>();
                    }

                    if (hit
                        && groundhit.collider != null
                        && groundhit.collider.CompareTag("Bouncer"))
                    {
                        bouncer = groundhit.collider.GetComponentInParent<Bouncer>();
                        Debug.Log(bouncer);
                    }

                    if (platform == null)
                    {
                        // wide sphere to help with tuneling
                        Collider[] cols = Physics.OverlapSphere(
                            position,
                            GroundDistance * 2f,
                            GroundMask);

                        foreach (Collider c in cols)
                        {
                            if (!c.CompareTag("MovingPlatform")) continue;
                            //below feet
                            if (c.bounds.max.y > OwnerTransform.position.y + GroundDistance) continue;
                            platform = c.GetComponentInParent<MovingPlatform>();
                            if (platform != null) break;
                        }
                    }

                    if (platform != null)
                    {
                        onPlatform = true;

                        // update stored velocity so we can add it on next frame if leaving platform
                        platformVelocity = platform.PlatformVelocity;

                        //positions updated with parent transform
                        if(OwnerTransform.parent != platform.collider.transform)
                        {
                            OwnerTransform.SetParent(platform.collider.transform, true);
                        }
                    }

                    if (bouncer != null)
                    {
                        grounded = false;

                        bouncer.Bounce(ref velocity);
                    }
                }
            }
            else
            {
                if (onPlatform){
                    if(OwnerTransform != null && OwnerTransform.parent != null)
                    {
                        OwnerTransform.SetParent(null, true);
                    }
                    //dont get extra down y
                    Vector3 momentum = new Vector3(platformVelocity.x,Mathf.Max(platformVelocity.y, 0f),platformVelocity.z);
                    velocity += momentum;
                    }

                onPlatform = false;
                groundTimer -= Time.fixedDeltaTime;
                if (groundTimer < 0f) groundTimer = 0f;
            }

                return grounded;

            }

            public static bool ResetCheck(
                Transform GroundCheck,
                float GroundDistance,
                LayerMask ResetMask
            )
            {
                bool rayGrounded = Physics.Raycast(GroundCheck.position, Vector3.down, out RaycastHit hit, GroundDistance, ResetMask);
                return rayGrounded;
            }

            public static class CollisionHandler
            {
                public static bool ResetCollision(MonoBehaviour target, Collision collision, LayerMask resetMask)
                {
                    bool reset;
                    int layer = collision.gameObject.layer;
                    if(((1<<layer) & resetMask.value) != 0)
                    {
                        reset = true;
                    }
                    else reset = false;

                    return reset;
                }
            }

            public static bool CanSurf(RaycastHit hit)
            {
                    float upDot = Vector3.Dot(hit.normal, Vector3.up);
                    return upDot < 0.75f && upDot > 0.1f;
            }

            public static void ClipVelocity(Vector3 velocity, Vector3 normal, ref Vector3 clipped, float overbounce = 1f)
            {
                var angle = normal.y;
                var backoff = Vector3.Dot(velocity, normal) * overbounce;


                for (int i = 0; i < 3; i++)
                {
                    var change = normal[i] * backoff;
                    clipped[i] = velocity[i] - change;
                }

                float adjust = Vector3.Dot(clipped, normal);

                if (adjust < 0.0f)
                {
                    clipped -= normal * adjust;
                }
            }

        public static Vector3 TryPlayerMove(Vector3 pos, Vector3 velocity, float dt, float height, float rad, LayerMask ground, bool grounded, float bounce = 0f, float surfaceFriction = 1f, float stepHeight = 8f)
        {
            GetCapsule(pos, height, rad, out Vector3 bottom, out Vector3 top);

            Vector3 ogVel = velocity;
            Vector3 primalVel = velocity;

            Vector3[] planes = new Vector3[MaxClipPlanes];
            int numPlanes = 0;

            float timeLeft = dt;

            for (int i = 0; i < MaxClipPlanes; i++)
            {
                if(velocity.magnitude < 0.0001f) break;

                Vector3 end = pos + velocity * timeLeft;

                if(!Physics.CapsuleCast(
                    bottom, 
                    top,
                    rad,
                    velocity.normalized,
                    out RaycastHit hit,
                    velocity.magnitude * timeLeft,
                    ground
                ))
                {
                    pos = end;
                    break;
                }

                float traveled = hit.distance / velocity.magnitude;

                timeLeft -= traveled;

                pos += velocity * traveled;

                if(timeLeft <= 0f) break;

                if(numPlanes < MaxClipPlanes)
                {
                    planes[numPlanes++] = hit.normal;
                }

                if (traveled > 0.0001f)
                {
                    ogVel = velocity;
                    numPlanes = 0;
                }

                if(numPlanes ==1)
                {
                        float overbounce = hit.normal.y > 0.7f ? 1.0f : 0.9f;

                        Vector3 clipped = velocity;
                        ClipVelocity(velocity,planes[0], ref clipped, overbounce);
                        velocity = clipped;
                }
                else if (numPlanes > 1){

                for(int j = 0; j < numPlanes; j++)
                {
                    Vector3 clipped = velocity;
                    ClipVelocity(velocity, planes[j], ref clipped, 1.0f);
                    velocity = clipped;

                    for(int n = 0; n < numPlanes; n++)
                    {
                        if(n!=j && Vector3.Dot(velocity, planes[n]) < 0f)
                        {
                            goto crease;
                        }
                    }
                    continue;

                    crease:
                    if (numPlanes >= 2)
                    {
                        Vector3 dir = Vector3.Cross(planes[0], planes[1]).normalized;
                        float speed = Vector3.Dot(velocity,dir);
                        velocity = dir * speed;
                    }
                    else
                    {
                            if (!grounded)
                            {
                                velocity = Vector3.ProjectOnPlane(velocity, planes[0]);
                            }
                            else
                            {
                                velocity *= 0.5f;   
                            }
                        // velocity = Vector3.zero;
                    }
                    break;
                }

                if(numPlanes >= 3)
                {
                 
                    Debug.Log("hitting too many planes, slowing down");
                    velocity *= 0.5f;
                    break;
                }
            }
            } 
 
            return velocity;
        }

        public static void Slamming(ref Vector3 velocity, Collision collision)
        {
            foreach(ContactPoint contact in collision.contacts)
            {
                float into = Vector3.Dot(velocity, contact.normal);

                if(into < 0f)
                {
                    velocity -= contact.normal * into;
                }
            }
        }

            public static void ApplyVelocity(Vector3 velocity, ref Rigidbody rb)
            {
                rb.linearVelocity = velocity;
            }

            public static void ApplyGravity(ref Vector3 velocity)
            {
                if(velocity.y <= MaxFallSpeed) velocity.y = MaxFallSpeed;
                velocity.y += Gravity.y * Time.fixedDeltaTime;
            }

            public static void StartGravity(ref Vector3 velocity)
            {
                if(velocity.y <= MaxFallSpeed) velocity.y = MaxFallSpeed;
                velocity.y += Gravity.y * 0.5f * Time.fixedDeltaTime;
            }

            
            public static void FinishGravity(ref Vector3 velocity)
            {
                if(velocity.y <= MaxFallSpeed) velocity.y = MaxFallSpeed;
                velocity.y += Gravity.y * Time.fixedDeltaTime * 0.5f;
            }

            public static void Jump(ref Vector3 velocity, float jumpHeight)
            {
                if(velocity.y <= 0)
                {
                    velocity.y = 0;
                }
                velocity.y += jumpHeight;
            }

        }
    }