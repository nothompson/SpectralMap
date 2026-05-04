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

    public void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.layer == 3) Explode();
    }

    public override void Explode()
    {
        ProjectileParticleManager.Instance.SpawnSkulls(transform);
        ProjectileParticleManager.Instance.SpawnPollutantBlast(transform);
        base.Explode();
    }
}
