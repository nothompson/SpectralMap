using UnityEngine;

public class SorcererCastBehavior : PredictedRangedBehavior
{
    public Transform particleSpawn;
    public override void Fire()
    {
        base.Fire();
        ProjectileParticleManager.Instance.SpawnSorcererCast(particleSpawn, 10);
    }

}
