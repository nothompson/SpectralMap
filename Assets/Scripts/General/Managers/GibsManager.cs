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

    public List<Transform> Gib(Vector3 pos, int n, bool returnAfter = true)
    {
        List<Transform> spawnedGibs = new List<Transform>();
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
                DecalManager.Instance.RegisterParticleSystem(ps);
                ps.Play();
            }

            if(returnAfter){
            StartCoroutine(ReturnAfter(gib, despawnTime));
            }

            spawnedGibs.Add(gib.transform);
            }
        }

        return spawnedGibs;
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
            ps.Stop();
            ps.Clear();
            DecalManager.Instance.Unregister(ps);

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
