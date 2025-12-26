using System.Collections.Generic;
using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        
        if (GibsManager.Instance != null)
        {
            GibsManager.Instance.RegisterParticleSystem(ps);
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (GibsManager.Instance != null)
        {
            GibsManager.Instance.HandleParticleCollision(ps, other);
        }
    }
}
