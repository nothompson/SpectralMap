    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using TMPro;    

public class FOVSettings : MonoBehaviour
{
    public Slider slider;
    public SpriteText text;

    public static int CurrentFOV;

    public void Awake()
    {
        slider.value = SettingsMenu.SettingsData.fov;
        UpdateDisplay();
    }

    public void ChangeFOV()
    {
        CurrentFOV = (int)slider.value;
        SettingsMenu.SettingsData.fov = (int)slider.value;
        GameSettings.Save(SettingsMenu.SettingsData);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        text.input = "FOV: " + slider.value;
        text.Refresh();
    }
}
