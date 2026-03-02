using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GrapplePooling : MonoBehaviour
{
    [SerializeField] private GameObject segment;
    [SerializeField] private GameObject grappleHead;
    [SerializeField] private int defaultCapacity;
    [SerializeField] private int maxSize;
    [SerializeField] private Vector3 segmentScale;
    [SerializeField] private Vector3 headScale;

    private IObjectPool<GameObject> pool;
    private IObjectPool<GameObject> headPool;

    void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: () => CreateSegment(segment),
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroySegment,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        headPool = new ObjectPool<GameObject>(
            createFunc: () => CreateSegment(grappleHead),
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroySegment,
            collectionCheck: true,
            defaultCapacity: 2,
            maxSize: 4
        );

    }

    private GameObject CreateSegment(GameObject prefab)
    {
        GameObject seg = Instantiate(prefab);
        seg.SetActive(false);
        return seg;
    }

    public Transform GetSegmentTransform()
    {
        GameObject seg = pool.Get();
        seg.transform.rotation = Random.rotation;
        return seg.transform;
    }

    public Transform GetHeadTransform()
    {
        GameObject seg = headPool.Get();
        return seg.transform;
    }

    public void ReturnSegmentTransform(Transform t)
    {
        if(t != null)
        {
            t.localScale = segmentScale;
            t.position = Vector3.zero;
            pool.Release(t.gameObject);
        }
    }

     public void ReturnHeadTransform(Transform t)
    {
        if(t != null)
        {
            t.localScale = headScale;
            t.position = Vector3.zero;
            headPool.Release(t.gameObject);
        }
    }


    public void ClearSegmentTransforms(List<Transform> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            ReturnSegmentTransform(list[i]);
        }
        list.Clear();
    }

    private void OnGet(GameObject seg)
    {
        if(seg != null)
        {
            seg.SetActive(true);
        }
    }

    private void OnRelease(GameObject seg)
    {
        if(seg == null) return;
        seg.SetActive(false);
    }

    private void OnDestroySegment(GameObject seg)
    {
        if(seg != null) Destroy(seg);
    }

    public void ClearPool()
    {
        if(pool != null)
        {
            pool.Clear();
        }
        if(headPool != null)
        {
            headPool.Clear();
        }
    }
    
}
