using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance; 

    [SerializeField] private float targetFlashValue;
    [SerializeField] private float flashDur;
    [SerializeField] private AnimationCurve FlashCurve;
    
    private Coroutine flashRoutine;

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
    }

    public void StartFlash(GameObject damaged)
    {
        if(flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DamageFlash(damaged));
    }

    public IEnumerator DamageFlash(GameObject damaged)
    {
        SkinnedMeshRenderer skin = damaged.GetComponentInChildren<SkinnedMeshRenderer>();
        Material mat = skin.material;

            mat.EnableKeyword("_EMISSION");

            Color emissionColor = mat.GetColor("_EmissionColor");
            
            float t = 0f;

            while(t < flashDur)
            {
                t += Time.deltaTime;

                float elapsed = Mathf.Clamp01(t / flashDur);
                float value = FlashCurve.Evaluate(elapsed);

                emissionColor.r = targetFlashValue * value;
                emissionColor.g = targetFlashValue * value;
                emissionColor.b = targetFlashValue * value;

                mat.SetColor("_EmissionColor", emissionColor);
                
                yield return null; 
            }
    }

    
}
