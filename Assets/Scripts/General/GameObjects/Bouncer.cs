using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public BoxCollider boxCollider;

    public float bounceHeight = 10f;
    public float maxHeight = 30f;
    private float cd;

    public bool bounced = false;

    public FMODUnity.StudioEventEmitter bounce;

    public SquashAndStretch Animation;

    public Vector3 cachedVelocity;

    public static event System.Action<Bouncer> OnBounce;

    public void PlayAnimation()
    {
        if(bounced) return;
        Animation.Play();
        bounced = true;
    }

    public void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer != 3)
        {
            if(other.gameObject.layer != 11)
            {
                return;
            }
        }

        if(bounced) return;
            bounced = true;
            Animation.Play();
            bounce.Play();

        if(other.gameObject.layer == 3)
        {
            PlayerControlRigid pc = other.gameObject.GetComponent<PlayerControlRigid>();
            if(pc != null)
            {
                pc.grounded = false;
                pc.playerVelocity.y = 0f;
                pc.playerVelocity.y += bounceHeight; 
            }
        }

        if(other.gameObject.layer == 11)
        {
            Enemy e = other.gameObject.GetComponent<Enemy>();
            if(e != null)
            {
                e.grounded = false;
                e.enemyVelocity.y = 0f;
                e.enemyVelocity.y += bounceHeight; 
            }
        }
    }



    public void Bounce(ref Vector3 velocity)
    {
        if(bounced) return;
            bounced = true;
            Animation.Play();
            bounce.Play();
            OnBounce?.Invoke(this);

            // float ymag = Mathf.Abs(velocity.y);
            // if (ymag > maxHeight)
            // {
            //     ymag = maxHeight;
            // }
            
            // velocity.y = ymag + bounceHeight;
    }

    public void Ready()
    {
        bounced = false;
    }

}
