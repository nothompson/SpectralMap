    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using TMPro;    

public class CrosshairSelect : MonoBehaviour
{
    public Image TargetImage;
    public Sprite[] Crosshairs;
    public Slider slider;

    private Sprite currentCrosshair;

    void Awake()
    {
        slider.minValue = 0;
        slider.maxValue = Crosshairs.Length - 1;
        if(currentCrosshair == null) currentCrosshair = Crosshairs[0];
        UpdateDisplay();
    }

    public void ChangeCrosshair()
    {
        currentCrosshair = Crosshairs[(int)slider.value];
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        TargetImage.sprite = currentCrosshair;
    }

}
