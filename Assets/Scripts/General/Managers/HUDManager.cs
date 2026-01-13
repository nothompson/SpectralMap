using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance; 
    public GameObject ReloadSprite;
    [SerializeField] private AnimationCurve PulseCurve;
    [SerializeField] private AnimationCurve RotationCurve;
    [SerializeField] private float MaxRotation;
    public bool reloading = false;

    private RectTransform rect;
    private Vector3 scale;
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
    }

    public void StartReload()
    {
        ReloadSprite.SetActive(true);
    }

    public void StopReload()
    {
        ReloadSprite.SetActive(false);
    }

    public IEnumerator Pulse()
    {
        float t = 0f;
        float dur = AudioManager.Instance.BeatWindowSize;

        rect = ReloadSprite.GetComponent<RectTransform>();

        scale = rect.localScale;
        rot = rect.localEulerAngles;
        float rand = Random.Range(0f,1f);
        bool which = rand > 0.5f;
        float targetRotation = which ? -1f * MaxRotation * Random.Range(0.75f,1.25f) : MaxRotation * Random.Range(0.75f,1.25f);

        while(t < dur)
        {
            t += Time.deltaTime;
            float elapsed = t / dur;
            float pulse = PulseCurve.Evaluate(elapsed);
            float rotation = RotationCurve.Evaluate(elapsed);
            rect.localScale = scale * pulse;
            rect.localEulerAngles = new Vector3(0f,0f, rot.z + (targetRotation * rotation));
            yield return null;
        }
        rect.localScale = scale;
        rect.localEulerAngles = rot;

    }

    public void OnDisable()
    {
        rect.localScale = scale;
        rect.localEulerAngles = rot;
        StopAllCoroutines();
        
    }
    

}
