using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabPool : MonoBehaviour
{
    public static PrefabPool Instance;
    [System.Serializable]
    public class PoolEntry
    {
        public string id;
        public GameObject prefab;
        public int size = 10;
    }

    public List<PoolEntry> entries;
    private Dictionary<string, Queue<GameObject>> pools = new();

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
        Init();
    }

    void Init()
    {
        foreach(var entry in entries)
        {
            Queue<GameObject> q = new Queue<GameObject>();

            for(int i = 0; i < entry.size; i++)
            {
                GameObject obj = Instantiate(entry.prefab);
                obj.SetActive(false);
                q.Enqueue(obj);
            }

            pools[entry.id] = q;
        }
    }

    public GameObject Get(string id)
    {
        if(!pools.TryGetValue(id, out var q)) return null;

        GameObject obj = q.Count > 0 ? q.Dequeue() : Instantiate(entries.Find(e => e.id == id).prefab);

        obj.SetActive(true);
        return obj;
    }

    public void Return(string id, GameObject obj)
    {
        obj.SetActive(false);
        pools[id].Enqueue(obj);
    }
}
