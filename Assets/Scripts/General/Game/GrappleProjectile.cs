using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
public class GrappleProjectile : MonoBehaviour
{
    public Grapple grapple;

    public Rigidbody rb;
    private float speed;
    private LayerMask targetMask;
    private float segmentSpacing;
    private float collisionRadius;
    private float maxDistance;
    private Vector3 velocity;
    private float steerStrength;
    private float distanceTravelled = 0f;
    private float distanceSinceLastSegment = 0f;

    private Transform player;
    private Camera cam;


    public void Init(Grapple owner, Vector3 direction, Transform pl)
    {
        grapple = owner;
        player = pl;
        cam = grapple.playerCam;

        targetMask = grapple.targetMask;
        speed = grapple.projectileSpeed;
        maxDistance = grapple.grappleDistance;
        segmentSpacing = grapple.segmentSpacing;
        collisionRadius = grapple.collisionRadius;
        steerStrength = grapple.steerStrength;

        velocity = direction.normalized * speed;

        // rb = GetComponent<Rigidbody>();

        // rb.linearVelocity = velocity;

    }

    void FixedUpdate()
    {
        if(cam != null) {
        Vector3 wishVel = cam.transform.forward * speed;
        velocity = Vector3.Lerp(velocity, wishVel, steerStrength * Time.fixedDeltaTime);
        velocity = velocity.normalized * speed;
        }
        float step = speed * Time.fixedDeltaTime;
        
        transform.position += velocity * Time.fixedDeltaTime;

        Vector3 from = player != null ? player.position : transform.position;

        distanceTravelled = Vector3.Distance(from, transform.position);
    

        if(distanceTravelled >= maxDistance)
        {
            if(grapple != null){
            grapple.OnProjectileMiss();
            Destroy(gameObject);
            }
        }


        if(Physics.SphereCast(transform.position, collisionRadius, velocity.normalized,
        out RaycastHit hit, step, targetMask, QueryTriggerInteraction.Ignore))
        {
            grapple.OnProjectileHit(hit.collider.transform, hit.point, hit.distance + distanceTravelled);

            Destroy(gameObject);
        }

    }

}
