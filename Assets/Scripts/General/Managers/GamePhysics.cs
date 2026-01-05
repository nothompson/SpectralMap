using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GamePhysics
{
    public static class GameFunctions
    {
        public static Vector3 ExplosionForce(Collider hit, Vector3 position, float explosionRadius, float explosionForce)
        {
            //get angle of explosion from target
            Vector3 dir = (hit.transform.position - position).normalized;

            //distance from explosion radius origin to target origin
            float dist = Vector3.Distance(hit.transform.position, position);

            //inversely proportional magnitude (so target gets blasted away from explosions instead of the direction they were shot)
            float inverse = 1.0f - Mathf.Clamp01(dist / explosionRadius);

            //calculate force 
            Vector3 force = dir * explosionForce * inverse;

            // force.y += 1.0f * inverse;

            return force;
        }

        public static float CalculateForceDamage(Vector3 force, float maximumDamage, float damageMultiplier)
        {
            float dmg;

            float mag = force.magnitude;
            float norm = Mathf.Clamp(mag, 0f, maximumDamage) / maximumDamage;
            float pow = Mathf.Pow(norm, 2f);
            dmg = pow * maximumDamage;

            float final = dmg * damageMultiplier;

            return final;
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
