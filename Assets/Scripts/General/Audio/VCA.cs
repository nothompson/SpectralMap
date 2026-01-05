using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VCA : MonoBehaviour
{

    private FMOD.Studio.VCA vca;

    public string vcaName;

    private Slider slider;

    void Start()
    {
        vca = FMODUnity.RuntimeManager.GetVCA("vca:/" + vcaName);
        slider = GetComponent<Slider>();
    }

    public void SetVolume(float value)
    {
        vca.setVolume(value);
    }

}
