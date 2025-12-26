using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : Rocket
{
    [Header("Grenade Params")]
    public float arc = 5f;
    public override void Start()
    {
        autoTimer -= 1f;
        base.Start();
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y + arc, rb.linearVelocity.z);
    }
    public override void Update()
    {
            autoTimer -= Time.deltaTime;

            if(autoTimer <= 0)
            {
                Explode();
            }
    }
}
