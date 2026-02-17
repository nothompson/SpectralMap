using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class SpectrumManager : MonoBehaviour
{
    public static SpectrumManager Instance;
    public int PollutantLevel;

    public Image image;
    public SpriteAnimate Animation;
    public AnimationCurve transitionCurve;

    private bool SpectrumActive;

    

    Coroutine Transition;
    Coroutine Change;

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
        SaveSpectrum();
    }

    void Start()
    {
        LoadSpectrum();
    }

    public void StartTransition()
    {
        if(Transition != null)
        {
            StopCoroutine(Transition);
            Transition = null;
        }

        // StartCoroutine();
    }



    public void PolluteSpectrum(int x)
    {
        PollutantLevel += x;
        SaveSpectrum();
    }

    public void PurifySpectrum(int x)
    {
        PollutantLevel -= x;
        if(PollutantLevel <= 0)
        {
            PollutantLevel = 0;
        }
        SaveSpectrum();
    }

    public void LoadSpectrum()
    {
        if(!File.Exists(GetSavePath())) return;

        string json = File.ReadAllText(GetSavePath());
        SpectrumSaveData data = JsonUtility.FromJson<SpectrumSaveData>(json);
        
        PollutantLevel = data.PollutantLevels;
    }

    public void SaveSpectrum()
    {
        SpectrumSaveData data = new SpectrumSaveData();

        data.PollutantLevels = PollutantLevel;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
    }

    string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "Spectrum.json");
    }

}
