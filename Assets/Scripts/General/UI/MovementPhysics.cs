using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementPhysics
{
    public static class MovementFunctions
    {
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

        public static Vector3 Accelerate(Vector3 velocity, Vector3 wishDir, float wishSpeed, float accel)
        {

            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addspeed = wishSpeed - currentSpeed;
            if (addspeed <= 0) return velocity;

            float accelspeed = accel * wishSpeed * Time.fixedDeltaTime;

            if (accelspeed > addspeed)
            {
                accelspeed = addspeed;
            }
            velocity += accelspeed * wishDir;

            return velocity;
        }

        public static Vector3 AirMovement(Vector3 velocity, Transform cam, float fmove, float smove, float speed, float accel)
        {
            Vector3 forward = cam.transform.forward;
            Vector3 right = cam.transform.right;

            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 wishvel = forward * fmove + right * smove;
            wishvel.y = 0;

            Vector3 wishDir = wishvel.normalized;
            float wishSpeed = wishvel.magnitude * speed;

            velocity = Accelerate(velocity, wishDir, wishSpeed, accel);

            ApplyGravity(ref velocity);
            
            return velocity;

        }
        public static Vector3 GroundMovement(Vector3 velocity, Transform main, float fmove, float smove, float speed, float accel)
        {
            Vector3 wishDir = new Vector3(smove, 0, fmove);
            wishDir = main.TransformDirection(wishDir);
            wishDir.Normalize();

            float wishSpeed = wishDir.magnitude;
            wishSpeed *= speed;

            velocity = Accelerate(velocity, wishDir, wishSpeed, accel);

            return velocity;
        }

        public static bool GroundedCheck(
            Transform GroundCheck,
            float GroundDistance,
            LayerMask GroundMask,
            ref Vector3 velocity,
            ref float groundTimer,
            float slopeLimit,
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

            bool ground = (sphereGrounded || rayGrounded) && slopeAngle <= slopeLimit;

            if (ground)
            {
                grounded = true;

                //"coyote time" allows a buffer period after leaving collider to still jump
                groundTimer = coyoteTime;

                if (velocity.y < -2f)
                {
                    //if grounded and negative velocity still accumulating, quickly clamp it
                    velocity.y = Mathf.Lerp(velocity.y, -2f, Time.fixedDeltaTime * 10f);
                }
            }
            else if (rayGrounded && slopeAngle > slopeLimit)
            {
                    groundTimer = 0f;
                    //wip surfing implementation 
                    Vector3 clipped = velocity;
                    ClipVelocity(velocity, hit.normal, ref clipped, 1.0001f);
                    velocity = clipped;

                    float slopeGravityMultiplier = 1f;
                    Vector3 slopeGravity = Vector3.ProjectOnPlane(Physics.gravity, hit.normal);
                    velocity += slopeGravity * slopeGravityMultiplier * Time.fixedDeltaTime;

                    // if (velocity.y < -2f)
                    // {
                    //     velocity.y = -2f;
                    // }
            }
            else
            {
                groundTimer -= Time.fixedDeltaTime;
                if (groundTimer < 0f)
                {
                    groundTimer = 0f;
                }
            }
            
            bool groundedBuffer = grounded || groundTimer > 0f;

            grounded = groundedBuffer;

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

        public static void ApplyVelocity(Vector3 velocity, ref Rigidbody rb)
        {
            rb.linearVelocity = velocity;
        }

        public static void ApplyGravity(ref Vector3 velocity)
        {
            float gravity = -20f;
            velocity.y += gravity * Time.fixedDeltaTime;
        }

    }
}