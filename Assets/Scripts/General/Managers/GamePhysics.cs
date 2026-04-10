using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GamePhysics
{
    public static class GameFunctions
    {
        public static Vector3 TargetedExplosionForce(Collider hit, Vector3 position, float explosionRadius, float explosionForce)
        {
            //get angle of explosion from target
            // Vector3 dir = (hit.transform.position - position).normalized;
            Vector3 closestPoint = hit.ClosestPoint(position);
            Vector3 dir = (closestPoint - position).normalized;
            float dist = Vector3.Distance(closestPoint, position);

            //distance from explosion radius origin to target origin
            // float dist = Vector3.Distance(hit.transform.position, position);

            //inversely proportional magnitude (so target gets blasted away from explosions instead of the direction they were shot)
            float inverse = Mathf.Max(0.2f, 1.0f - Mathf.Clamp01(dist / explosionRadius));

            //calculate force 
            Vector3 force = dir * explosionForce * inverse;

            return force;
        }

        public static Vector3 SelfExplosionForce(Collider hit, Vector3 position, float explosionRadius, float explosionForce)
        {
            //get angle of explosion from target
            Vector3 dir = (hit.transform.position - position).normalized;

            // if(dir.magnitude < 0.01f)
            // {
            //     dir = Vector3.up;
            // }
            // else
            // {
            //     dir.Normalize();
            // }

            //distance from explosion radius origin to target origin
            float dist = Vector3.Distance(hit.transform.position, position);

            //inversely proportional magnitude (so target gets blasted away from explosions instead of the direction they were shot)
            float inverse = 1.0f - Mathf.Clamp01(dist / explosionRadius);

            //calculate force 
            Vector3 force = dir * explosionForce * inverse;

            return force;
        }

        public static float CalculateForceDamage(Collider hit, Vector3 position, float explosionRadius, float maximumDamage, float damageMultiplier, bool direct = false)
        {
            if(direct) return maximumDamage * damageMultiplier;

            Vector3 closestPoint = hit.ClosestPoint(position);
            float dist = Vector3.Distance(closestPoint, position);
            float falloff = 1f - Mathf.Clamp01(dist / explosionRadius);
            float damage = maximumDamage * damageMultiplier * falloff;

            // float dmg;

            // float mag = force.magnitude;
            // float norm = Mathf.Clamp(mag, 0f, maximumDamage) / maximumDamage;
            // float pow = Mathf.Pow(norm, 2f);
            // dmg = pow * maximumDamage;

            // float final = dmg * damageMultiplier;

            return damage;
        }
        
        public static void ApplyForceToRigidbody(ref Rigidbody rb, Enemy e, Vector3 force)
        {
            if (rb != null && (e == null || rb != e.rb))
            {
                // Debug.Log(force);
                Vector3 forced = rb.linearVelocity + force;
                rb.linearVelocity = forced;
                // rb.AddForce(force, ForceMode.Impulse);
            }
        }

        public static bool FilterLayers(int layer, LayerMask[] ignoreLayers)
        {
            foreach(var l in ignoreLayers)
            {
                if((l.value & (1 << layer)) != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
