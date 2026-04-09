using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public BoxCollider boxCollider;

    public float bounceHeight = 10f;
    public float maxHeight = 30f;
    public float cooldown = 5f;
    private float cd;

    public bool bounced = false;

    public FMODUnity.StudioEventEmitter bounce;

    public SquashAndStretch Animation;

    public Vector3 cachedVelocity;

    public void PlayAnimation()
    {
        if(bounced) return;
        Animation.Play();
        bounced = true;
    }

    public void Bounce(ref Vector3 velocity)
    {
        if(bounced) return;
            Animation.Play();
            bounced = true;
            bounce.Play();

            float ymag = Mathf.Abs(velocity.y);
            if (ymag > maxHeight)
            {
                ymag = maxHeight;
            }
            
            velocity.y = ymag + bounceHeight;
    }

    public void Ready()
    {
        bounced = false;
    }

}
