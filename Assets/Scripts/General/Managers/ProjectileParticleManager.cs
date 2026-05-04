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
        public ParticleSystem system;
    }

    private List<ParticleEntry> trackedParticles = new List<ParticleEntry>();
    private ParticleSystem.Particle[] particleBuffer;
    private Dictionary<ParticleSystem, ParticleSystem.Particle[]> particleBuffers = new ();

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

    public ParticleSystem Skulls;
    public ParticleSystem PollutantBlast;

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

    private void SpawnParticle(ParticleSystem ps, Transform spawn, int count, float positionJitter = 0f, bool track = false)
    {
        var emitParams = new ParticleSystem.EmitParams();

        for(int i = 0; i < count; i++)
        {
            emitParams.position = positionJitter > 0f ? spawn.position + Random.insideUnitSphere * positionJitter : spawn.position;
            emitParams.applyShapeToPosition = true;

            ps.Emit(emitParams, 1);

            if (track)
            {
                trackedParticles.Add(new ParticleEntry{
                    target = spawn,
                    remainingLifetime = ps.main.startLifetime.constantMax,
                    system = ps
                });
            }
        }
    }

    public void Register(Fireball fireball)
    {
        activeFireballs.Add(fireball);
    }
    public void Delete(Fireball fireball)
    {
        activeFireballs.Remove(fireball);
    }

    public void SpawnSorcererCast(Transform spawn, int x, bool track = false)
        => SpawnParticle(SorcererCast, spawn, x, positionJitter: 0.5f, track);

    public void SpawnSquidBlast(Transform spawn, bool track = false)
        => SpawnParticle(SquidBlast, spawn, 1, track: track);

    public void SpawnSpitTongue(Transform spawn, bool track = false)
        => SpawnParticle(SpitTongue, spawn, 1, track: track);

    public void SpawnGoreBlast(Transform spawn, bool track = false)
        => SpawnParticle(GoreBlast, spawn, 1, track: track);

    public void SpawnBloodShot(Transform spawn, bool track = false)
        => SpawnParticle(BloodShot, spawn, 1, track: track);

    public void SpawnSpectralBlast(Transform spawn, bool track = false)
        => SpawnParticle(SpectralBlast, spawn, 1, track: track);

    public void SpawnScreech(Transform spawn, bool track = true)
        => SpawnParticle(Screech, spawn, 1, track: track);

    public void SpawnSkulls(Transform spawn, bool track = false)
        => SpawnParticle(Skulls, spawn, 15, positionJitter: 2f, track: track);

    public void SpawnPollutantBlast(Transform spawn, bool track = false)
        => SpawnParticle(PollutantBlast, spawn, 1, track: track);

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
            }
            else
            {
                trackedParticles[i] = e;
            }
        }

        var groups = new Dictionary<ParticleSystem, List<ParticleEntry>>();
        foreach(var entry in trackedParticles)
        {
            if(!groups.ContainsKey(entry.system)) groups[entry.system] = new List<ParticleEntry>();

            groups[entry.system].Add(entry);
        }

        foreach(var(ps, entries) in groups)
        {
            int count = ps.particleCount;
            if(count == 0) continue;

            if(!particleBuffers.TryGetValue(ps, out var buffer) || buffer.Length < count)
            {
                buffer = new ParticleSystem.Particle[count];
                particleBuffers[ps] = buffer;
            }

            ps.GetParticles(buffer,count);

            foreach(var entry in entries)
            {
                if(entry.target == null) continue;

                int best = 0;
                float bestDelta = float.MaxValue;
                for(int j = 0; j < count; j++)
                {
                    float delta = Mathf.Abs(buffer[j].remainingLifetime - entry.remainingLifetime);
                    if(delta < bestDelta) { bestDelta = delta; best = j; }
                }
                buffer[best].position = entry.target.position;
            }

            ps.SetParticles(buffer,count);

        }
    }
}
