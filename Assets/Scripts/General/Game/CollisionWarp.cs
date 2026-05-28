using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionWarp : MonoBehaviour
{
    public Transform warpPoint;

    public float SpeedMultiplier = 1f;

    public void OnTriggerEnter(Collider other)
    {

        if(other.gameObject.layer != 3) return;

        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

        rb.isKinematic = true;

        rb.position = warpPoint.position;

        rb.isKinematic = false;

        PlayerControlRigid pc = other.gameObject.GetComponent<PlayerControlRigid>();

        float speed = pc.playerSpeed;

        pc.playerVelocity = warpPoint.forward * speed * SpeedMultiplier;
    }
}