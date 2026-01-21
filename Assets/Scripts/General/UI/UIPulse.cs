using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIPulse : MonoBehaviour
{
    [SerializeField] private AnimationCurve yCurve;
    [SerializeField] private AnimationCurve xCurve;
    private Vector3 scale;
    private RectTransform rect;
    void Start()
    {
        rect = GetComponent<RectTransform>();
        scale = rect.localScale;
    }

    public IEnumerator Pulse(float dur)
    {
        float t = 0f;
        Vector3 start = scale;
        Vector3 target = start;
        while(t < dur)
        {
            t += Time.deltaTime;
            float time = t / dur;
            float yVal = yCurve.Evaluate(time);
            float xVal = xCurve.Evaluate(time);
            target.y = start.y * yVal;
            target.x = start.x * xVal;
            rect.localScale = target;
            yield return null;
        }
        rect.localScale = start;
    }
}
