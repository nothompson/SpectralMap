using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GibsManager : MonoBehaviour
{
    public static GibsManager Instance { get; private set; }
    private IObjectPool<GameObject> pool;

    [Header("Gib Array")]
    public GameObject[] gibs;

    [Header("Gib Stats")]
    public float explosionForce = 5f;
    public float despawnTime = 3f;

    [Header("Blood Decals")]
    public LayerMask surfaceLayers = -1;
    public float minTimeBetweenDecals = 0.02f;

    public float minTimeFactor = 0.01f;

    private Dictionary<ParticleSystem, float> lastDecalTime = new Dictionary<ParticleSystem, float>();

    private Dictionary<ParticleSystem, float> decalSpawnCooldown = new Dictionary<ParticleSystem, float>();

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    public FMODUnity.EventReference gibSplat;

    private void Awake()
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

        pool = new ObjectPool<GameObject>(
            createFunc: CreateItem,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyItem,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 20
        );
    }

    public void Gib(Vector3 pos, int n)
    {
        for (int i = 0; i < n; i++)
        {
            GameObject gib = pool.Get();
            if(gib != null){
            gib.transform.position = pos;
            gib.transform.rotation = Random.rotation;

            if (gib.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(Random.onUnitSphere * explosionForce, ForceMode.Impulse);
            }

            FMODUnity.RuntimeManager.PlayOneShot(gibSplat, pos);

            if (gib.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                ps.Play();
            }

            StartCoroutine(ReturnAfter(gib, despawnTime));
            }
        }
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


    public void HandleParticleCollision(ParticleSystem ps, GameObject other)
    {
        if (DecalManager.Instance == null)
        {
            return;
        }

        if (!lastDecalTime.ContainsKey(ps))
        {
            return;
        }

        if (!decalSpawnCooldown.ContainsKey(ps))
        {
            return;
        }

        if (Time.time - lastDecalTime[ps] < decalSpawnCooldown[ps])
        {
            return;
        }

        if (((1 << other.layer) & surfaceLayers) == 0)
        {
            return;
        }

        int numCollisions = ps.GetCollisionEvents(other, collisionEvents);

        if (numCollisions > 0)
        {
            for (int i = 0; i < numCollisions; i++)
            {
                decalSpawnCooldown[ps] += minTimeFactor;
                DecalManager.Instance.SpawnDecal(
                    collisionEvents[i].intersection,
                    collisionEvents[i].normal, 
                    DecalManager.Instance.bloodSplatter
                );
            }
            lastDecalTime[ps] = Time.time;
        }
    }

    private GameObject CreateItem()
    {
        GameObject prefab = gibs[Random.Range(0, gibs.Length)];
        GameObject gib = Instantiate(prefab);
        gib.SetActive(false);
        return gib;
    }

    private void OnGet(GameObject gib)
    {
        if(gib != null){
            gib.SetActive(true);
        }
    }

    private void OnRelease(GameObject gib)
    {
        if (gib == null) return;
        gib.SetActive(false);
        
        if (gib.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (gib.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            minTimeBetweenDecals = 0.05f;
            ps.Stop();
            ps.Clear();

            if (lastDecalTime.ContainsKey(ps))
            {
                lastDecalTime.Remove(ps);
            }

            if (decalSpawnCooldown.ContainsKey(ps))
            {
                decalSpawnCooldown[ps] = minTimeBetweenDecals;
            }
        }
    }

    private void OnDestroyItem(GameObject gib)
    {
        if(gib != null)
            Destroy(gib);
    }

    private IEnumerator ReturnAfter(GameObject gib, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if(gib!=null){
            pool.Release(gib);
        }
    }

    public void ClearPool()
    {
        if(pool != null)
        {
            StopAllCoroutines();
            pool.Clear();
        }
    }
}
