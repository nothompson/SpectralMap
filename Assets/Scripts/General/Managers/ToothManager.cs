using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class ToothManager : MonoBehaviour
{
    public static ToothManager Instance;

    public int ToothCount;

    public SpriteText text;

    public GameObject Container;

    public float transitionDur;
    private RectTransform rect;

    private Vector2 BasePosition;

    public AnimationCurve TransitionCurve;

    public bool ToothActive;

    private List<string> CollectedIDs = new List<string>();

    Coroutine TransitionRoutine;

    Coroutine CountdownRoutine;

    public FMODUnity.EventReference pickup;

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

        rect = Container.GetComponent<RectTransform>();

        BasePosition = rect.anchoredPosition;
    }

    public void AddTooth(ToothObject data)
    {
        if(CollectedIDs.Contains(data.ID)) return;


        data.Added = true;

         FMODUnity.RuntimeManager.PlayOneShot(pickup);

        ToothCount += 1;

        CollectedIDs.Add(data.ID);
        SaveToothCount();
        StartTransition(true);
    }

    public void StartTransition(bool intro = true, bool count = true)
    {
        if(TransitionRoutine != null)
        {
            StopCoroutine(TransitionRoutine);
        }
        TransitionRoutine = StartCoroutine(Transition(intro, count));
    }

    IEnumerator Transition(bool intro, bool count = true)
    {
        float t = 0f;
        Vector2 offscreen = new Vector2(200f, BasePosition.y);
        Vector2 start = intro ? offscreen : BasePosition;
        Vector2 target = intro ? BasePosition : offscreen;
        if (intro)
        {
            Container.SetActive(true);
            ToothActive = true;
            rect.anchoredPosition = offscreen;
        }

        while(t < transitionDur)
        {
            t += count ? Time.deltaTime : Time.unscaledDeltaTime;
            float time = t / transitionDur;
            float elapsed = intro ? time : 1f - time;

            float value = TransitionCurve.Evaluate(elapsed);

            rect.anchoredPosition = Vector2.LerpUnclamped(offscreen, BasePosition, value);

            yield return null;
        }

        rect.anchoredPosition = target;


        if (intro)
        {
            if(count) StartCountdown();
        }
        else
        {
            Container.SetActive(false);
            ToothActive = false;
        }

        TransitionRoutine = null;

    }

    public void StartCountdown()
    {
        if(CountdownRoutine != null)
        {
            StopCoroutine(CountdownRoutine);
        }
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(5f);
        StartTransition(false);
    }


    public void OnSaveChange()
    {
        ToothCount = 0;
        CollectedIDs.Clear();
        LoadAllTeeth();
        InitUI();
    }

    public void InitUI()
    {
        Container.SetActive(false);

    }

    public void Update()
    {
        text.input = ToothCount.ToString();
        text.Refresh();
    }

    public void SaveToothCount()
    {
        ToothSaveData data = new ToothSaveData
        {
            ToothCount = ToothCount,
            CollectedIDs = new List<string>(CollectedIDs)
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
    }

    public void LoadToothCount()
    {
        if(!File.Exists(GetSavePath())) return;

        string json = File.ReadAllText(GetSavePath());
        
        ToothSaveData data = JsonUtility.FromJson<ToothSaveData>(json);

        ToothCount = data.ToothCount;
        
        CollectedIDs = data.CollectedIDs ?? new List<string>();
    }

    public void LoadAllTeeth()
    {
        ToothObject[] allTeeth = Resources.LoadAll<ToothObject>("ToothPickups");

        foreach(ToothObject tooth in allTeeth)
        {
            tooth.Added = false;
        }

        LoadToothCount();

        foreach(ToothObject tooth in allTeeth)
        {
            if (CollectedIDs.Contains(tooth.ID))
            {
                tooth.Added = true;
            }
        }
    }

    public bool IsCollected(string id) => CollectedIDs.Contains(id);

    string GetSavePath()
    {
        return SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Tooth.json");
    }
}