using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkStep : MonoBehaviour
{
    public float shrinkDuration = 3f;
    public AnimationCurve Curve; 

    public GameObject Container;

    Coroutine Shrinking;

    public FMODUnity.StudioEventEmitter sound;

    public void OnTriggerEnter(Collider other)
    {
        if(Shrinking != null) return;

        sound.Play();
        
        Shrinking = StartCoroutine(Shrink());
    }

    IEnumerator Shrink()
    {
        Vector3 cachedScale = transform.localScale;
        float t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float elapsed = t / shrinkDuration;
            float value = Curve.Evaluate(elapsed);

            if(value <= 0.2f) Container.SetActive(false);
            else Container.SetActive(true);

            Vector3 target = cachedScale * value;

            transform.localScale = target;

            yield return null;
        }

        transform.localScale = cachedScale;
        Shrinking = null;
    }
}