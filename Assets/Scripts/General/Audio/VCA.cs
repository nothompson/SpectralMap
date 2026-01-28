using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VCA : MonoBehaviour
{

    public FMOD.Studio.VCA vca;

    public string vcaName;
    private Slider slider;

    public bool debug = false;

    void Start()
    {
        vca = FMODUnity.RuntimeManager.GetVCA("vca:/" + vcaName);
        slider = GetComponent<Slider>();
    }

    public void SetVolume(float value)
    {
        vca.setVolume(value);

        if(SettingsMenu.SettingsData != null)
        {
            if(vcaName == "Master")
            {
                SettingsMenu.SettingsData.masterVolume = value;
            }
            else if(vcaName == "Music")
            {
                SettingsMenu.SettingsData.musicVolume = value;
            }
            else if(vcaName == "Sounds")
            {
                SettingsMenu.SettingsData.soundsVolume = value;
            }

            GameSettings.Save(SettingsMenu.SettingsData);
        }
    }



}
