using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalManager : MonoBehaviour
{
    public static DecalManager Instance { get; private set; }

    [Header("Reference")]
    public ParticleSystem bloodSplatter;

    public ParticleSystem fireSplatter;

    [Header("Params")]
    public float minSize = 0.3f;
    public float maxSize = 0.8f;

    [Header("Collisions")]
    public LayerMask surfaceLayers = -1;
    public float minTimeBetweenDecals = 0.02f;
    public float minTimeFactor = 0.01f;

    private ParticleSystem.EmitParams emitParams;

    private Dictionary<ParticleSystem, float> lastDecalTime = new Dictionary<ParticleSystem, float>();

    private Dictionary<ParticleSystem, float> decalSpawnCooldown = new Dictionary<ParticleSystem, float>();

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

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

        emitParams = new ParticleSystem.EmitParams();
    }

   public void SpawnDecal(Vector3 position, Vector3 normal, ParticleSystem ps)
    {
    if (ps == null) return;

    emitParams.position = position + normal * 0.01f;

    Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
    
    rotation *= Quaternion.AngleAxis(0, normal);

    emitParams.rotation3D = rotation.eulerAngles;
    emitParams.startSize = Random.Range(minSize, maxSize);

    ps.Emit(emitParams, 1);
    }

    public void RegisterParticleSystem(ParticleSystem ps)
    {
        if (!lastDecalTime.ContainsKey(ps))
        {
            lastDecalTime[ps] = 0f;
        }
        if (!decalSpawnCooldown.ContainsKey(ps))
        {
            decalSpawnCooldown[ps] = minTimeBetweenDecals;
        }
    }

    public void HandleParticleCollision(ParticleSystem ps, GameObject other, ParticleSystem decalType = null)
    {
        if(!lastDecalTime.ContainsKey(ps) || !decalSpawnCooldown.ContainsKey(ps)) return;

        if(Time.time - lastDecalTime[ps] < decalSpawnCooldown[ps]) return;

        if(((1 << other.layer) & surfaceLayers) == 0) return;

        int numCollisions = ps.GetCollisionEvents(other, collisionEvents);
        if(numCollisions == 0) return;
    
        ParticleSystem target = decalType != null ? decalType : bloodSplatter;

        for(int i = 0; i < numCollisions; i++)
        {
            decalSpawnCooldown[ps] += minTimeFactor;
            SpawnDecal(collisionEvents[i].intersection, collisionEvents[i].normal, target);
        }

        lastDecalTime[ps] = Time.time;
    }

    public void Unregister(ParticleSystem ps)
    {
        lastDecalTime.Remove(ps);

        if(decalSpawnCooldown.ContainsKey(ps)) decalSpawnCooldown[ps] = minTimeBetweenDecals;
    }
}
