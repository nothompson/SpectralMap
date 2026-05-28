using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshJitter : MonoBehaviour
{
    [SerializeField] private float fps;
    [SerializeField] private float scaleRange;
    [SerializeField] private float rotationRange;
    [SerializeField] private float positionRange = 1f;
    [SerializeField] private bool lockPosition = false;
    [HideInInspector] public bool parented = false;

    private Vector3 seed;

    public Vector3 position;
    public Quaternion rotation;
    public Quaternion targetRot;
    public Vector3 scale;
    private Vector3 worldPosition; 
    private Vector3 worldScale;
    private Quaternion worldRotation;
    private bool waiting = true;
    private bool isVisible = true;

    void OnEnable()
    {
        position = transform.localPosition;
        scale = transform.localScale;
        rotation = transform.localRotation;
        targetRot = rotation;
        
        seed = new Vector3(Random.Range(0f,1000f), Random.Range(0f,1000f), Random.Range(0f,1000f));
        StartCoroutine(Jitter());
    }

    void OnBecameVisible()
    {
        isVisible = true;

    }

    void OnBecameInvisible()
    {
        isVisible = false;
        
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void UpdateBaseValues()
    {
        scale = transform.localScale;
        rotation = transform.localRotation;
        targetRot = rotation;
        worldPosition = transform.position;
        worldRotation = transform.rotation;
        worldScale = transform.lossyScale;
        if (!lockPosition)
        position = transform.localPosition;
        
        StopAllCoroutines();
        
        waiting = true;
        
        StartCoroutine(Jitter());
    }

    public IEnumerator Jitter()
    {
        float wait = 1f / fps;

        while(true){
            if (waiting)
            {
                waiting = false;
                yield return null;
                continue;
            }

            if (!isVisible)
            {
                yield return new WaitForSeconds(wait);
                continue;
            }


        float xScale = (Mathf.PerlinNoise(Time.time, seed.x) * scaleRange) - scaleRange * 0.5f;
        float yScale = (Mathf.PerlinNoise(Time.time, seed.y) * scaleRange) - scaleRange * 0.5f;
        float zScale = (Mathf.PerlinNoise(Time.time, seed.z) * scaleRange) - scaleRange * 0.5f;

        float xRot = (Mathf.PerlinNoise(Time.time, seed.x) * rotationRange) - rotationRange * 0.5f;
        float yRot = (Mathf.PerlinNoise(Time.time, seed.y) * rotationRange) - rotationRange * 0.5f;
        float zRot = (Mathf.PerlinNoise(Time.time, seed.z) * rotationRange) - rotationRange * 0.5f;
        
        Vector3 jitEuler = new Vector3(xRot, yRot, zRot);
        Quaternion jitRot = Quaternion.Euler(jitEuler);

        Vector3 jitScale = new Vector3(xScale, yScale, yScale);

        rotation =  Quaternion.Slerp(rotation, targetRot, Time.deltaTime * 30f);

        if (parented)
        {
            transform.rotation = worldRotation * jitRot;
        
        }
        else
        {
            transform.localRotation = rotation * jitRot;

            transform.localScale = scale + jitScale;

            if(!lockPosition){
                float xPos = (Mathf.PerlinNoise(Time.time, seed.x) * positionRange) - positionRange * 0.5f;
                float yPos = (Mathf.PerlinNoise(Time.time, seed.y) * positionRange) - positionRange * 0.5f;
                float zPos = (Mathf.PerlinNoise(Time.time, seed.z) * positionRange) - positionRange * 0.5f;
                Vector3 jitPos = new Vector3(xPos,yPos,zPos);
                transform.localPosition = position + jitPos;
            }
        }

        yield return new WaitForSeconds(wait);
        }
    }
}