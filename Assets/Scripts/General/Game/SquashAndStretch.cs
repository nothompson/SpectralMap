using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class SquashAndStretch : MonoBehaviour
{
    [SerializeField] public Transform TargetTransform;

    [SerializeField] public AnimationCurve Animation;
    // [SerializeField] public AnimationCurve VerticalAnimation;
    [SerializeField] public float AnimationDur;
    [SerializeField] public UnityEvent PeakDeviationEvent;

    [SerializeField] public UnityEvent EndEvent;
    [SerializeField] public bool Loop = false;
      [SerializeField] public bool horizontal = true;
    private Vector3 InitialScale;
    private float maxDeviation;
    private bool peakEventTriggered;

    void Start()
    {
        InitialScale = TargetTransform.localScale;
        maxDeviation = FindMaxDeviation();
        if (Loop)
        {
            Play();
        }
    }

    public void Play()
    {
        StartCoroutine(Begin());
    }

    private float FindMaxDeviation()
    {
        int sampleCount = 256;
        float max = float.MinValue;
        for(int i =0; i <= sampleCount; i++)
        {
            float time = (float)i / sampleCount;
            float deviation = Mathf.Abs(Animation.Evaluate(time) - 1f);
            if(deviation > max) max = deviation;
        }
        return max;
    }

    IEnumerator Begin()
    {
        if(TargetTransform == null) yield break;
        float t = 0f;
        peakEventTriggered = false;
        Vector3 cachedScale = InitialScale;
        Vector3 targetScale = cachedScale;
        while(t < AnimationDur)
        {
            t += Time.deltaTime;
            float time = t / AnimationDur;
            float deviation = Animation.Evaluate(time) - 1f;

            if(!peakEventTriggered && Mathf.Abs(deviation) >= maxDeviation * 0.95f)
                {
                    peakEventTriggered = true;
                    PeakDeviationEvent.Invoke();    
                }

            float verticalValue = 1f + deviation;
            float horizontalValue = 1f - deviation;

            targetScale = horizontal ? new Vector3(cachedScale.x * horizontalValue, cachedScale.y * verticalValue, cachedScale.z * horizontalValue) : new Vector3(cachedScale.x, cachedScale.y * verticalValue, cachedScale.z) ;
            
            TargetTransform.localScale = targetScale;
            yield return null;
        }
        TargetTransform.localScale = InitialScale;
        if(Loop) Play();
        if(EndEvent != null) EndEvent.Invoke();
    }
}
