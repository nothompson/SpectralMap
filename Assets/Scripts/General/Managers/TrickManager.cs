using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class TrickManager : MonoBehaviour
{
    public static TrickManager Instance;
    public PlayerControlRigid pc;

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

    private float FallingDur = 0.5f;

    Coroutine trickAnimation;

    Coroutine scoreAnimation;

    private int TrickCount = 0;

    public bool completed = false;
    public bool completeCombo = false;
    Coroutine ComboTimer;
    public float comboTimerDur;
    public bool comboTimerActive = false;
    public float comboTimerElapsed = 0f;

    Coroutine BoredTimer;

    Coroutine BoredomRoutine;
    Coroutine FallingTimerRoutine;

    public float boredDur = 2f;
    public float boredCheckDur;

    public int maxTricks = 15;

    private Trick active = null;
    private bool surfing = false;   
    private bool falling = false;   

    public float pps = 50f;
    private float accumulatedPoints = 0f;
    public float speed = 0f;

    public GameObject Clock;
    public SpriteAnimate ClockAnimation;

    public RectTransform smallHand;
    public RectTransform bigHand;

    Coroutine ClockReset;

    public enum TrickType{
            rocketjump,
            pogo,
            wall,
            surfing,
            freefall,
            surfJump,
            sync,
            triSync,
            quadSync,
            megaSync,
            bomb,
            airshot,
            direct,
            airAirshot,
            kill
        }

    public class Trick
    {
        public TrickType Type;
        public string Display;
        public int Points;
        public bool Continuous;

        public Trick(TrickType type, int syncs = 1)
        {
            Type = type;
            TrickData(type);
            if(syncs > 1) SyncData(syncs);
        }

        private void TrickData(TrickType type)
        {
            switch (type)
            {
                case TrickType.airAirshot:
                    Display = "Air-Airshot";
                    Points = 350;
                    Continuous = false;
                    break;

                case TrickType.airshot:
                    Display = "Airshot";
                    Points = 250;
                    Continuous = false;
                    break;
                case TrickType.bomb:
                    Display = "Bomb";
                    Points = 200;
                    Continuous = false;
                    break;
                case TrickType.direct:
                    Display = "Direct";
                    Points = 100;
                    Continuous = false;
                    break;
                case TrickType.kill:
                    Display = "Kill";
                    Points = 500;
                    Continuous = false;
                    break;

                case TrickType.pogo:
                    Display = "Pogo";
                    Points = 100;
                    Continuous = false;
                    break;

                case TrickType.rocketjump:
                    Display = "Rocket Jump";
                    Points = 50;
                    Continuous = false;
                    break;
                case TrickType.wall:
                    Display = "Wall";
                    Points = 75;
                    Continuous = false;
                    break;
                case TrickType.surfing:
                    Display = "Surfing";
                    Points = 2;
                    Continuous = true;
                    break;
                case TrickType.freefall:
                    Display = "Flying";
                    Points = 1;
                    Continuous = true;
                    break;
            }
        }
        private void SyncData(int syncs)
        {
            Continuous = false;
            if(syncs == 2)
            {
                Type = TrickType.sync;
                Display = "Sync";
                Points = 500;
            }
            else if(syncs == 3)
            {
                Type = TrickType.triSync;
                Display = "Tri-Sync";
                Points = 2500;
            }
            else if(syncs == 4)
            {
                Type = TrickType.quadSync;
                Display = "Quad-Sync";
                Points = 5000;
            }
            else if (syncs > 4)
            {
                Type = TrickType.megaSync;
                Display = "Mega-Sync";
                Points = 10000;
            }            
        }
    }

    private List<Trick> currentTricks = new List<Trick>();

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

        trickTextInitSize = trickText.rectTransform.localScale;

        scoreTextInitSize = scoreText.rectTransform.localScale;

        TrickText = trickText.GetComponent<SpriteText>();

        ScoreText = scoreText.GetComponent<SpriteText>();

    }

    public void RegisterPlayer(GameObject player)
    {
        pc = player.GetComponent<PlayerControlRigid>();
    }

    public void AddTrick(Trick trick)
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
        currentTricks.Add(trick);

        if(currentTricks.Count > maxTricks)
        {
            currentTricks.RemoveAt(0);
        }
        TrickText.input = string.Join("+ ", currentTricks.Select(ty =>ty.Display));

        Score += trick.Points;

        ScoreText.input = Score.ToString("#,##0") + " x " + TrickCount;
        TrickText.Refresh();
        ScoreText.Refresh();

        if(trickAnimation != null)
        {
            StopCoroutine(trickAnimation);
            trickText.rectTransform.localScale = trickTextInitSize;
        }

        if(ClockReset != null)
        {
            StopCoroutine(ClockReset);
            ClockReset = null;
        }

        if(ClockAnimation != null)
        {
            ClockReset = StartCoroutine(ResetClock());
        }

        if(!surfing && !falling) StartBoredom();

        trickAnimation = StartCoroutine(AnimateText(trickText.rectTransform, trickTextInitSize, trickAnimationDur, addTrickAnimation));

        if(trick.Type != TrickType.freefall) StopFalling();
    }

    void StartBoredom()
    {
        if(BoredTimer != null)
        {
            StopCoroutine(BoredTimer);
            BoredTimer = null;
        }
        if(BoredomRoutine != null)
        {
            StopCoroutine(BoredomRoutine);
            BoredomRoutine = null;
        }

        BoredTimer = StartCoroutine(BoredomTimer());
    }

    IEnumerator BoredomTimer()
    {
        float t = 0f;

        while(t < boredCheckDur)
        {
            if(!PauseManager.Instance.paused) t += Time.deltaTime;
            yield return null;
        }

        BoredomRoutine = StartCoroutine(Boredom());
    }

    IEnumerator Boredom()
    {
        float t = 0f;

        Clock.SetActive(true);

        int frames = ClockAnimation.sprites.Length;
        if(frames == 0)
        {
            StartComboTimer();
            yield break;
        }
     
        ClockAnimation.fps = Mathf.FloorToInt(ClockAnimation.sprites.Length / boredDur);
        ClockAnimation.direction = false;

        while(t < boredDur)
        {
            if (!PauseManager.Instance.paused && !surfing && !falling && speed < 35f)
            {
                t += Time.deltaTime;
                float time = Mathf.Clamp01(t / boredDur);
                float f = (1f - time) * (frames - 1);
                int index = Mathf.RoundToInt(f);

                index = Mathf.Clamp(index, 0, frames - 1);

                ClockAnimation.index = index;
                ClockAnimation.image.sprite = ClockAnimation.sprites[index];

                float bigSpeed = t * -100f;
                float smallSpeed = t * -300f;

                var smallRot = smallHand.localEulerAngles;
                smallRot.z = smallSpeed;
                smallHand.localEulerAngles = smallRot;

                var bigRot = bigHand.localEulerAngles;
                bigRot.z = bigSpeed;
                bigHand.localEulerAngles = bigRot;

            }
        yield return null;
        }

        ClockAnimation.index = 0;
        ClockAnimation.image.sprite = ClockAnimation.sprites[0];

        StartComboTimer();

        BoredomRoutine = null;
    }

    IEnumerator ResetClock()
    {
        int frames = ClockAnimation.sprites.Length;
        if(frames == 0) yield break;

        int start = Mathf.Clamp(ClockAnimation.index, 0, frames - 1);
        int target = frames - 1;
        float t = 0f;
        float dur = boredCheckDur;
        Vector3 startingRotSmall = smallHand.localEulerAngles;
        Vector3 startingRotBig = bigHand.localEulerAngles;

        while(t < dur)
        {
            t += Time.deltaTime;
            float time = Mathf.Clamp01(t / dur);
            float f = Mathf.Lerp(start, target, time);
            int index = Mathf.RoundToInt(f);
            index = Mathf.Clamp(index, 0, frames - 1);
            ClockAnimation.index = index;
            ClockAnimation.image.sprite = ClockAnimation.sprites[index];

    
                var smallRot = smallHand.localEulerAngles;
                smallRot.z = Mathf.Lerp(startingRotSmall.z, 0, time);
                smallHand.localEulerAngles = smallRot;

                var bigRot = bigHand.localEulerAngles;
                bigRot.z = Mathf.Lerp(startingRotBig.z, 0, time);
                bigHand.localEulerAngles = bigRot;

            yield return null;
        }
        ClockAnimation.index = target;
        ClockAnimation.image.sprite = ClockAnimation.sprites[target];
        ClockReset = null;
        Clock.SetActive(false);
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
        if (surfing) yield break;

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
            if(ComboTimer != null)
            {
                StopCoroutine(ComboTimer);
                ComboTimer = null;
            }
            ComboTimer = StartCoroutine(CompleteComboTimer());
            Clock.SetActive(false);
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

        if (comboTimerActive && !surfing)
        {
            StartCoroutine(CompleteCombo());
        }

        comboTimerActive = false;
        ComboTimer = null;
        comboTimerElapsed = 0f;
    }

    public void ResetCombo()
    {
        active = null;
        currentTricks.Clear();
        TrickText.input = string.Empty;
        TrickText.Refresh();
        ScoreText.input = string.Empty;
        ScoreText.Refresh();
        Score = 0;
        TrickCount = 0;
        accumulatedPoints = 0f;
        ClockAnimation.isPlaying = false;
        ClockAnimation.index = ClockAnimation.index = ClockAnimation.sprites.Length - 1;

    }

    public void RocketJump()
    {
        Trick trick = new Trick(TrickType.rocketjump);
        AddTrick(trick);
    }
    public void Pogo()
    {
        Trick trick = new Trick(TrickType.pogo);
        AddTrick(trick);
    }
    public void Wall()
    {
        Trick trick = new Trick(TrickType.wall);
        AddTrick(trick);
    }

    public void Sync(int syncs)
    {
        Trick trick = new Trick(TrickType.sync, syncs);
        AddTrick(trick);
    }

    public void Bomb()
    {
        Trick trick = new Trick(TrickType.bomb);
        AddTrick(trick);
    }

    public void Direct()
    {
        Trick trick = new Trick(TrickType.direct);
        AddTrick(trick);
    }

    public void Airshot()
    {
        Trick trick = new Trick(TrickType.airshot);
        AddTrick(trick);
    }

    public void AirAirshot()
    {
        Trick trick = new Trick(TrickType.airAirshot);
        AddTrick(trick);
    }

    public void Kill()
    {
        Trick trick = new Trick(TrickType.kill);
        AddTrick(trick);
    }

    public void Update()
    {
        
        if(PauseManager.Instance.paused) return;

        int points;

        if ((surfing || falling) && active != null)
        {

            if(ComboTimer != null)
            {
                StopCoroutine(ComboTimer);
                ComboTimer = null;
                comboTimerActive = false;
            }

            accumulatedPoints += (pps * speed) * Time.deltaTime;

            points = Mathf.FloorToInt(accumulatedPoints);
            if (points > 0)
            {
                
            Score += points;
            accumulatedPoints -= points;
            
            ScoreText.input = Score.ToString("#,##0") + " x " + TrickCount;
            ScoreText.Refresh();
            }
        }
        else
        {
            points  = 0;
        }
    }

    public void StartSurfing()
    {
        if(!surfing){

             if(ComboTimer != null)
            {
                StopCoroutine(ComboTimer);
                ComboTimer = null;
                comboTimerActive = false;
            }

            if(scoreAnimation != null)
            {
                StopCoroutine(scoreAnimation);
                scoreText.rectTransform.localScale = scoreTextInitSize;
            }

            StartBoredom();

            // bool last = currentTricks.Count > 0 && currentTricks[currentTricks.Count - 1].Type == TrickType.surfing;

            // if (last)
            // {
            //     active = currentTricks[currentTricks.Count - 1];
            //     surfing = true;
            // }
            // else
            // {
                Trick trick = new Trick(TrickType.surfing);
                AddTrick(trick);
                active = trick;
                surfing = true;
            // }
        }
    }

    public void StopSurfing()
    {
      if(surfing){
            active = null;
            surfing = false;
            accumulatedPoints = 0f;

            StartBoredom();
        }
    }

    public void StartFalling()
    {
        if(!falling){
            if(ComboTimer != null)
            {
                StopCoroutine(ComboTimer);
                ComboTimer = null;
                comboTimerActive = false;
            }

            if(scoreAnimation != null)
            {
                StopCoroutine(scoreAnimation);
                scoreText.rectTransform.localScale = scoreTextInitSize;
            }

            StartFallingTimer();
            }
    }

    void StartFallingTimer()
    {
        if(FallingTimerRoutine != null)
        {
            StopCoroutine(FallingTimerRoutine);
            FallingTimerRoutine = null;
        }
        FallingTimerRoutine = StartCoroutine(FallingTimer());
    }

    IEnumerator FallingTimer()
    {
        falling = true;
        float t = 0f;
        while(t < FallingDur)
        {
            t += Time.deltaTime;
            yield return null;
        }
        Trick trick = new Trick(TrickType.freefall);
        AddTrick(trick);
        active = trick;
        FallingTimerRoutine = null;
    }

    public void StopFalling()
    {
        if (falling)
        {
            if (FallingTimerRoutine != null)
            {
                StopCoroutine(FallingTimerRoutine);
                FallingTimerRoutine = null;
            }
            active = null;
            falling = false;
            accumulatedPoints = 0f;

            StartBoredom();
        }
    }

}
