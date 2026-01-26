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
        Screen.SetResolution(res.width,res.height,ResolutionOptions.pendingWindow, res.refreshRateRatio);
        StartCoroutine(Deactivate());
    }

    IEnumerator Deactivate()
    {
        yield return null;
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
