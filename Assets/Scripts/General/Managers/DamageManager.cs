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


}
