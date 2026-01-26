using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetManager : MonoBehaviour
{
    public static ResetManager Instance;
    public GameObject Canvas;
    public RawImage image;
    public Texture2D texture;
    public Material material;

    [SerializeField] private AnimationCurve transitionCurve;
    [SerializeField] private float transitionDur;
     [SerializeField] private float FlowStrength;
    Coroutine transitionRoutine;
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

    public void StartReset()
    {
        if(PauseManager.Instance.paused) return; 
        
        if(transitionRoutine !=null) 
        {
            StopCoroutine(transitionRoutine);
            StopReset();
        }

        transitionRoutine = StartCoroutine(ScreenShot());
    }

    public IEnumerator ScreenShot()
    {
        yield return new WaitForEndOfFrame();
        texture = ScreenCapture.CaptureScreenshotAsTexture();

        material.SetTexture("_Input", texture);
        image.material = material;

        Vector2 random = new Vector2(Random.Range(-0.05f, 0.05f),Random.Range(-0.05f, 0.05f));

        Canvas.SetActive(true);

        material.SetVector("_Random", random);
        material.SetFloat("_Dur", transitionDur);
        material.SetFloat("_FlowStrength", 0f);

        float t = 0f;
        while(t < transitionDur)
        {
            t += Time.deltaTime;
            float time = t / transitionDur;
            float alpha = transitionCurve.Evaluate(time);
            float strength = transitionCurve.Evaluate(1f - time);
            material.SetFloat("_Alpha", alpha);
            material.SetFloat("_FlowStrength", strength * FlowStrength);
            yield return null;
        }
        StopReset();

    }

    void StopReset()
    {
        material.SetFloat("_Alpha", 0f);
        material.SetFloat("_Dur", 0);
        material.SetFloat("_FlowStrength", 0f);

        Canvas.SetActive(false);
        Object.Destroy(texture);
    }
}
