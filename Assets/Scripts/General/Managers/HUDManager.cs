using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance; 
    public GameObject ReloadSprite;
    public GameObject Container;
    public GameObject Player;
    [SerializeField] private AnimationCurve PulseCurve;
    [SerializeField] private AnimationCurve RotationCurve;
    [SerializeField] private AnimationCurve TransitionCurve;
    [SerializeField] private float MaxRotation;
     [SerializeField] float targetMelt = 0.2f;

     [SerializeField] float baseMelt = 0.05f;

     [SerializeField] float successDur = 0.1f;

     [SerializeField] float failDur = 0.1f;

    [SerializeField] Color successColor;

    [SerializeField] Color failColor;
    [SerializeField] Color baseColor;

    public bool reloading = false;

    private RectTransform rect;
    private RectTransform containerRect;
    private Image image;
    private Material mat;
    private Vector3 scale;
    private Vector3 rot;
    private float bps;
    Coroutine pulseRoutine;
    Coroutine successRoutine;
    Coroutine failRoutine;
    Coroutine transitionRoutine;
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
        image = ReloadSprite.GetComponent<Image>();
        mat = image.material;

        rect = ReloadSprite.GetComponent<RectTransform>();
        containerRect = Container.GetComponent<RectTransform>();
        scale = rect.localScale;
        rot = rect.localEulerAngles;

    }

    public void StartReload()
    {
        Container.SetActive(true);
        float beat = AudioManager.Instance.BeatDur;
        bps = 1f/beat;

        StopScaling();

        transitionRoutine = StartCoroutine(Transition(true));
        
    }

    public void StopReload()
    {
        StopScaling();
        transitionRoutine = StartCoroutine(Transition(false));
    }

    public IEnumerator Transition(bool intro)
    {
        float t = 0f;
        float dur = AudioManager.Instance.BeatWindowSize * 0.5f;
        float value = 0f;
        Vector3 start = containerRect.localScale;
        Vector3 target = intro? Vector3.one : Vector3.zero;

        while(t < dur)
        {
            t += Time.deltaTime;
            
            float time = t / dur;
            
            value = TransitionCurve.Evaluate(time);

            containerRect.localScale = Vector3.Lerp(start, target, value);

            yield return null;
        }

        containerRect.localScale = target;

        if(!intro) Container.SetActive(false);

        transitionRoutine = null;
    }

    public IEnumerator Pulse()
    {
        float t = 0f;
        float dur = AudioManager.Instance.BeatWindowSize;

        Vector3 startScale = scale;
        Vector3 startRot = rot;
        float rand = Random.Range(0f,1f);

        Vector2 randCoord = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        bool which = rand > 0.5f;
        float targetRotation = which ? -1f * MaxRotation * Random.Range(0.75f,1.25f) : MaxRotation * Random.Range(0.75f,1.25f);

        while(t < dur)
        {
            t += Time.deltaTime;    
            float elapsed = t / dur;
            float pulse = PulseCurve.Evaluate(elapsed);
            float rotation = RotationCurve.Evaluate(elapsed);

            rect.localScale = startScale * pulse;

            rect.localEulerAngles = new Vector3(0f,0f, startRot.z + (targetRotation * rotation));

            mat.SetFloat("_distortion", baseMelt + (targetMelt * rotation));
            mat.SetFloat("_bps", bps);
            mat.SetVector("_RandCoord", randCoord);

            yield return null;
        }
        Reset();

    }

    public void StartSuccess()
    {
        if(successRoutine != null)
        {
            StopCoroutine(successRoutine);
        }
        if(failRoutine != null)
        {
            StopCoroutine(failRoutine); 
        }
        successRoutine = StartCoroutine(Success());
    }

  public void StartFailure()
    {
        if(successRoutine != null)
        {
            StopCoroutine(successRoutine);
        }
        if(failRoutine != null)
        {
            StopCoroutine(failRoutine);
        }
        failRoutine = StartCoroutine(Failure());
    }

    IEnumerator Success()
    {
      float t = 0f;

        while(t < failDur * 0.5f)
        {
            t += Time.deltaTime;
            float time = t / failDur;
            Color target = Color.Lerp(baseColor, successColor, time);
            mat.SetColor("_Color", target);
            yield return null;
        }
        yield return new WaitForSeconds(0.05f);

        while(t < failDur * 0.5f)
        {
            t += Time.deltaTime;
            float time = t / failDur;
            Color target = Color.Lerp(successColor, baseColor, time);
            mat.SetColor("_Color", target);
            yield return null;
        }
        mat.SetColor("_Color", baseColor);
    }

    IEnumerator Failure()
    {
        float t = 0f;

        while(t < failDur * 0.5f)
        {
            t += Time.deltaTime;
            float time = t / failDur;
            Color target = Color.Lerp(baseColor, failColor, time);
            mat.SetColor("_Color", target);
            yield return null;
        }
        yield return new WaitForSeconds(0.05f);

        while(t < failDur * 0.5f)
        {
            t += Time.deltaTime;
            float time = t / failDur;
            Color target = Color.Lerp(failColor, baseColor, time);
            mat.SetColor("_Color", target);
            yield return null;
        }
        mat.SetColor("_Color", baseColor);
    }

    public void OnDisable()
    {
        if(pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }
        if(rect != null) Reset();
    }

    public void Reset()
    {
        rect.localScale = scale;
        rect.localEulerAngles = rot;
        if(mat!=null)
        {
            mat.SetFloat("_distortion", baseMelt);
            mat.SetColor("_Color", baseColor);
        }
    }

    void StopScaling()
    {
        if(pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }
        if(transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }
    }


    

}
