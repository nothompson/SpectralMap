using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance;
    public GameObject canvas;
    public Slider sensitivitySlider; 

    public float mouseSensitivity; 

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
        SetSensitivity(sensitivitySlider.value);
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
