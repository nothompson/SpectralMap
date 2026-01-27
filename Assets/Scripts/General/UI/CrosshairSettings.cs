
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using TMPro;    

public class CrosshairSettings : MonoBehaviour
{
    public GameObject Crosshair;
    public Slider Red;
    public Slider Green;
    public Slider Blue;
    public Slider Alpha;

    public Slider Scale;

    private Image crosshairImage;
    private RectTransform crosshairRect;

    public void Awake()
    {
        crosshairImage = Crosshair.GetComponent<Image>();
        crosshairRect = Crosshair.GetComponent<RectTransform>();
    }
    public void ChangeColor()
    {
        crosshairImage.color = new Color(Red.value, Green.value, Blue.value, Alpha.value);
        CrosshairManager.Instance.crosshairImage.color = new Color(Red.value, Green.value, Blue.value, Alpha.value);
    }

    public void ChangeScale()
    {
        crosshairRect.localScale = new Vector3(Scale.value,Scale.value,Scale.value);
        CrosshairManager.Instance.crosshairRect.localScale = new Vector3(Scale.value,Scale.value,Scale.value);
    }
}
