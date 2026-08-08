using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance; 

    // [System.Serializable]
    // public class DeathScene
    // {
    //     public Sprite[] sprites;
    // }
    // public DeathScene[] DeathScenes;

    public SpriteAnimate[] DeathLetters;

    public GameObject BloodCanvas;
    public GameObject DeathCanvas;

    public GameObject RetryCanvas;
    public GameObject TextCanvas;
    public GameObject QuitCanvas;

    public Image Blood;
    public Image Death;

    public GameObject DeathSceneCanvas;
    public GameObject DeathTextCanvas;

    [HideInInspector] public Image DeathSceneImg;

    [HideInInspector] public SpriteAnimate DeathSceneAnimate;

    [SerializeField] private AnimationCurve BloodCurve;

    [SerializeField] private AnimationCurve DeathCurve;


    [SerializeField] private float BloodDur;

    Coroutine overlayRoutine;

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

        Blood = BloodCanvas.GetComponentInChildren<Image>();
        Death = DeathCanvas.GetComponentInChildren<Image>();
        DeathSceneImg = DeathSceneCanvas.GetComponentInChildren<Image>();

        DeathSceneAnimate = DeathSceneCanvas.GetComponentInChildren<SpriteAnimate>();

        ResetAll();
    }

    void ResetAll()
    {
        BloodCanvas.SetActive(false);
        DeathCanvas.SetActive(false);
        DeathSceneCanvas.SetActive(false);
        DeathTextCanvas.SetActive(false);
        RetryCanvas.SetActive(false);
        DeathManager.PlayerDead = false;
    }

    public void PlayDamageOverlay(float dmg)
    {
        if(overlayRoutine != null)
        {
            StopCoroutine(overlayRoutine);
        }
        
        overlayRoutine = StartCoroutine(DamageOverlay(dmg));
    }

    IEnumerator DamageOverlay(float damageOffset)
    {
        float t = 0f;
        Color fade = Blood.color;
        Blood.color = new Color(1f,1f,1f,0f);
        BloodCanvas.SetActive(true);
        float deathOffset = 0f;

        float dmg = damageOffset * 0.1f;

        if (DeathManager.PlayerDead)
        {
            deathOffset = 2f;
            Death.color = new Color(1f,1f,1f,0f);

            // DeathSceneAnimate.sprites = DeathScenes[0].sprites;

            DeathCanvas.SetActive(true);

            DeathSceneCanvas.SetActive(true);

            DeathTextCanvas.SetActive(true);

            StartCoroutine(DeathText());


            DeathSceneImg.color = new Color(1f,1f,1f,0f);

            Cursor.lockState = CursorLockMode.None;
            CursorManager.Instance.TriggerCursor(true);
        }

        float newtime = BloodDur + damageOffset + deathOffset;

        while(t < newtime)
        {
            t += Time.deltaTime;
            float time = Mathf.Clamp01(t / newtime);

            if (DeathManager.PlayerDead)
            {
                fade.a = DeathCurve.Evaluate(time);
                Death.color = fade;

                fade.a = DeathCurve.Evaluate(time);
                DeathSceneImg.color = fade;
            }
            else
            {
                fade.a = BloodCurve.Evaluate(time);  
            }

            Blood.color = fade;
            yield return null;
        }

        if(!DeathManager.PlayerDead){
            BloodCanvas.SetActive(false);
        }
        else
        {
            RetryCanvas.SetActive(true);
        }

    }

    IEnumerator DeathText(System.Action onTarget = null)
    {
        yield return null;

        foreach(var s in DeathLetters)
        {
            s.AnimateToEnd(onTarget);
        }
    }

    public void Respawn()
    {
        StartCoroutine(DeathText(DisableText));
        
        StartCoroutine(ReloadRoutine());
    }

    public void DisableText()
    {
        DeathTextCanvas.SetActive(false);
    }

    IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(1f);
        Reload();
        ResetAll();
    }

    public async void Reload()
    {
        LevelManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
    }


}
