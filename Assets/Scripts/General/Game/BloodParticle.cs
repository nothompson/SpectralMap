using System.Collections.Generic;
using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        
        DecalManager.Instance.RegisterParticleSystem(ps);
    }

    void OnParticleCollision(GameObject other)
    {
        DecalManager.Instance.HandleParticleCollision(ps, other);
    }
}
