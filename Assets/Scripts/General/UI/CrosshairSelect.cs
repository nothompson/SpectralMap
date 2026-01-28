    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using TMPro;    

public class CrosshairSelect : MonoBehaviour
{
    public Image TargetImage;
    public Slider slider;
    private Sprite currentCrosshair;

    public SpriteText text;
    private Sprite[] Crosshairs;

    void Awake()
    {
        Crosshairs = CrosshairManager.Instance.Crosshairs;
        slider.minValue = 0;
        slider.maxValue = Crosshairs.Length - 1;

        slider.value = SettingsMenu.SettingsData.crosshairIndex;
        currentCrosshair = Crosshairs[SettingsMenu.SettingsData.crosshairIndex];

        UpdateDisplay();
    }

    public void ChangeCrosshair()
    {
        currentCrosshair = Crosshairs[(int)slider.value];
        SettingsMenu.SettingsData.crosshairIndex = (int)slider.value;
        GameSettings.Save(SettingsMenu.SettingsData);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        TargetImage.sprite = currentCrosshair;
        CrosshairManager.Instance.crosshairImage.sprite = currentCrosshair;
            if(CrosshairManager.Instance.CrosshairDict.TryGetValue(currentCrosshair, out string input))
            {
                text.input = input;
                text.Refresh();
            }
    }

}
