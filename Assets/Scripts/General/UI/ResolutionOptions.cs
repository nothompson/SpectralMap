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
            if (Window)
            {
                pendingWindow = SettingsMenu.SettingsData.windowType;
                slider.value = GetWindowValue(pendingWindow);
                UpdateDisplay(Window);
            }
            else
            {
                pendingResolution = SettingsMenu.SettingsData.resolution;
                slider.value = GetResolutionValue(pendingResolution);
                UpdateDisplay(Window);
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
            if (target.width <= 0 || target.height <= 0)
            {
                target.width = Screen.width;
                target.height = Screen.height;
            }
        
            int closest = 0;
            int smallest = int.MaxValue;

            for(int i = 0; i < resolutions.Length; i++)
            {
                int diff = Mathf.Abs(resolutions[i].width - target.width) + Mathf.Abs(resolutions[i].height - target.height);

                if(diff < smallest)
                {
                    smallest = diff;
                    closest = i;
                }

                if(resolutions[i].width == target.width &&
                resolutions[i].height == target.height)
                {
                    return i;
                }
            }
            return closest;
        }

        int GetWindowValue(FullScreenMode mode)
        {
            return mode switch
            {
                FullScreenMode.FullScreenWindow => 1,
                FullScreenMode.Windowed => 2,
                FullScreenMode.MaximizedWindow => 3,
                _ => 0
            };
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
                    3 => "Borderless",
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
