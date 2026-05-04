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

    public int FinalScore = 0;

    public float ResetTime = 1f;

    private SpriteText TrickText;
    private SpriteText ScoreText;
    public RectTransform ScoreRect;

    public AnimationCurve addTrickAnimation;
    public AnimationCurve completeComboAnimation; 

    [SerializeField] private AnimationCurve ClockCurve;

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
    public float accumulatedPoints = 0f;
    public int storedPoints;
    public float speed = 0f;

    public GameObject Clock;
    public SpriteAnimate ClockAnimation;

    public RectTransform smallHand;
    public RectTransform bigHand;

    Coroutine ClockReset;

    Coroutine ClockTransition;

    private bool clockActive = false;

    [SerializeField] private float transitionDur;

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
            kill,

            grapple,
            grappleJump,
        }

    public class Trick
    {
        public TrickType Type;
        public string Display;
        public int Points;
        public bool Continuous;
        public Trick(TrickType type, int syncs = 1, int customPoints = 0, string customDisplay = null)
        {
            Type = type;
            if(customPoints > 0) Points = customPoints;
            if(customDisplay != null) Display = customDisplay;
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
                    Continuous = false;
                    break;

                case TrickType.pogo:
                    Display = "Pogo";
                    Points = 100;
                    Continuous = false;
                    break;

                case TrickType.rocketjump:
                    Display = "Blast Jump";
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
                    Display = "Free Falling";
                    Points = 1;
                    Continuous = true;
                    break;

                case TrickType.grapple:
                    Display = "Grapple";
                    Points = 50;
                    Continuous = false;
                    break;
                case TrickType.grappleJump:
                    Display = "Grapple Jump";
                    Points = 100;
                    Continuous = false;
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

    public List<Trick> currentTricks = new List<Trick>();

    public List<Trick> trickHistory = new List<Trick>();

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

        trickHistory.Add(trick);

        currentTricks.Add(trick);

        TrickCount++;

        bool continuous = false;
        if(currentTricks.Count > 1)
        {
            Trick last = currentTricks[currentTricks.Count - 2];
            Trick cur = currentTricks[currentTricks.Count - 1];

            if(last != null && last.Continuous && cur.Continuous && last.Type == cur.Type)
            {
                TrickCount--;
                currentTricks.RemoveAt(0);
                continuous = true;
            }
        }
   


        if(currentTricks.Count > maxTricks)
        {
            currentTricks.RemoveAt(0);
        }


        if(!continuous){
            TrickText.input = string.Join("+ ", currentTricks.Select(ty =>ty.Display));
        }

        Score += trick.Points;

        ScoreText.input = Score.ToString("#,##0") + " x " + TrickCount;
        TrickText.Refresh();
        ScoreText.Refresh();

        if(trickAnimation != null)
        {
            StopCoroutine(trickAnimation);
            trickText.rectTransform.localScale = trickTextInitSize;
        }

        if(clockActive && ClockReset != null)
        {
            StopCoroutine(ClockReset);
        }
        if (clockActive && !surfing && !falling)
        {
            ClockReset = StartCoroutine(ResetClock());
        }

        if(!surfing && !falling) StartBoredom();

        trickAnimation = StartCoroutine(AnimateText(trickText.rectTransform, trickTextInitSize, trickAnimationDur, addTrickAnimation));

        if(trick.Type != TrickType.freefall) StopFalling();
    }

    public void StartTransition(bool intro)
    {
        if(ClockTransition != null)
        {
            StopCoroutine(ClockTransition);
            ClockTransition = null;
        }
        StartCoroutine(Transition(intro));
    }

    IEnumerator Transition(bool intro)
    {
        float t = 0f;
        Vector3 target = intro ? Vector3.one : Vector3.zero;
        RectTransform clockRect = Clock.GetComponent<RectTransform>();
        Vector3 starting;

        if (intro)
        {
            Clock.SetActive(true);
            clockRect.localScale = Vector3.zero;
            starting = Vector3.one;
        }
        else
        {
            starting = clockRect.localScale;
        }

        
        while(t < transitionDur)
        {
            t += Time.deltaTime;
            float time = t / transitionDur;
            float elapsed = intro ? time : 1f - time;
            float value = ClockCurve.Evaluate(elapsed);

            clockRect.localScale = starting * value;
            yield return null;
        }
        clockRect.localScale = target;
        if (!intro)
        {
            Clock.SetActive(false);
        }

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

        if(Score <= 0 || completed) return;

        BoredTimer = StartCoroutine(BoredomTimer());
    }

    IEnumerator BoredomTimer()
    {
        float t = 0f;

        while(t < boredCheckDur)
        {
            if(!PauseManager.Instance.paused && !falling && !surfing) t += Time.deltaTime;
            yield return null;
        }

        if(!clockActive){
            StartTransition(true);
            clockActive = true;
        }

        BoredomRoutine = StartCoroutine(Boredom());
    }

    IEnumerator Boredom()
    {
        if(ClockReset != null)
        {
            StopCoroutine(ClockReset);
            ClockReset = null;
        }
        float t = 0f;
        
        int frames = ClockAnimation.sprites.Length;
        if(frames == 0)
        {
            StartComboTimer();
            yield break;
        }

        ClockAnimation.fps = Mathf.FloorToInt(ClockAnimation.sprites.Length / boredDur);
        ClockAnimation.direction = false;

        Vector3 starting = bigHand.localEulerAngles;

        while(t < boredDur)
        {
            if (!PauseManager.Instance.paused && !surfing && !falling)
            {
                t += Time.deltaTime;
                float time = Mathf.Clamp01(t / boredDur);
                float f = (1f - time) * (frames - 1);
                int index = Mathf.RoundToInt(f);

                index = Mathf.Clamp(index, 0, frames - 1);

                ClockAnimation.index = index;
                ClockAnimation.image.sprite = ClockAnimation.sprites[index];

                float bigSpeed = t * -100f;
                float smallSpeed = t * -500f;

                Vector3 smallRot = smallHand.localEulerAngles;
                smallRot.z = smallSpeed;
                smallHand.localEulerAngles = smallRot;

                Vector3 bigRot = bigHand.localEulerAngles;
                bigRot.z = Mathf.Lerp(starting.z, -360f, time);
                bigHand.localEulerAngles = bigRot;

            }
            if (!clockActive)
            {
                yield break;
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
        if(BoredomRoutine != null)
        {
            StopCoroutine(BoredomRoutine);
            BoredomRoutine = null;
        }
        if(BoredTimer != null)
        {
            StopCoroutine(BoredTimer);
            BoredTimer = null;
        }
        int frames = ClockAnimation.sprites.Length;
        if(frames == 0) yield break;

        int start = Mathf.Clamp(ClockAnimation.index, 0, frames - 1);
        int target = frames - 1;
        float t = 0f;
        float dur = boredCheckDur;

        float startSmall = smallHand.localEulerAngles.z;
        if(startSmall > 180f) startSmall -= 360f;

        float startBig = bigHand.localEulerAngles.z;
        if(startBig > 180f) startBig -= 360f;

        if(startSmall > 0f) startSmall -= 360f; 
        if(startBig > 0f) startBig -= 360f;

        while(t < dur)
        {
            t += Time.deltaTime;
            float time = Mathf.Clamp01(t / dur);
            
            float f = Mathf.Lerp(start, target, time);
            int index = Mathf.RoundToInt(f);
            index = Mathf.Clamp(index, 0, frames - 1);
            ClockAnimation.index = index;
            ClockAnimation.image.sprite = ClockAnimation.sprites[index];

            float smallZ = Mathf.Lerp(startSmall, 0f, time);
            float bigZ = Mathf.Lerp(startBig, 0f, time);

            Vector3 smallRot = smallHand.localEulerAngles;
            smallRot.z = smallZ;
            smallHand.localEulerAngles = smallRot;

            Vector3 bigRot = bigHand.localEulerAngles;
            bigRot.z = bigZ;
            bigHand.localEulerAngles = bigRot;

            yield return null;
        }

        ClockAnimation.index = target;
        ClockAnimation.image.sprite = ClockAnimation.sprites[target];

        Vector3 small = smallHand.localEulerAngles;
        small.z = 0f;
        smallHand.localEulerAngles = small;

        Vector3 big = bigHand.localEulerAngles;
        big.z = 0f;
        bigHand.localEulerAngles = big;

        ClockReset = null;
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

        if (ClockReset != null)
        {
            StopCoroutine(ClockReset);
            ClockReset = null;
        }

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

        if(Score == 0)
        {
            ScoreText.input = string.Empty;
            ScoreText.Refresh();
            yield break;
        }
        FinalScore = Score * TrickCount;
        ScoreText.input = FinalScore.ToString("#,##0");
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
        trickHistory.Clear();
        TrickText.input = string.Empty;
        TrickText.Refresh();
        ScoreText.input = string.Empty;
        ScoreText.Refresh();
        Score = 0;
        TrickCount = 0;
        FinalScore = 0;
        accumulatedPoints = 0f;
        storedPoints = 0;
        
        clockActive = false;
        StartTransition(false);

        if (ClockReset != null)
        {
            StopCoroutine(ClockReset);
            ClockReset = null;
        }

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

    public void Grapple()
    {
        Trick trick = new Trick(TrickType.grapple);
        AddTrick(trick);
    }
    public void GrappleJump()
    {
        Trick trick = new Trick(TrickType.grappleJump);
        AddTrick(trick);
    }

    public void Kill(int enemyPoints, string enemyName)
    {
        Trick trick = new Trick(TrickType.kill, customPoints: enemyPoints, customDisplay: enemyName);
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

            storedPoints += points;
            
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

            if(scoreAnimation != null)
            {
                StopCoroutine(scoreAnimation);
                scoreText.rectTransform.localScale = scoreTextInitSize;
            }

            Trick trick = new Trick(TrickType.surfing);
            AddTrick(trick);
            active = trick;
            surfing = true;
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
        if(surfing) return;
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
