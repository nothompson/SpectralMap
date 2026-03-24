using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HitNumberManager : MonoBehaviour
{
    public static HitNumberManager Instance;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 20;

    private Queue<HitNumber> pool = new Queue<HitNumber>();
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
        InitPool();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearPool();
        InitPool();
    }

    private void ClearPool()
    {
        pool.Clear();
    }

    void InitPool()
    {
        for(int i = 0; i <= poolSize; i++)
        {
            pool.Enqueue(CreateNew());
        }
    }

    private HitNumber CreateNew()
    {
        HitNumber num = Instantiate(prefab).GetComponent<HitNumber>();
        num.gameObject.SetActive(false);
        return num;
    }

    public void DisplayHitNumber(float input, Transform t)
    {
        HitNumber num = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        num.OnSpawn(input, t.position + Vector3.up);
    }

    public void ReturnHitNumber(HitNumber num)
    {
        num.gameObject.SetActive(false);
        pool.Enqueue(num);
    }
}
