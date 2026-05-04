using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ReloadManager : MonoBehaviour
{
    public static ReloadManager Instance; 
    public GameObject ReloadSprite;
    public GameObject Container;
    public GameObject Player;
    private Light ShellLight;
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

    public Animator rightHandAnim;
    public Animator leftHandAnim;

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

    PlayerControlRigid pc;

    private float BaseIntensity;
    [SerializeField] private float TargetIntensity;
    private float BaseRange;
    [SerializeField] private float TargetRange;

    private Color BaseLightColor;
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

    void Update()
    {
        if(ShellLight == null) return;
        ProjectileParticleManager.Instance.ReloadPulse.transform.position = ShellLight.transform.position;
    }

    public void RegisterPlayer(GameObject player)
    {
        Player = player;
        pc = player.GetComponent<PlayerControlRigid>();
        GameObject go = Player.transform.Find("YawPivot/Camera/overlay/SpectralShellLight")?.gameObject;
        ShellLight = go.GetComponent<Light>();

        Launcher launcher = player.GetComponentInChildren<Launcher>();

        if(launcher != null)
        {
            rightHandAnim = launcher.rightHandAnim;
            leftHandAnim = launcher.leftHandAnim;
        }

        Debug.Log(launcher);
        
        BaseIntensity = ShellLight.intensity;
        BaseRange = ShellLight.range;
        BaseLightColor = ShellLight.color;
    }

    public void StartReload()
    {
        rightHandAnim.ResetTrigger("Reload");
        if(!pc.grappling) leftHandAnim.ResetTrigger("Reload");

        Container.SetActive(true);
        ShellLight.gameObject.SetActive(true);
        // ShellLight.intensity = 0f;

        float beat = AudioManager.Instance.BeatDur;
        bps = 1f/beat;

        StopScaling();

        transitionRoutine = StartCoroutine(Transition(true));

         rightHandAnim.SetInteger("HandState", 2);
        rightHandAnim.SetBool("IsReloading", true);
        if(!pc.grappling){
            leftHandAnim.SetInteger("HandState", 2);
            leftHandAnim.SetBool("IsReloading", true);
        }
        
    }

    public void ReloadAttempt()
    {
        rightHandAnim.SetTrigger("Reload");
        if(!pc.grappling){
        leftHandAnim.SetTrigger("Reload");
        }
    }

    public void StopReload(bool animate = false)
    {        
        StopScaling();
        transitionRoutine = StartCoroutine(Transition(false));
        StartCoroutine(Stopping());
        if(animate){
        StartCoroutine(StopReloadSequence());
        }
        else
        {
            rightHandAnim.SetTrigger("Reload");
            if(!pc.grappling) leftHandAnim.SetTrigger("Reload");

            rightHandAnim.SetInteger("HandState", 0);
            rightHandAnim.SetBool("IsReloading", false);
            leftHandAnim.SetBool("IsReloading", false);
            if(!pc.grappling) leftHandAnim.SetInteger("HandState", 0);
        }   
    }

    private IEnumerator StopReloadSequence()
    {
        rightHandAnim.SetTrigger("Reload");
        if(!pc.grappling) leftHandAnim.SetTrigger("Reload");

        yield return new WaitForSeconds(0.25f);

        rightHandAnim.SetInteger("HandState", 0);
        rightHandAnim.SetBool("IsReloading", false);
        leftHandAnim.SetBool("IsReloading", false);
        if(!pc.grappling) leftHandAnim.SetInteger("HandState", 0);
    }

    IEnumerator Stopping()
    {
        yield return null;
        reloading = false;
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

        if(!intro) 
        {
            Container.SetActive(false);
            ShellLight.gameObject.SetActive(false);
        }

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

        AudioManager.Instance.ReloadTick();

        ProjectileParticleManager.Instance.ReloadPulse.Emit(20);
        ProjectileParticleManager.Instance.ReloadPulse.Play();

        while(t < dur)
        {
            t += Time.deltaTime;    
            float elapsed = t / dur;
            float pulse = PulseCurve.Evaluate(elapsed);
            float rotation = RotationCurve.Evaluate(elapsed);

            rect.localScale = startScale * pulse;

            rect.localEulerAngles = new Vector3(0f,0f, startRot.z + (targetRotation * rotation));

            ShellLight.intensity = BaseIntensity  + (TargetIntensity * rotation);

            mat.SetFloat("_distortion", baseMelt + (targetMelt * rotation));
            mat.SetFloat("_bps", bps);
            mat.SetVector("_RandCoord", randCoord);

            yield return null;
        }
        Reset();

    }

    public void StartSuccess()
    {
        AudioManager.Instance.ReloadSuccess();
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
        AudioManager.Instance.ReloadFailure();
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
            Color lightTarget = Color.Lerp(BaseLightColor, successColor, time);
            ShellLight.color = lightTarget;
            yield return null;
        }
        yield return new WaitForSeconds(0.05f);

        while(t < failDur * 0.5f)
        {
            t += Time.deltaTime;
            float time = t / failDur;
            Color target = Color.Lerp(successColor, baseColor, time);
            mat.SetColor("_Color", target);
            Color lightTarget = Color.Lerp(successColor, BaseLightColor, time);
            ShellLight.color = lightTarget;
            yield return null;
        }
        mat.SetColor("_Color", baseColor);
        ShellLight.color = BaseLightColor;
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

            Color lightTarget = Color.Lerp(BaseLightColor, failColor, time);
            ShellLight.color = lightTarget;

            yield return null;
        }
        yield return new WaitForSeconds(0.05f);

        while(t < failDur * 0.5f)
        {
            t += Time.deltaTime;
            float time = t / failDur;
            Color target = Color.Lerp(failColor, baseColor, time);
            mat.SetColor("_Color", target);

            Color lightTarget = Color.Lerp(failColor, BaseLightColor, time);
            ShellLight.color = lightTarget;

            yield return null;
        }
        mat.SetColor("_Color", baseColor);
        ShellLight.color = BaseLightColor;
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
        ShellLight.intensity = BaseIntensity;
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
