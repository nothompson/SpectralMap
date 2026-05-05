using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Containers")]
    public static SettingsMenu Instance;
    public GameObject Container;
    public GameObject SubContainer;

    [Header("Visuals")]
    [SerializeField] private AnimationCurve transitionCurve;
    public SpriteAnimate configSprite;

    public bool animating = false;
    Coroutine transitionRoutine;
    [Header("Audio")]
    public Slider masterVolume; 
    public Slider musicVolume; 
    public Slider soundsVolume; 
    private FMOD.Studio.VCA vca;

    [Header("Gameplay")]
    public Slider sensitivitySlider; 
    public float mouseSensitivity;
    public Slider fovSlider; 

    [Header("Video")]
    public Slider Window;
    public Slider Resolution;
    public ApplyResolution apply;

    [Header("Crosshair")]
    public Slider crosshairIndex;
    public Slider crosshairRed;
    public Slider crosshairGreen;
    public Slider crosshairBlue;
    public Slider crosshairAlpha;
    public Slider crosshairScale;
    public Slider crosshairRotation;

    public static GameSettingsData SettingsData;

    [Header("References")]
    public TriggerRaycasts CloseButton;


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

        SettingsData = GameSettings.Load();

        sensitivitySlider.value = SettingsData.sensitivity;
        mouseSensitivity = SettingsData.sensitivity;

        var master = FMODUnity.RuntimeManager.GetVCA("vca:/Master");

        master.setVolume(SettingsData.masterVolume);
        masterVolume.value = SettingsData.masterVolume;

        var music = FMODUnity.RuntimeManager.GetVCA("vca:/Music");

        music.setVolume(SettingsData.musicVolume);
        musicVolume.value = SettingsData.musicVolume;

        var sounds = FMODUnity.RuntimeManager.GetVCA("vca:/Sounds");

        sounds.setVolume(SettingsData.soundsVolume);
        soundsVolume.value = SettingsData.soundsVolume;

        FOVSettings.CurrentFOV = SettingsData.fov;

    }

    public void Open()
    {
        if(animating) return;
        
        if((JournalManager.Instance != null && JournalManager.Instance.animating) || (InventoryManager.Instance != null && InventoryManager.Instance.animating)) return;

        PauseManager.Instance.TriggerRaycasts(false);

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

        PauseManager.Instance.TriggerRaycasts(true);

        configSprite.index = 0;

        StartTransition(false);

        SubContainer.SetActive(false);
        StartCoroutine(configSprite.AnimateToTarget(configSprite.sprites.Length - 1, null, () =>
        {
            Container.SetActive(false);
            animating = false;
        }));
        animating = true;

        StartCoroutine(apply.Deactivate());
    }

    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
            SettingsData.sensitivity = value;
            GameSettings.Save(SettingsData);
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
