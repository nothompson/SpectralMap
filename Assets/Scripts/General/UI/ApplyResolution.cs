    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using TMPro;    

public class ApplyResolution : MonoBehaviour
{
    void Awake()
    {
        gameObject.SetActive(false);
    }
    public void Apply()
    {
        var res = ResolutionOptions.pendingResolution;
        var win = ResolutionOptions.pendingWindow;
        Screen.SetResolution(res.width,res.height,win, res.refreshRateRatio);

        if(win == FullScreenMode.Windowed)
        {
            res.width = Screen.width;
            res.height = Screen.height;
        }

        SettingsMenu.SettingsData.resolution = res;

        SettingsMenu.SettingsData.windowType = win;

        StartCoroutine(Deactivate());
    }

    public IEnumerator Deactivate()
    {
        yield return null;
        GameSettings.Save(SettingsMenu.SettingsData);
        gameObject.SetActive(false);
    }

    public void MakeActive()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
    }

}
