using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject ScreenShotCanvas;

    public GameObject PauseMenu;

    public GameObject ExitOptions;

    public RawImage image;

    public Texture2D texture;
    public GameObject[] pauseObjects;

    public AnimationCurve introAnimation;
    public AnimationCurve outroAnimation;
    private SpriteAnimate[] pauseSprites;

    private float flowTime; 

    public bool paused = false;

    private float targetAlpha = 1f;
    private float targetStrength = 0.35f;

    private float alphaDelay = 0.5f;
    private float strengthDelay = 2f;
    private float closeDelay = 0.25f;

    private float hudIntroDelay = 0.35f;

    private float hudIntroIncrement = 0.05f;

    private float hudOutroDelay = 0.25f;

    private float hudOutroIncrement = 0.025f;

    public Material pauseMaterial;

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

        pauseSprites = PauseMenu.GetComponentsInChildren<SpriteAnimate>();
    }

    void Update()
    {
        if (paused)
        {
            flowTime += Time.unscaledDeltaTime * 0.5f;
            pauseMaterial.SetFloat("_UnscaledTime",flowTime);
        }
    }

    public void Pause()
    {
        flowTime = 0f;
        paused = true;
        Time.timeScale = 0f;
        StartCoroutine(ScreenShot());
        StartCoroutine(HudIntro());
    }

    public void Unpause()
    {
        StartCoroutine(Close());
    }

    public IEnumerator ScreenShot()
    {
        
        yield return new WaitForEndOfFrame();
        texture = ScreenCapture.CaptureScreenshotAsTexture();

        float randomOffset = Random.Range(-0.25f,0.25f);

        pauseMaterial.SetFloat("_RandomOffset", randomOffset);

        // image.texture = texture;
        
        pauseMaterial.SetTexture("_Texture2D", texture);
        pauseMaterial.SetTexture("_MainTex", texture);

        // image.material = null;

        image.material = pauseMaterial;

        ScreenShotCanvas.SetActive(true);

        StartCoroutine(AlphaRamp());
        StartCoroutine(StrengthUpRamp());

    }

    public IEnumerator AlphaRamp()
    {
        float t = 0f;
            while(t < alphaDelay)
            {
                t += Time.unscaledDeltaTime;

                float alpha = (t / alphaDelay) * targetAlpha;

                pauseMaterial.SetFloat("_Alpha", alpha);

                yield return null;
            }
    }

    public IEnumerator StrengthUpRamp()
    {
        float t = 0f;
        while(t < strengthDelay)
        {
            t += Time.unscaledDeltaTime;

            float strength = (t / strengthDelay) * targetStrength;

            pauseMaterial.SetFloat("_Strength", strength);

            yield return null;
        }
    }

    public IEnumerator HudIntro()
    {
        var rects = new RectTransform[pauseObjects.Length];
        var baseScales = new Vector3[pauseObjects.Length];
        var offsets = new float[pauseObjects.Length];

        float dur = hudIntroDelay + (hudIntroIncrement * (pauseObjects.Length -1));

        for(int i = 0; i < pauseObjects.Length; i++)
        {
            if(pauseObjects[i] == null) continue;

            rects[i] = pauseObjects[i].GetComponent<RectTransform>();
            baseScales[i] = new Vector3(1f,1f,1f);
            rects[i].localScale = Vector3.zero;

            offsets[i] = i * hudIntroIncrement;

        }

        PauseMenu.SetActive(true);

        foreach(var i in pauseSprites)
        {
            if(i._current != null) i.StopCoroutine(i._current);
            i.SetFrame(0);
        }

        float t = 0f;

        while(t < dur)
        {
            t += Time.unscaledDeltaTime;
            for(int i = 0; i < rects.Length; i++)
            {
                float time = (t - offsets[i]) / hudIntroDelay;

                time = Mathf.Clamp01(time);

                float value = introAnimation.Evaluate(time);

                rects[i].localScale = baseScales[i] * value;
            }
            yield return null;
        }
    }

    public IEnumerator Close()
    {
        StartCoroutine(CloseBackground());

        if(SettingsMenu.Instance != null)
        {
            SettingsMenu.Instance.canvas.SetActive(false);
        }
        if(JournalManager.Instance != null)
        {
            JournalManager.Instance.canvas.SetActive(false);
        }

        ExitOptions.SetActive(false);

        var rects = new RectTransform[pauseObjects.Length];
        var baseScales = new Vector3[pauseObjects.Length];
        var offsets = new float[pauseObjects.Length];

        float dur = hudOutroDelay + (hudOutroIncrement * (pauseObjects.Length -1));

        for(int i = pauseObjects.Length - 1; i >= 0; i--)
        {
            if(pauseObjects[i] == null) continue;

            rects[i] = pauseObjects[i].GetComponent<RectTransform>();
            baseScales[i] = new Vector3(1f,1f,1f);

            offsets[i] = (pauseObjects.Length - 1 - i) * hudOutroIncrement;

        }

        float t = 0f;

        while(t < dur)
        {
            t += Time.unscaledDeltaTime;
            for(int i = 0; i < rects.Length; i++)
            {
                float time = (t - offsets[i]) / hudOutroDelay;

                time = Mathf.Clamp01(time);

                float value = outroAnimation.Evaluate(time);

                rects[i].localScale = baseScales[i] * value;
            }
            yield return null;
        }

        
        paused = false;
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;

        CleanUp();

        yield return null;
    }

    public IEnumerator CloseBackground()
    {
        float t = closeDelay;

        while(t > 0)
            {
                t -= Time.unscaledDeltaTime;

                float ramptoZero = (t / closeDelay);

                pauseMaterial.SetFloat("_Alpha", ramptoZero * targetAlpha);
                pauseMaterial.SetFloat("_Strength", ramptoZero * targetStrength);

                yield return null;
            }
    }

    public void CleanUp()
    {
        ScreenShotCanvas.SetActive(false);
        Object.Destroy(texture);
    }


}
