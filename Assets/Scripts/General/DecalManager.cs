using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalManager : MonoBehaviour
{
    public static DecalManager Instance { get; private set; }

    [Header("Reference")]
    public ParticleSystem bloodSplatter;

    public ParticleSystem fireSplatter;

    [Header("Params")]
    public float minSize = 0.3f;
    public float maxSize = 0.8f;

    private ParticleSystem.EmitParams emitParams;

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

        emitParams = new ParticleSystem.EmitParams();
    }

   public void SpawnDecal(Vector3 position, Vector3 normal, ParticleSystem ps)
    {
    if (ps == null) return;

    emitParams.position = position + normal * 0.01f;

    Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
    
    rotation *= Quaternion.AngleAxis(0, normal);

    emitParams.rotation3D = rotation.eulerAngles;
    emitParams.startSize = Random.Range(minSize, maxSize);

    ps.Emit(emitParams, 1);
    }
}
