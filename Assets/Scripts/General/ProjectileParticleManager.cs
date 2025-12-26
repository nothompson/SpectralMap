using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileParticleManager : MonoBehaviour
{
    public static ProjectileParticleManager Instance { get; private set; }

    public ParticleSystem fireballExplosionSmoke;
    
    public ParticleSystem fireballSmoke;

    public ParticleSystem fireballExplode;

    public HashSet<Fireball> activeFireballs = new HashSet<Fireball>();

    private ParticleSystem.EmitParams smokeParams;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        smokeParams = new ParticleSystem.EmitParams();
    }

    public void Register(Fireball fireball)
    {
        activeFireballs.Add(fireball);
    }
    public void Delete(Fireball fireball)
    {
        activeFireballs.Remove(fireball);
    }

    void Update()
    {
        if(activeFireballs.Count > 0){
         // Use a temporary list for removals to avoid modifying the set while iterating
            var toRemove = new List<Fireball>();
            foreach (var fb in activeFireballs)
            {
                if (fb == null) // Unity's null check works for destroyed UnityEngine.Object
                {
                    toRemove.Add(fb);
                    continue;
                }

                smokeParams.velocity = -fb.transform.forward * 2f;

                smokeParams.position = fb.transform.position;

                fireballSmoke.Emit(smokeParams, 1);
                
                
            }
            foreach (var fb in toRemove)
                activeFireballs.Remove(fb);
        }
    }
}
