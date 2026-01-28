
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

    public Slider Rotation;

    private Image crosshairImage;
    private RectTransform crosshairRect;

    public void Awake()
    {
        crosshairImage = Crosshair.GetComponent<Image>();
        crosshairRect = Crosshair.GetComponent<RectTransform>();

        crosshairImage.color = SettingsMenu.SettingsData.crosshairColor;
        Red.value = SettingsMenu.SettingsData.crosshairColor.r;
        Green.value = SettingsMenu.SettingsData.crosshairColor.g;
        Blue.value = SettingsMenu.SettingsData.crosshairColor.b;
        Alpha.value = SettingsMenu.SettingsData.crosshairColor.a;
        
        crosshairRect.localScale = SettingsMenu.SettingsData.crosshairScale;
        Scale.value = SettingsMenu.SettingsData.crosshairScale.x;
       
        crosshairRect.localEulerAngles = SettingsMenu.SettingsData.crosshairRotation;
        Rotation.value = SettingsMenu.SettingsData.crosshairRotation.z;
    
    }
    public void ChangeColor()
    {
        if(crosshairImage == null) return;
        Color color = new Color(Red.value, Green.value, Blue.value, Alpha.value);
        crosshairImage.color = color;
        CrosshairManager.Instance.crosshairImage.color = color;
        SettingsMenu.SettingsData.crosshairColor = color;
        GameSettings.Save(SettingsMenu.SettingsData);
    }

    public void ChangeScale()
    {
        if(crosshairRect == null) return;
        Vector3 scale = new Vector3(Scale.value,Scale.value,Scale.value);
        crosshairRect.localScale = scale;
        CrosshairManager.Instance.crosshairRect.localScale = scale;
        SettingsMenu.SettingsData.crosshairScale = scale;
        GameSettings.Save(SettingsMenu.SettingsData);
    }
    public void ChangeRotation()
    {
        Vector3 rotation = new Vector3(0f,0f,Rotation.value);
        if(crosshairRect == null) return;
        crosshairRect.localEulerAngles = rotation;
        CrosshairManager.Instance.crosshairRect.localEulerAngles = rotation;
        SettingsMenu.SettingsData.crosshairRotation = rotation;
        GameSettings.Save(SettingsMenu.SettingsData);
    }
}
