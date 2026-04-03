using UnityEngine;

public class SpawnerBehavior : AttackBehavior
{
    [SerializeField] private int NumberToSpawn;
    [SerializeField] private float SpawnAngle;
    
    public override float Fire()
    {
        for(int i = 0; i < NumberToSpawn; i++)
        {
            GameObject instance = Instantiate(AttackPrefab, AttackPoint.position, AttackPoint.rotation);

            LeachEnemy leach = instance.GetComponent<LeachEnemy>();

            if(leach != null)
            {
                leach.spawnDir = RandomSpawn(AttackPoint.up, SpawnAngle);
            }
        }
        return Cooldown;
    }

    Vector3 RandomSpawn(Vector3 axis, float degrees)
    {
        float angle = Random.Range(0f, degrees) * Mathf.Deg2Rad;
        float azimuth = Random.Range(0f,360f) * Mathf.Deg2Rad;

        float x = Mathf.Sin(angle) * Mathf.Cos(azimuth);
        float y = Mathf.Cos(angle);
        float z = Mathf.Sin(angle) * Mathf.Sin(azimuth);

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, axis);
        return rotation * new Vector3(x,y,z);
    }
}
