using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VCA : MonoBehaviour
{

    private FMOD.Studio.VCA vca;

    public string vcaName;
    public float initValue;

    private Slider slider;

    public bool debug = false;

    void Awake()
    {
        vca = FMODUnity.RuntimeManager.GetVCA("vca:/" + vcaName);
        slider = GetComponent<Slider>();
    }

    public void SetVolume(float value)
    {
        vca.setVolume(value);
    }



}
