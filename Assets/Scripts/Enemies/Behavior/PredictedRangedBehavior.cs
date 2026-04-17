using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PredictedRangedBehavior : AttackBehavior
{
    [Header("Projectile Settings")]
    [SerializeField] public float projectileSpeed = 20f;
    [SerializeField] float predictionClamp = 8f;

    [Header("Projectile Modifiers")]
    [SerializeField] private float AutoTimer = 1f;
    [SerializeField] private float ExplosionRadius = 1f;

    [HideInInspector] public Enemy enemy;
    Transform player;

    [HideInInspector] public Vector3 predictedTarget;

    readonly Queue<Vector3> positionSamples = new();
    int maxSamples = 10;

    public override void InitBehavior(GameObject owner, Transform point)
    {
        base.InitBehavior(owner, point);
        enemy = owner.GetComponent<Enemy>();
        player = enemy.player;
        StartTracking();
    }

    public void StartTracking()
    {
        positionSamples.Clear();
        StopAllCoroutines(); 
        StartCoroutine(TrackPlayer());
    }

    IEnumerator TrackPlayer()
    {
        while (true)
        {
            Vector3 sample = new Vector3(player.position.x, player.position.y + 0.33f, player.position.z);
            positionSamples.Enqueue(sample);

            // Maintain maxSamples in buffer
            while (positionSamples.Count > maxSamples)
                positionSamples.Dequeue();

            yield return new WaitForFixedUpdate();
        }
    }

    public Vector3 PredictTarget()
    {
        if (positionSamples.Count < 2)
            return new Vector3(player.position.x, player.position.y + 0.33f, player.position.z);

        Vector3[] samples = positionSamples.ToArray();
        int n = samples.Length;

        float totalTime = (n - 1) * Time.fixedDeltaTime;

        //delta velocity 
        Vector3 velocity = (samples[n - 1] - samples[0]) / totalTime;

        // travel time and distance needed for prediction
        float dist = Vector3.Distance(enemy.transform.position, samples[n - 1]);
        float travelTime = dist / Mathf.Max(projectileSpeed, 0.01f);

        // last position + rate of change in velocity * time
        Vector3 predicted = samples[n - 1] + velocity * travelTime;

        // clamp to avoid outliers freaking whole prediciton
        Vector3 offset = predicted - samples[n - 1];
        if (offset.magnitude > predictionClamp)
            predicted = samples[n - 1] + offset.normalized * predictionClamp;

        return predicted;
    }

    public override void Fire()
    {
        predictedTarget = PredictTarget();
        OnFire?.Invoke(AttackPoint);
        SpawnProjectile(predictedTarget);
        StartCoroutine(CooldownRoutine());
    }

    void SpawnProjectile(Vector3 target)
    {
        if (AttackPrefab == null || AttackPoint == null) return;

        Vector3 dir = (target - AttackPoint.position).normalized;

        GameObject hitbox = Instantiate(AttackPrefab, AttackPoint.position, Quaternion.LookRotation(dir));

        EnemyProjectile projectile = hitbox.GetComponent<EnemyProjectile>();
        Rocket rocket = hitbox.GetComponent<Rocket>();
        Grenade grenade = hitbox.GetComponent<Grenade>();

        if (projectile != null)
        {
            projectile.damage = Damage;
            projectile.thisEnemy = Owner;
            projectile.speed = projectileSpeed;
        }

        if (rocket != null)
        {
            rocket.explosionForce = Force;
            rocket.maximumDamage = Damage;
            rocket.explosionRadius = ExplosionRadius;
        }

        if (grenade != null)
        {
            grenade.autoTimer = AutoTimer;
        }
    }
}