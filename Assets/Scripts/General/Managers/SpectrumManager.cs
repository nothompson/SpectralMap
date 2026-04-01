using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class SpectrumManager : MonoBehaviour
{
    public static SpectrumManager Instance;
    public int PollutantLevel;

    public int MaxPollutantLevel = 100;
    public int NPCKill = 5;

    public GameObject container;
    // private Image image;
    private SpriteAnimate Animation;
    public AnimationCurve transitionCurve;

    public AnimationCurve RotationCurve;
    public float rotation;

    public SpectrumUI sprite;

    private bool SpectrumActive;

    public float transitionDur;

    private bool outroPlaying = false;

    Coroutine TransitionRoutine;
    Coroutine ShakeRoutine;
    Coroutine CooldownRoutine;

    private Vector3 rot;

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

        // image = container.GetComponent<Image>();

    }

    public void OnSaveChange()
    {
        PollutantLevel = 0;
        LoadSpectrum();
        InitUI();
    }

    public void InitUI()
    {
        GameObject go = sprite.gameObject;

        RectTransform rt = go.GetComponent<RectTransform>();

        SpriteAnimate sa = go.GetComponent<SpriteAnimate>();

        Image im = go.GetComponent<Image>();

        Vector3 pl = rt.anchoredPosition;

        sprite.rect = rt;
        sprite.image = im;
        sprite.placement = pl;
        sprite.spriteAnimate = sa;
        sprite.spriteAnimate.image = im;
        sprite.sprites = sa.sprites;

        rot = sprite.rect.localEulerAngles;
        
        StartCoroutine(DelayInit());
    }

    IEnumerator DelayInit()
    {
        yield return null;
        sprite.SyncToCurrentLevel();
    }

    public void PolluteSpectrum(int x)
    {
        PollutantLevel += x;

        if(PollutantLevel >= MaxPollutantLevel)
        {
            PollutantLevel = MaxPollutantLevel;
        }

        EventManager.Instance.OnPollute(PollutantLevel);

        SaveSpectrum();

        if(SpectrumActive && !outroPlaying){
            StartShake();
            StartCooldown();
            return;
        }

        StartTransition();
    }

    public void PurifySpectrum(int x)
    {
        PollutantLevel -= x;
        if(PollutantLevel <= 0)
        {
            PollutantLevel = 0;
        }
        EventManager.Instance.OnPurify(PollutantLevel);
        SaveSpectrum();

                if(SpectrumActive && !outroPlaying){
            StartShake();
            StartCooldown();
            return;
        }

        StartTransition();
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

    public void StartTransition(bool intro = true)
    {
        if(TransitionRoutine != null)
        {
            StopCoroutine(TransitionRoutine);
        }
        TransitionRoutine = StartCoroutine(Transition(intro));
    }

    public void StartShake()
    {
        if(ShakeRoutine != null)
        {
            StopCoroutine(ShakeRoutine);
        }

        ShakeRoutine = StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float t = 0f;

        float dur = 0.4f;

        bool rand = Random.Range(0f, 1f) > 0.5f;

        float targetRotation = rand ? -1f * rotation * Random.Range(0.75f, 1.25f) : rotation * Random.Range(0.75f, 1.25f);

        RectTransform rect = container.GetComponent<RectTransform>();

        while(t < dur)
        {    
            t += Time.unscaledDeltaTime;
            float elapsed = t / dur;
            float rotValue = RotationCurve.Evaluate(elapsed);
            rect.localEulerAngles = new Vector3(0f, 0f, rot.z + (targetRotation * rotValue));
            yield return null;
        }

    }

    IEnumerator Transition(bool intro)
    {
        sprite.waiting = true;
        float t = 0f;
        Vector3 target = intro ? Vector3.one : Vector3.zero;
        RectTransform rect = container.GetComponent<RectTransform>();
        Vector3 starting;

        if (intro)
        {
            container.SetActive(true);
            SpectrumActive = true;
            rect.localScale = Vector3.zero;
            starting = Vector3.one;
        }
        else
        {
            starting = rect.localScale;
        }
        
        while(t < transitionDur)
        {
            t += Time.deltaTime;
            float time = t / transitionDur;
            float elapsed = intro ? time : 1f - time;
            if(elapsed <= 0.8f && !intro)
            {
                outroPlaying = true;
            }
            float value = transitionCurve.Evaluate(elapsed);

            rect.localScale = starting * value;
            yield return null;
        }
        rect.localScale = target;

        if (!intro)
        {
            container.SetActive(false);
            SpectrumActive = false;
            sprite.waiting = false;
        }
        else
        {
            sprite.OnShow();
            StartCooldown();
        }
        outroPlaying = false;
    }

    void StartCooldown()
    {
        if(CooldownRoutine != null)
        {
            StopCoroutine(CooldownRoutine);
        }
        CooldownRoutine = StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(4f);
        StartTransition(false);
    }

    string GetSavePath()
    {
        return SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Spectrum.json");
    }

}
