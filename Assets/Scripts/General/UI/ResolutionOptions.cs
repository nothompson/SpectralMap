    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using TMPro;    

public class ResolutionOptions : MonoBehaviour
{
    public SpriteText text;
    [SerializeField] private ApplyResolution apply;
    public Slider slider;
    public bool Window;

    public static FullScreenMode pendingWindow = FullScreenMode.FullScreenWindow;

    public static Resolution pendingResolution;

    static Resolution[] resolutions;

    void Awake()
    {
        if(resolutions == null) resolutions = Screen.resolutions;

        if (!Window)
        {
            slider.minValue = 0;
            slider.maxValue = resolutions.Length - 1;
        }

        UpdateDisplay(Window);
    }

    void Start()
    {
        if (!Window)
        {
            pendingResolution = Screen.currentResolution;

            slider.value = GetResolutionValue(pendingResolution);

            UpdateDisplay(false);
        }
        else
        {
            UpdateDisplay(true);
        }
    }

    public void ChangeWindowType()
    {
        pendingWindow = GetModeFromSlider();
        UpdateDisplay(true);
    }

    public void ChangeResolution()
    {
        pendingResolution = resolutions[(int)slider.value];
        UpdateDisplay(false);
    }

    public void ValueChange()
    {
        apply.MakeActive();
    }


    FullScreenMode GetModeFromSlider()
    {
        switch ((int)slider.value)
        {
            case 1: return FullScreenMode.FullScreenWindow;
            case 2: return FullScreenMode.Windowed;
            case 3: return FullScreenMode.MaximizedWindow;
            default: return FullScreenMode.FullScreenWindow;
        }
    }

    int GetResolutionValue(Resolution target)
    {
        for(int i = 0; i < resolutions.Length; i++)
        {
            if(resolutions[i].width == target.width &&
            resolutions[i].height == target.height && 
            resolutions[i].refreshRateRatio.value == target.refreshRateRatio.value)
            {
                return i;
            }
        }
        return resolutions.Length - 1;
    }

    void UpdateDisplay(bool window)
    {
        string input;

        if (window)
        {
            input = slider.value switch
            {
                1 => "Fullscreen",
                2 => "Windowed",
                3 => "Windowed Borderless",
                _ => "Fullscreen"
            };
        }
        else
        {
            Resolution res = pendingResolution;
            input = $"{res.width} x {res.height}";
        }

        text.input = input;
        text.Refresh();
    }
}
