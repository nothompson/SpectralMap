using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public LayerMask bounceMask;
    public PlayerControlRigid pc;
    public Transform playerFeet;
    public BoxCollider boxCollider;

    public float bounceHeight = 10f;
    public float maxHeight = 30f;
    public float cooldown = 5f;
    private float cd;

    private bool bounced = false;

    public FMODUnity.StudioEventEmitter bounce;

    void Start(){
        cd = cooldown;
    }

    // Update is called once per frame
    void Update()
    {
        BounceCheck();
        BounceCooldown();
    }

    public void BounceCheck()
    {
        float radius = 0.75f;
        Collider[] hits = Physics.OverlapSphere(playerFeet.position, radius, bounceMask);
        foreach (Collider hit in hits)
        {
            if (hit == boxCollider){
                Bounce();
            }
        }
    }

    public void Bounce()
    {
        if (!bounced)
        {
            bounce.Play();
            bounced = true;

            float ymag = Mathf.Abs(pc.playerVelocity.y);
            if (ymag > maxHeight)
            {
                ymag = maxHeight;
            }
            
            pc.playerVelocity = new Vector3(pc.playerVelocity.x, ymag + bounceHeight, pc.playerVelocity.z);
        }
    }

    public void BounceCooldown()
    {
        if(bounced){
            if (cd > 0){
                cd -= Time.deltaTime * 20f;
            }
            else{
                bounced = false;
                cd = cooldown;
            }
        }
    }
}
