using UnityEngine;

public class EnemyParticle : MonoBehaviour
{
    public enum ParticleType
    {
        SorcererCast,
        SpitTongue,
        GoreBlast,
        SpectralBlast,
        BloodShot,
        SquidBlast,
        Screech,
        Skulls,
        Pollutant

    }

    [SerializeField] private ParticleType type;
    [SerializeField] private Transform overrideSpawn;

    public void Spawn(Transform attackPoint)
    {
        Transform spawn = overrideSpawn != null ? overrideSpawn : attackPoint;

        switch(type)
        {
            case ParticleType.SorcererCast:
                ProjectileParticleManager.Instance.SpawnSorcererCast(spawn, 10);
                break;
            case ParticleType.SpitTongue:
                ProjectileParticleManager.Instance.SpawnSpitTongue(spawn);
                break;
            case ParticleType.GoreBlast:
                ProjectileParticleManager.Instance.SpawnGoreBlast(spawn);
                break;
            case ParticleType.SpectralBlast:
                ProjectileParticleManager.Instance.SpawnSpectralBlast(spawn);
                break;
            case ParticleType.BloodShot:
                ProjectileParticleManager.Instance.SpawnBloodShot(spawn);
                break;
            case ParticleType.SquidBlast:
                ProjectileParticleManager.Instance.SpawnSquidBlast(spawn);
                break;
            case ParticleType.Screech:
                ProjectileParticleManager.Instance.SpawnScreech(spawn);
                break;
            case ParticleType.Skulls:
                ProjectileParticleManager.Instance.SpawnSkulls(spawn);
                break;
            case ParticleType.Pollutant:
                ProjectileParticleManager.Instance.SpawnPollutantBlast(spawn);
                break;
        }
    }
}