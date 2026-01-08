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
            public static Vector3 GroundMovement(Vector3 velocity, Transform main, float fmove, float smove, float speed, float accel)
            {
                Vector3 wishDir = new Vector3(smove, 0, fmove);
                wishDir = main.TransformDirection(wishDir);
                wishDir.Normalize();

                float wishSpeed = wishDir.magnitude;
                wishSpeed *= speed;

                if(velocity.y < 0)
                {
                    velocity.y = 0;
                }

                velocity = Accelerate(velocity, wishDir, wishSpeed, accel);

                return velocity;
            }

            public static bool GroundedCheck(
                Transform GroundCheck,
                float GroundDistance,
                LayerMask GroundMask,
                ref Vector3 velocity,
                ref float groundTimer,
                float coyoteTime
                )
            {
                bool grounded = false;
                //test to see if touching any collisions given ground mask
                bool sphereGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);
                //now see what the normals are of the surface
                //raycast to find normals of surface
                //in order, 1. check origin point of ray, 2. choose which component of vector to test, 3. out will be filled with data of raycast if hit (how we get normals)
                //4. maximum distance of ray, and 5. what to check ray against
                bool rayGrounded = Physics.Raycast(GroundCheck.position, Vector3.down, out RaycastHit hit, GroundDistance, GroundMask);

                float slopeAngle = 0f;

                if (rayGrounded)
                {
                    //if Raycast hits we can get normal, otherwise just assume its flat
                    slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                }

                bool ground = (sphereGrounded || rayGrounded) && slopeAngle <= SlopeLimit;

                if (ground)
                {
                    grounded = true;

                    //"coyote time" allows a buffer period after leaving collider to still jump
                    groundTimer = coyoteTime;
                }
                else
                {
                    groundTimer -= Time.fixedDeltaTime;
                    if (groundTimer < 0f)
                    {
                        groundTimer = 0f;
                    }
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

            public static bool CanSurf(RaycastHit hit)
            {
                    float upDot = Vector3.Dot(hit.normal, Vector3.up);
                    return upDot < 0.7f && upDot > 0.05f;
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

        public static Vector3 TryPlayerMove(Vector3 pos, Vector3 velocity, float dt, float rad, LayerMask ground, bool grounded, ref int stepFramesLeft, float bounce = 0f, float surfaceFriction = 1f, float stepHeight = 8f)
        {
            Vector3 ogVel = velocity;
            Vector3 primalVel = velocity;

            Vector3[] planes = new Vector3[MaxClipPlanes];
            int numPlanes = 0;

            float timeLeft = dt;

            if (grounded)
            {
                velocity += Vector3.up * 0.01f;
                Debug.Log("pushing up slightly");
            }

            for (int i = 0; i < MaxClipPlanes; i++)
            {
                if(velocity.magnitude < 0.0001f) break;

                Vector3 end = pos + velocity * timeLeft;

                if(!Physics.CapsuleCast(
                    pos + Vector3.up * 0.6f,
                    pos + Vector3.up * 1.4f,
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

                if(hit.distance < 0.05f && stepFramesLeft >= 0) 
                {
                    Vector3 stepUp = Vector3.up * 2f;
                    Vector3 newPos = pos + stepUp;
                    if (!Physics.CapsuleCast(newPos + Vector3.up * 0.5f, newPos + Vector3.up * 1.5f, rad, velocity.normalized, velocity.magnitude * timeLeft, ground))
                    {
                        stepFramesLeft -= 1;
                        Debug.Log("trying to step up ledge");
                        velocity.y += stepHeight;
                    }
                }

                pos += velocity * hit.distance * 0.99f;

                float traveled = hit.distance / velocity.magnitude;
                timeLeft -= traveled;

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
                        Debug.Log("contacted with plane, clipping velocity");

                        float overbounce = hit.normal.y > 0.7f ? 1.0f : 0.9f;

                        Vector3 clipped = velocity;
                        ClipVelocity(velocity,planes[0], ref clipped, overbounce);
                        velocity = clipped;
                }
                else{

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
                        Debug.Log("hitting two planes");
                        Vector3 dir = Vector3.Cross(planes[0], planes[1]).normalized;
                        float speed = Vector3.Dot(velocity,dir);
                        velocity = dir * speed;
                    }
                    else
                    {
                        velocity *= 0.5f;
                        // velocity = Vector3.zero;
                    }
                    break;
                }
            } 
            if(numPlanes >= 3)
                {
                    Debug.Log("hitting too many planes, slowing down");
                    velocity *= 0.5f;
                    break;
                }
            }
            return velocity;
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

        }
    }