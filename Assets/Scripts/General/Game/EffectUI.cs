using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectUI : MonoBehaviour
{
    public RectTransform rect;
    public Image image;
    public AnimationCurve IntroCurve;
    public AnimationCurve OutroCurve;
    public float introDur;
    public float EffectDuration;

    void Start()
    {
        rect.localScale = Vector3.zero;
        StartCoroutine(Intro());
        StartCoroutine(Outro());
    }

    IEnumerator Intro()
    {
        float t = 0f;
        while(t < introDur)
        {
            t += Time.deltaTime;
            float elapsed = t / introDur;

            float value = IntroCurve.Evaluate(elapsed);

            rect.localScale = Vector3.one * value;
            yield return null;  
        }
        rect.localScale = Vector3.one;
    }

    IEnumerator Outro()
    {
        float t = 0f;
        Color target = new Color (1f,1f,1f,1f);
        while(t < EffectDuration)
        {
            t += Time.deltaTime;
            float elapsed = t / EffectDuration;

            float value = OutroCurve.Evaluate(elapsed);
            target.a = value;
            image.color = target;
            yield return null;
        }
        target.a = 0;
        image.color = target;
    }
}