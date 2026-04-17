using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileParticleManager : MonoBehaviour
{
    public static ProjectileParticleManager Instance { get; private set; }

    private struct ParticleEntry
    {
        public Transform target;
        public float remainingLifetime;
    }

    private List<ParticleEntry> trackedParticles = new List<ParticleEntry>();
    private ParticleSystem.Particle[] particleBuffer;

    public ParticleSystem fireballExplosionSmoke;
    
    public ParticleSystem fireballSmoke;

    public ParticleSystem fireballExplode;

    public ParticleSystem ReloadPulse;
    public ParticleSystem FireballBlast;

    public HashSet<Fireball> activeFireballs = new HashSet<Fireball>();

    private ParticleSystem.EmitParams smokeParams;

    public ParticleSystem SorcererCast;
    public ParticleSystem SpitTongue;
    public ParticleSystem GoreBlast;
    public ParticleSystem SpectralBlast;
    public ParticleSystem BloodShot;

    public ParticleSystem SquidBlast;
    public ParticleSystem Screech;

    // private ParticleSystem.EmitParams sorcererCastParams;
    // private ParticleSystem.EmitParams spitTongueParams;

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

    public void SpawnSorcererCast(Transform spawn, int x)
    {
    for (int i = 0; i < x; i++)
    {
        var sorcererCastParams = new ParticleSystem.EmitParams();
        sorcererCastParams.position = spawn.position + Random.insideUnitSphere * 0.5f;
        sorcererCastParams.applyShapeToPosition = true;

        SorcererCast.Emit(sorcererCastParams, 1);
    }
    }

    public void SpawnSquidBlast(Transform spawn)
    {
        var squidBlastParams = new ParticleSystem.EmitParams();
        squidBlastParams.position = spawn.position;

        SquidBlast.Emit(squidBlastParams, 1);
    }

    public void SpawnSpitTongue(Transform spawn)
    {
        var spitTongueParams = new ParticleSystem.EmitParams();
        spitTongueParams.position = spawn.position;

        SpitTongue.Emit(spitTongueParams, 1);
    }

    public void SpawnGoreBlast(Transform spawn)
    {
        var goreBlastParams = new ParticleSystem.EmitParams();
        goreBlastParams.position = spawn.position;

        GoreBlast.Emit(goreBlastParams, 1);
    }

    public void SpawnBloodShot(Transform spawn)
    {
        var bloodShotParams = new ParticleSystem.EmitParams();
        bloodShotParams.position = spawn.position;

        BloodShot.Emit(bloodShotParams, 1);
    }

    public void SpawnSpectralBlast(Transform spawn)
    {
        var spectralBlastParams = new ParticleSystem.EmitParams();
        spectralBlastParams.position = spawn.position;

        SpectralBlast.Emit(spectralBlastParams, 1);
    }

    public void SpawnScreech(Transform spawn)
    {
        var screechParams = new ParticleSystem.EmitParams();
        screechParams.position = spawn.position;

        Screech.Emit(screechParams, 1);

        trackedParticles.Add(new ParticleEntry
        {
            target = spawn,
            remainingLifetime = Screech.main.startLifetime.constantMax
        });
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

    void LateUpdate()
    {
        for(int i = trackedParticles.Count - 1; i >= 0; i--)
        {
            var e = trackedParticles[i];
            e.remainingLifetime -= Time.deltaTime;

            if(e.target == null || e.remainingLifetime <= 0f)
            {
                trackedParticles.RemoveAt(i);
                continue;
            }
            trackedParticles[i] = e;
        }

        if(trackedParticles.Count == 0) return;

        int count = Screech.particleCount;
        if (count == 0) return;

        if(particleBuffer == null || particleBuffer.Length < count)
        {
            particleBuffer = new ParticleSystem.Particle[count];
        }

        Screech.GetParticles(particleBuffer,count);

        for(int i = 0; i < trackedParticles.Count && i < count; i++)
        {
            var entry = trackedParticles[i];
            if(entry.target == null) continue;

            int best = 0;
            float bestDelta = float.MaxValue;
            for (int j = 0; j < count; j++)
            {
                float delta = Mathf.Abs(particleBuffer[j].remainingLifetime - entry.remainingLifetime);
                if(delta < bestDelta)
                {
                    bestDelta = delta;
                    best = j;
                }

            }
            particleBuffer[best].position = entry.target.position;
        }
        Screech.SetParticles(particleBuffer, count);
    }
}
