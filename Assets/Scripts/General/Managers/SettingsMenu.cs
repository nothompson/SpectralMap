using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance;
    public GameObject Container;
    public GameObject SubContainer;
    public GameObject master; 

    public SpriteAnimate configSprite;

    [SerializeField] private AnimationCurve transitionCurve;

    public bool animating = false;

    Coroutine transitionRoutine;

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
        if(animating) return;
        
        if((JournalManager.Instance != null && JournalManager.Instance.animating) || (InventoryManager.Instance != null && InventoryManager.Instance.animating)) return;

        SubContainer.SetActive(false);

        Container.SetActive(true);

        configSprite.index = configSprite.sprites.Length - 1;

        StartTransition(true);

        StartCoroutine(configSprite.AnimateToTarget(0, null, () =>
        {
            SubContainer.SetActive(true);
            animating = false;
        }));
        animating = true;
        AudioManager.Instance.StartConfigHover();
    }

    public void Close()
    {
        if(animating) return;

        configSprite.index = 0;

        StartTransition(false);

        SubContainer.SetActive(false);
        StartCoroutine(configSprite.AnimateToTarget(configSprite.sprites.Length - 1, null, () =>
        {
            Container.SetActive(false);
            animating = false;
        }));
        animating = true;
    }

    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
    }

    public void StartTransition(bool intro)
    {
        if(transitionRoutine != null) {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
            }
        transitionRoutine = StartCoroutine(Transition(intro));
    }

    IEnumerator Transition(bool intro)
    {
        float t = 0f;
        float dur = intro ? 0.25f : 0.75f;

        Vector3 target = intro ? Vector3.one : Vector3.zero;
        RectTransform rect = Container.GetComponent<RectTransform>();
        Vector3 starting;

        if (intro)
        {
            rect.localScale = Vector3.zero;
            starting = Vector3.one;
        }
        else
        {
            starting = rect.localScale;
        }
        while(t < dur)
        {
            t += Time.unscaledDeltaTime;
            float time = Mathf.Clamp01(t / dur);
            float elapsed = intro ? time : 1f - time;
            float value = transitionCurve.Evaluate(elapsed);

            rect.localScale = starting * value;
            yield return null;
        }
        rect.localScale = target;
        transitionRoutine = null;
    }
    
}
