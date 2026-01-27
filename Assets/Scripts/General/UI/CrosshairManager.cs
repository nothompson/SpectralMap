using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CrosshairManager : MonoBehaviour
{
    public static CrosshairManager Instance;

    public GameObject Crosshair;

    
    public Image crosshairImage;
    public RectTransform crosshairRect;

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

        crosshairImage = Crosshair.GetComponent<Image>();
        crosshairRect = Crosshair.GetComponent<RectTransform>();
    }

}
