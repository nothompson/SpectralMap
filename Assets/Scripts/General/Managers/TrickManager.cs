using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrickManager : MonoBehaviour
{
    public static TrickManager Instance;

    public Dictionary<string, int> TrickDictionary = new Dictionary<string, int>();

    public Canvas trickCanvas;

    public TMP_Text trickText;

    public TMP_Text scoreText;

    public int Score = 0;

    public float ResetTime = 1f;

    private SpriteText TrickText;
    private SpriteText ScoreText;

    public AnimationCurve addTrickAnimation;
    public AnimationCurve completeComboAnimation; 

    private Vector3 trickTextInitSize;
    private Vector3 scoreTextInitSize;

    private float trickAnimationDur = 0.33f;

    private float scoreAnimationDur = 0.66f;

    Coroutine trickAnimation;

    Coroutine scoreAnimation;

    private int TrickCount = 0;

    public bool completed = false;
    public bool completeCombo = false;
    Coroutine ComboTimer;
    public float comboTimerDur;
    public bool comboTimerActive = false;
    public float comboTimerElapsed = 0f;


    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitTrickDictionary();
        }
        else
        {
            Destroy(gameObject);
        }

        trickTextInitSize = trickText.rectTransform.localScale;

        scoreTextInitSize = scoreText.rectTransform.localScale;

        TrickText = trickText.GetComponent<SpriteText>();

        ScoreText = scoreText.GetComponent<SpriteText>();

    }

    void InitTrickDictionary()
    {
        TrickDictionary["BHop"] = 100;
        TrickDictionary["RocketJump"] = 50;
        TrickDictionary["Pogo"] = 100;
        TrickDictionary["Wall"] = 75;
        TrickDictionary["Sync"] = 250;
        TrickDictionary["Tri-Sync"] = 500;
        TrickDictionary["Quad-Sync"] = 1000;
        TrickDictionary["Mega-Sync"] = 5000;

        TrickDictionary["Bomb"] = 200;
        TrickDictionary["Airshot"] = 250;
        TrickDictionary["Direct"] = 150;
        TrickDictionary["Air-Airshot"] = 350;
        TrickDictionary["Kill"] = 500;
    }

    public void AddTrick(string trick)
    {
        if (completed)
        {
            ResetCombo();
            TrickCount = 0;
            completed = false;
        }

        if(ComboTimer != null)
        {
            StopCoroutine(ComboTimer);
            ComboTimer = null;
            comboTimerActive = false;
        }

        TrickCount++;
        if(TrickDictionary.TryGetValue(trick, out int value))
        {
            Score += value;
        }

        if (string.IsNullOrEmpty(TrickText.input))
        {
            TrickText.input = trick;
        }
        else
        {
            TrickText.input += " + " + trick;
        }

        ScoreText.input = Score.ToString("#,##0") + " x " + TrickCount;
        TrickText.Refresh();
        ScoreText.Refresh();

        if(trickAnimation != null)
        {
            StopCoroutine(trickAnimation);
            trickText.rectTransform.localScale = trickTextInitSize;
        }
        trickAnimation = StartCoroutine(AnimateText(trickText.rectTransform, trickTextInitSize, trickAnimationDur, addTrickAnimation));
    }

    IEnumerator AnimateText(RectTransform rect, Vector3 initSize, float duration, AnimationCurve curve)
    {
        
        float t = 0f;

        while(t < duration)
        {
            t += Time.deltaTime;

            float time = Mathf.Clamp01(t / duration);

            float scale = curve.Evaluate(time);

            rect.localScale = initSize * scale;

            yield return null;
        }
    }

    public IEnumerator CompleteCombo()
    {
        completed = true;

        if(ComboTimer != null)
        {
            StopCoroutine(ComboTimer);
            ComboTimer = null;
            comboTimerActive = false;
        }

        if(Score == 0)
        {
            ScoreText.input = string.Empty;
            ScoreText.Refresh();
            yield break;
        }
        
        ScoreText.input = (Score * TrickCount).ToString("#,##0");
        ScoreText.Refresh();

        if(scoreAnimation != null)
        {
            StopCoroutine(scoreAnimation);
            scoreText.rectTransform.localScale = scoreTextInitSize;
        }
        scoreAnimation = StartCoroutine(AnimateText(scoreText.rectTransform, scoreTextInitSize, scoreAnimationDur, completeComboAnimation));

        yield return new WaitForSeconds(scoreAnimationDur + ResetTime);
        
        if(completed){
            ResetCombo();
            TrickCount = 0; 
        }
    }

    public void StartComboTimer()
    {
        if(Score > 0 && !completed && !comboTimerActive)
        {
            ComboTimer = StartCoroutine(CompleteComboTimer());
        }
    }

    public IEnumerator CompleteComboTimer()
    {
        comboTimerActive = true;
        comboTimerElapsed = 0f;

        while(comboTimerElapsed < comboTimerDur)
        {
            if (!PauseManager.Instance.paused)
            {
                comboTimerElapsed += Time.deltaTime;
            }
            yield return null;
        }

        if (comboTimerActive)
        {
            StartCoroutine(CompleteCombo());
        }

        comboTimerActive = false;
        ComboTimer = null;
        comboTimerElapsed = 0f;
    }

    public void ResetCombo()
    {
        TrickText.input = string.Empty;
        TrickText.Refresh();
        ScoreText.input = string.Empty;
        ScoreText.Refresh();
        Score = 0;
    }

    public void BHop()
    {
        AddTrick("BHop");
    }
    public void RocketJump()
    {
        AddTrick("RocketJump");
    }
    public void Pogo()
    {
        AddTrick("Pogo");
    }
    public void Wall()
    {
        AddTrick("Wall");
    }

    public void Sync(int syncs)
    {
        if(syncs == 2)
        {
            AddTrick("Sync");
        }
        if(syncs == 3)
        {
            AddTrick("Tri-Sync");
        }
        if(syncs == 4)
        {
            AddTrick("Quad-Sync");
        }
        if(syncs > 4)
        {
            AddTrick("Mega-Sync");
        }
    }

}
