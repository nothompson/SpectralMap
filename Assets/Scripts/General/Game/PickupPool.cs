using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
public class PickupPool : MonoBehaviour
{
    public static PickupPool Instance;

    [System.Serializable]
    public struct PickupPrefabEntry
    {
        public Pickup.PickupType type;
        public GameObject prefab; 
    }

    [SerializeField] private PickupPrefabEntry[] prefabs;
    [SerializeField] private int poolSize = 20;

    private Dictionary<Pickup.PickupType, Queue<Pickup>> pools = new();
    private Dictionary<Pickup.PickupType, GameObject> prefabLookup = new();

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }

        Lookup();
        InitPools();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearPools();
        InitPools();
    }

    private void Lookup()
    {
        foreach(var entry in prefabs)
        {
            prefabLookup[entry.type] = entry.prefab;
        }
    }

    private void InitPools()
    {
        foreach(var entry in prefabs){
            var queue = new Queue<Pickup>();
            for(int i = 0; i < poolSize; i++)
            {
                queue.Enqueue(CreateNew(entry.prefab));
            }
            pools[entry.type] = queue;
            }
    }

    private void ClearPools()
    {
        pools.Clear();
    }


    private Pickup CreateNew(GameObject prefab)
    {
        Pickup p = Instantiate(prefab).GetComponent<Pickup>();
        p.gameObject.SetActive(false);
        return p;
    }

    public Pickup Get(Vector3 position, Pickup.PickupType type, float size)
    {
        if(!pools.TryGetValue(type, out var pool))
        {
            return null;
        }

        Pickup p = pool.Count > 0 ? pool.Dequeue() : CreateNew(prefabLookup[type]);
        p.transform.position = position;
        p.Type = type;
        p.size = size;
        p.gameObject.SetActive(true);
        p.OnSpawn();
        return p;
    }

    public void Return(Pickup p)
    {
        p.gameObject.SetActive(false);
        pools[p.Type].Enqueue(p);
    }
}
