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

    private Vector3 seed;

    public Vector3 position;
    public Quaternion rotation;
    public Quaternion targetRot;
    public Vector3 scale;

    private bool waiting = true;

    void OnEnable()
    {
        position = transform.localPosition;
        scale = transform.localScale;
        rotation = transform.localRotation;
        targetRot = rotation;
        
        seed = new Vector3(Random.Range(0f,1000f), Random.Range(0f,1000f), Random.Range(0f,1000f));
        StartCoroutine(Jitter());
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
        if(lockPosition) return;
        position = transform.localPosition;
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
        rotation =  Quaternion.Slerp(rotation, targetRot, Time.deltaTime * 5f);

        float xPos = (Mathf.PerlinNoise(Time.time, seed.x) * positionRange) - positionRange * 0.5f;
        float yPos = (Mathf.PerlinNoise(Time.time, seed.y) * positionRange) - positionRange * 0.5f;
        float zPos = (Mathf.PerlinNoise(Time.time, seed.z) * positionRange) - positionRange * 0.5f;

        float xScale = (Mathf.PerlinNoise(Time.time, seed.x) * scaleRange) - scaleRange * 0.5f;
        float yScale = (Mathf.PerlinNoise(Time.time, seed.y) * scaleRange) - scaleRange * 0.5f;
        float zScale = (Mathf.PerlinNoise(Time.time, seed.z) * scaleRange) - scaleRange * 0.5f;

        float xRot = (Mathf.PerlinNoise(Time.time, seed.x) * rotationRange) - rotationRange * 0.5f;
        float yRot = (Mathf.PerlinNoise(Time.time, seed.y) * rotationRange) - rotationRange * 0.5f;
        float zRot = (Mathf.PerlinNoise(Time.time, seed.z) * rotationRange) - rotationRange * 0.5f;


        Vector3 jitPos = new Vector3(xPos,yPos,zPos);

        Vector3 jitEuler = new Vector3(xRot, yRot, zRot);

        Quaternion jitRot = Quaternion.Euler(jitEuler);

        Vector3 jitScale = new Vector3(xScale, yScale, yScale);

        transform.localScale = scale + jitScale;
        transform.localRotation = rotation * jitRot;

        if(!lockPosition){

        transform.localPosition = position + jitPos;
        }



        yield return new WaitForSeconds(wait);
        }
    }
}
