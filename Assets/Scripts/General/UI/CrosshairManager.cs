using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CrosshairManager : MonoBehaviour
{
    public static CrosshairManager Instance;

    public GameObject Crosshair;
    public Sprite[] Crosshairs;
    public Image crosshairImage;
    public RectTransform crosshairRect;

    public Dictionary<Sprite, string> CrosshairDict;

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
        SetupDictionary();
    }

    void SetupDictionary()
    {
        CrosshairDict = new Dictionary<Sprite, string>
        {
            {Crosshairs[0], "Flesh Maw"},
            {Crosshairs[1], "Flesh Spawn"},
            {Crosshairs[2], "Slimeball"},
            {Crosshairs[3], "Divine Touch"},
            {Crosshairs[4], "Polluted Touch"},
            {Crosshairs[5], "Infested Spectrum"},
            {Crosshairs[6], "Spectral Eye"},
            {Crosshairs[7], "Brain Encasing"},
            {Crosshairs[8], "Pink Matter"},
            {Crosshairs[9], "Fungal Crossing"},
            {Crosshairs[10], "Mold Circle"},
            {Crosshairs[11], "Pocket Watch"},
            {Crosshairs[12], "Dimensional Wheel"},
            {Crosshairs[13], "Enhanced Targeting"},
            {Crosshairs[14], "Default"},
            {Crosshairs[15], "Launcher"},
            {Crosshairs[16], "Window"},
            {Crosshairs[17], "Scatter"},
            {Crosshairs[18], "Divine Spiral"},
            {Crosshairs[19], "Sharpshooter"},
            {Crosshairs[20], "Classic"},
            {Crosshairs[21], "Beam"},
            {Crosshairs[22], "Spearhead"},
            
        };
    }

    void Start()
    {
        crosshairImage.sprite = Crosshairs[SettingsMenu.SettingsData.crosshairIndex];

        crosshairImage.color = SettingsMenu.SettingsData.crosshairColor;
        
        crosshairRect.localScale = SettingsMenu.SettingsData.crosshairScale;

        crosshairRect.localEulerAngles = SettingsMenu.SettingsData.crosshairRotation;
    }

    public void Activate()
    {
        if (!Crosshair.activeInHierarchy)
        {
            Crosshair.SetActive(true);
        }
        else
        {
            Crosshair.SetActive(false);
        }
    }
}
