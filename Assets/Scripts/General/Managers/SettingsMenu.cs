using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance;
    public GameObject canvas;
    public GameObject master; 
    private Slider masterSlider;
    public Slider sensitivitySlider; 

    public float mouseSensitivity; 


    private FMOD.Studio.VCA vca;

    

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
    }

    void Start()
    {
        masterSlider = master.GetComponent<Slider>();

        SetSensitivity(sensitivitySlider.value);

        vca = FMODUnity.RuntimeManager.GetVCA("vca:/Master");

        vca.setVolume(masterSlider.value);
    }

    public void Open()
    {
        canvas.SetActive(true);
    }

    public void Close()
    {
        canvas.SetActive(false);
    }

    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
    }
    
}
