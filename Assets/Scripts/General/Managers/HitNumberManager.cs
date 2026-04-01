using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HitNumberManager : MonoBehaviour
{
    public static HitNumberManager Instance;
    
    [System.Serializable]
    public struct HitNumberPrefabEntry
    {
        public HitNumber.HitType type;
        public GameObject prefab; 
    }

    [SerializeField] private HitNumberPrefabEntry[] prefabs;

    [SerializeField] private int poolSize = 20;

    private Queue<HitNumber> pool = new Queue<HitNumber>();

    private Dictionary<HitNumber.HitType, Queue<HitNumber>> pools = new();
    private Dictionary<HitNumber.HitType, GameObject> prefabLookup = new();

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
            return;
        }
        Lookup();
        InitPools();
    }

    private void Lookup()
    {
        foreach(var entry in prefabs)
        {
            prefabLookup[entry.type] = entry.prefab;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearPool();
        InitPools();
    }

    private void ClearPool()
    {
        pools.Clear();
    }

    void InitPools()
    {
       foreach(var entry in prefabs){
            var queue = new Queue<HitNumber>();
            for(int i = 0; i < poolSize; i++)
            {
                queue.Enqueue(CreateNew(entry.prefab));
            }
            pools[entry.type] = queue;
            }
    }

    private HitNumber CreateNew(GameObject prefab)
    {
        HitNumber num = Instantiate(prefab).GetComponent<HitNumber>();
        num.gameObject.SetActive(false);
        return num;
    }

    public HitNumber DisplayHitNumber(float input, Transform t, HitNumber.HitType type)
    {

        if(!pools.TryGetValue(type, out var pool))
        {
            return null;
        }

        HitNumber num = pool.Count > 0 ? pool.Dequeue() : CreateNew(prefabLookup[type]);
        num.Type = type;
        num.OnSpawn(input, t.position + Vector3.up);
        return num;
    }

    public void ReturnHitNumber(HitNumber num)
    {
        num.gameObject.SetActive(false);
        pools[num.Type].Enqueue(num);
    }
}
