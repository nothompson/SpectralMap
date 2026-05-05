using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabObject : MonoBehaviour, IInteractable
{
    private bool isHeld = false;
    private Rigidbody rb;
    private Collider collider;
    private PlayerInteract holder;

       void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }


    public void OnInteract(GameObject player)
    {
        PlayerInteract pi = player.GetComponent<PlayerInteract>();
        if(pi == null) return;

        if (isHeld)
        {
            Drop(pi);
        }
        else
        {
            if(pi.HeldObject != null)
            {
                pi.HeldObject.GetComponent<GrabObject>()?.Drop(pi);
            }

            Pickup(pi);
        }

        
    }

    public void Pickup(PlayerInteract pi)
{
    isHeld = true;
    holder = pi;
    pi.HeldObject = gameObject;
    pi.HeldObject.layer = LayerMask.NameToLayer("Held");
    pi.HoldingObject = true;
    PlayerManager.Instance.HoldingObject = true;

    Collider playerCollider = pi.GetComponent<Collider>();
    Physics.IgnoreCollision(collider, playerCollider, true);

    if (ReloadManager.Instance.reloading)
    {
        ReloadManager.Instance.StopReload();
        ReloadManager.Instance.reloading = false;
    }

    if(rb != null)
    {
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearDamping = 15f;
        rb.angularDamping = 15f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    transform.SetParent(null);
}

public void Drop(PlayerInteract pi, bool thrown = false)
{
    isHeld = false;
    holder = null;
    
    if(rb != null)
    {
        if (thrown)
            {
                Vector3 throwVel = Camera.main.transform.forward * 10f;
                rb.AddForce(throwVel, ForceMode.Impulse);
            }
        rb.useGravity = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    pi.HoldingObject = false;
    pi.HeldObject.layer = LayerMask.NameToLayer("Object");
    PlayerManager.Instance.HoldingObject = false;

    Collider playerCollider = pi.GetComponent<Collider>();
    Physics.IgnoreCollision(collider, playerCollider, false);

    pi.HeldObject = null;
}

void FixedUpdate()
{
    if(!isHeld || holder == null) return;

    Vector3 target = holder.ObjectAnchor.position;
    Vector3 delta = target - transform.position;

    if(delta.magnitude > 7f) Drop(holder);

    rb.linearVelocity = delta * 20f;
}

float GetColliderRadius()
{
    if (collider is SphereCollider sc) return sc.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
    if (collider is CapsuleCollider cc) return cc.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
    if (collider is BoxCollider bc) return Mathf.Min(bc.size.x, bc.size.y, bc.size.z) * 0.5f * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
    return 0.3f;
}

    public void ExitInteract()
    {
        
    }

    public bool CanInteract()
    {
        return true;
    }

    public InteractionType GetInteractionType() => InteractionType.Grab;
}