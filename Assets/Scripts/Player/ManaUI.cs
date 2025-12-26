using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaUI : SpriteUI
{
    private MagicManagement mm;

    Color og = Color.white;

    public Color error = Color.red;

    bool ErrorStarted = false;

    override public void Awake()
    {
        mm = GetComponentInParent<MagicManagement>();
        base.Awake();
    }

    void Update()
    {
        Calculate(mm.magicPoints, mm.maximumMagic, 1f, 8f, 5f, true);
    }

    public void Error()
    {
        // StopAllCoroutines();
        if(!ErrorStarted)
            StartCoroutine(ErrorRoutine());
    }
    
    public IEnumerator ErrorRoutine()
    {
        ErrorStarted = true;
        float timer = 0f;
        float dur = 0.1f;


        while (timer < dur)
        {
            timer += Time.deltaTime;
            float t = timer / dur;
            image.color = Color.Lerp(og, error, t);
            yield return null;
        }
        yield return new WaitForSeconds(0.025f);

        timer = 0f;

        while (timer < dur)
        {
            timer += Time.deltaTime;
            float t = timer / dur;
            image.color = Color.Lerp(error, og, t);
            yield return null;
        }

        image.color = og;
        ErrorStarted = false;
    }
}
