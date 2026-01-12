using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIHoverJuice : MonoBehaviour
{
    [SerializeField] private AnimationCurve ScaleCurve;
    [SerializeField] private AnimationCurve RotationCurve;
    [SerializeField] private float jiggleDur; 

    [SerializeField] private float rotation; 

    Coroutine hoverRoutine;

    RectTransform rect;
    Vector3 scale;
    Vector3 rot;
    public void Start()
    {
        rect = gameObject.GetComponent<RectTransform>();
        scale = rect.localScale;
        rot = rect.localEulerAngles;
    }

    public void StartHover(bool intro)
    {
        ResetRoutines();
        hoverRoutine = StartCoroutine(Hover(intro));
    }

    public IEnumerator Hover(bool intro)
    {
        float t = 0f;

        float dur = intro ? jiggleDur : jiggleDur * 0.5f;

        Vector3 current = rect.localScale;
        Vector3 target = scale;
        bool rand = Random.Range(0f, 1f) > 0.5f;

        Debug.Log(rand);

        float targetRotation = rand ? -1f * rotation * Random.Range(0.75f, 1.25f) : rotation * Random.Range(0.75f, 1.25f);


        while(t < jiggleDur)
        {    
            t += Time.unscaledDeltaTime;
            float elapsed = (t / jiggleDur);
            float curve = intro ? elapsed : 1f - elapsed;
            float scaleValue = ScaleCurve.Evaluate(curve);
            float rotValue = intro ? RotationCurve.Evaluate(curve) : 0f;
            rect.localScale = Vector3.LerpUnclamped(current, target * scaleValue, elapsed);
            rect.localEulerAngles = new Vector3(0f, 0f, rot.z + (targetRotation * rotValue));
            yield return null;
        }

        rect.localScale = intro ? target * ScaleCurve.Evaluate(1f) : scale;

    }

    void ResetRoutines()
    {
        if(hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
            hoverRoutine = null;
        }


    }


}
