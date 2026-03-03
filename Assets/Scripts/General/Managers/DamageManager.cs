using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance; 
    public GameObject BloodCanvas;
    public GameObject VeinCanvas;

    public Image Blood;
    public Image Vein;

    [SerializeField] private AnimationCurve BloodCurve;
    [SerializeField] private float BloodDur;
    [SerializeField] private AnimationCurve VeinCurve;
    [SerializeField] private float VeinDur;

    Coroutine overlayRoutine;

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

        Blood = BloodCanvas.GetComponentInChildren<Image>();
        Vein = VeinCanvas.GetComponentInChildren<Image>();

        BloodCanvas.SetActive(false);
        VeinCanvas.SetActive(false);
    }

    public void PlayDamageOverlay(float dmg)
    {
        if(overlayRoutine != null)
        {
            StopCoroutine(overlayRoutine);
        }
        overlayRoutine = StartCoroutine(DamageOverlay(dmg));
    }

    IEnumerator DamageOverlay(float damageOffset)
    {
        float t = 0f;
        Color fade = Blood.color;
        Blood.color = new Color(1f,1f,1f,0f);
        BloodCanvas.SetActive(true);

        while(t < BloodDur + damageOffset)
        {
            t += Time.deltaTime;
            float time = Mathf.Clamp01(t / BloodDur);

            fade.a = BloodCurve.Evaluate(time);

            Blood.color = fade;
            yield return null;
        }
        BloodCanvas.SetActive(false);

    }


}
