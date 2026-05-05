    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    using MusicScripts;

    public class MainMenu : MonoBehaviour
    {
        private Vector2 last;

        private float deltaSmoothed = 0f;

        private float deltaLerp = 2f;

        public FMODUnity.StudioEventEmitter chime;

        public GameObject[] sprites;

        public SpriteAnimate[] spriteAnimations;

        public GameObject[] buttons;
        public Graphic[] buttonGraphics;

        public GameObject title;
        public GameObject[] letters;
        public SpriteAnimate[] letterAnimations;
        public int[] targetFrames = new int[12];

        private UIJitter[] jitters;

        private Coroutine[] activeAnimations;

        public AnimationCurve scaleCurve;

        public AnimationCurve positionCurve;

        public AnimationCurve outroScaleCurve;

        public AnimationCurve outroPositionCurve;

        public float introDur;

        public float introSpread;

        public Material bg;

        public bool introPlaying = false;
        public bool queueLoad = false;

        public bool OrganIsPlayable = true;

        private int titlefinished = 0;

        private float blendSmoothed = 0f;

        private float blendLerp = 0.25f;

        private float blendvel;

        // private float directionSpeed = 0.2f;

        private float directionSeed;

        private Vector2 currentDirection;
        private Vector2 targetDirection;
        private Vector2 directionVelocity;

        [SerializeField] private float BaseDirectionSpeed;
        [SerializeField] private float BaseDistortStrength = 0.05f;
        [SerializeField] private float BaseDistortSpeed = 0.1f;
        [SerializeField] private float BaseFlowStrength = 0.5f;
        [SerializeField] private float BaseFlowSpeed = 0.1f;
        [SerializeField] private float BasePhaseStrength = 1f;

        [SerializeField] private float TargetDirectionSpeed;
        [SerializeField] private float TargetDistortStrength;
        [SerializeField] private float TargetDistortSpeed;
        [SerializeField] private float TargetFlowStrength;
        [SerializeField] private float TargetFlowSpeed;
        [SerializeField] private float TargetPhaseStrength;
        [SerializeField] private float keyDur;
        [SerializeField] private AnimationCurve KeyAnimation;


        [HideInInspector] public RectTransform[] spriteRects;
         [HideInInspector] public Vector3[] spriteBaseScales;
         [HideInInspector] public RectTransform[] bRects;
         [HideInInspector] public Vector3[] buttonBasePositions;

         [HideInInspector] public bool spriteIntroComplete = false;
        

        private float distortStrength;
        private float distortSpeed;

        Coroutine keydistortRoutine;


        void Start()
        {
            spriteIntroComplete = false;
            
            StopAllCoroutines();

            DisableTricks();

            Cursor.lockState = CursorLockMode.None;
            CursorManager.Instance.TriggerCursor(true);

            SettingsMenu.Instance.CloseButton.Targets = buttonGraphics;

            last = Vector2.zero;

            spriteAnimations = new SpriteAnimate[sprites.Length];
            for(int i = 0; i < sprites.Length; i++)
            {
                spriteAnimations[i] = sprites[i].GetComponent<SpriteAnimate>();
            }

            jitters = GetComponentsInChildren<UIJitter>();

            SetupDefaultKeys();

            activeAnimations = new Coroutine[sprites.Length];

            letterAnimations = new SpriteAnimate[letters.Length];
            for(int i = 0; i < letters.Length; i++)
            {
                letterAnimations[i] = letters[i].GetComponent<SpriteAnimate>();
            }

            StartCoroutine(SpriteIntro());

            if (PauseManager.Instance != null && PauseManager.Instance.paused)
            {
                    PauseManager.Instance.paused = false;
            }

            directionSeed = Random.Range(0f,1000f);

            Vector2 direction = new Vector2(Mathf.PerlinNoise(directionSeed, Time.unscaledTime * BaseDirectionSpeed) - 0.5f, Mathf.PerlinNoise(directionSeed + 100, Time.unscaledTime * BaseDirectionSpeed) - 0.5f);

            direction.Normalize();

            bg.SetVector("_Direction", direction);

            BaseValues();
            
        }

        IEnumerator SpriteIntro()
        {

            if(letterAnimations != null && letterAnimations.Length > 0)
            {
                StartCoroutine(AnimateTitle());
                AudioManager.Instance.RisingTexture();
            }

            var rects = new RectTransform[sprites.Length];
            var baseScales = new Vector3[sprites.Length];

            var buttonRects = new RectTransform[buttons.Length];
            var basePositions = new Vector3[buttons.Length];

            var spriteOffset = new float[sprites.Length];
            var buttonOffset = new float[buttons.Length];

            for(int i = 0; i < sprites.Length; i++)
            {
                if(sprites[i] == null) continue;

                rects[i] = sprites[i].GetComponent<RectTransform>();
                if(rects[i] == null) continue;

                baseScales[i] = new Vector3(1f,1f,1f);

                rects[i].localScale = Vector3.zero;

                float bipolar = Random.value * 2f - 1f;
                float rand = Random.value;
                spriteOffset[i] = rand * introSpread;
            }

            spriteRects = rects;
            spriteBaseScales = baseScales;


            for(int i = 0; i < buttons.Length; i++)
            {
                if(buttons[i] == null) continue;

                buttonRects[i] = buttons[i].GetComponent<RectTransform>();
                if(buttonRects[i] == null) continue;

                basePositions[i] = buttonRects[i].anchoredPosition;

                buttonRects[i].anchoredPosition = new Vector2(buttonRects[i].anchoredPosition.x, -400f);
                
                float bipolar = Random.value * 2f - 1f;
                float rand = Random.value;
                buttonOffset[i] = rand * introSpread;
            }
            bRects = buttonRects;
            buttonBasePositions = basePositions;

            var spriteSoundPlayed = new bool[sprites.Length];
            var buttonSoundPlayed = new bool[buttons.Length];
            
            float t = 0f;

            yield return new WaitForSeconds(1.5f);

            title.SetActive(true);

            while(t< introDur + introSpread)
            {
                t += Time.unscaledDeltaTime;

                for(int i = 0; i < rects.Length; i++)
                {
                    if(rects[i] == null) continue;

                    float time = (t + spriteOffset[i]) / introDur;

                    time = Mathf.Clamp01(time);

                    float curveValue = scaleCurve.Evaluate(time);

                    rects[i].localScale = baseScales[i] * curveValue;

                    if(!spriteSoundPlayed[i] && curveValue >= 0.8f)
                    {
                        spriteSoundPlayed[i] = true;
                        AudioManager.Instance.Pop();
                    }
                }

                for(int i = 0; i < buttonRects.Length; i++)
                {
                    if(buttonRects[i] == null) continue;

                    float time = (t + buttonOffset[i]) / introDur;

                    time = Mathf.Clamp01(time);

                    float curveValue = positionCurve.Evaluate(time);

                    buttonRects[i].anchoredPosition = basePositions[i] * curveValue;

                    if(!buttonSoundPlayed[i] && curveValue <= 1.5f)
                    {
                        buttonSoundPlayed[i] = true;
                        AudioManager.Instance.WindSlice();
                    }

                }
            yield return null;
            }

            for(int i = 0; i < rects.Length; i++)
            {
                if(rects[i] == null) continue;

                rects[i].localScale = baseScales[i];
            }

            for(int i = 0; i < buttonRects.Length; i++)
            {
                if(buttonRects[i] == null) continue;

                buttonRects[i].anchoredPosition = basePositions[i];

                UIJitter jitter = buttons[i].GetComponent<UIJitter>();
                if (jitter != null)
                    jitter.EnableJitter();
            }

            spriteIntroComplete = true;

        }

        public IEnumerator AnimateTitle()
        {
            yield return new WaitForSeconds(0.25f);
            introPlaying = true;
            titlefinished = 0;

            float letterDelay = 0.05f;

            for(int i = 0; i < letterAnimations.Length; i++)
            {
                letters[i].SetActive(true);
                var anim = letterAnimations[i];

                if(anim == null || anim.sprites == null || anim.sprites.Length == 0) continue;

                int lastFrame = anim.sprites.Length - 1;

                anim.SetFrame(lastFrame);

                anim.AnimateTo(this, 0, onTarget:() => CompleteTitleLetter());

                yield return new WaitForSeconds(letterDelay);
            }

            while(titlefinished < letterAnimations.Length) yield return null;

            introPlaying = false;
        }

        void CompleteTitleLetter()
        {
            titlefinished++;
        }

        public void DisableJitter()
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            if(buttons[i] == null) continue;
            UIJitter jit = buttons[i].GetComponent<UIJitter>();
            if(jit != null) jit.DisableJitter();
        }
    }

        void Update()
        {
            GetMouseData();
            Background();
            if(!OrganIsPlayable) return;
                MusicScript.PlayNotes(chime, MusicScript.MajorScale, -9);
                CheckKeyAnimations();
        }

        void SetupDefaultKeys()
        {
            for(int i = 0; i < Mathf.Min(10, sprites.Length); i++)
            {
                targetFrames[i] = spriteAnimations[i].sprites.Length - 1;
            }
            targetFrames[10] = spriteAnimations[10].sprites.Length - 1;
            targetFrames[11] = spriteAnimations[11].sprites.Length - 1;
        }

        void CheckKeyAnimations()
        {
            Key[] keys = {
                Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3,
                Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7,
                Key.Digit8, Key.Digit9, Key.Minus, Key.Equals
            };

            for(int i = 0; i < Mathf.Min(keys.Length, sprites.Length); i++)
            {
                if(Keyboard.current[keys[i]].wasPressedThisFrame)
                {
                    TriggerSpriteAnimations(i);
                    StartKeyDistortion();
                }
            }
        }

        void TriggerSpriteAnimations(int index)
        {
            if(index >= spriteAnimations.Length || spriteAnimations[index] == null) return;

            var spriteAnim = spriteAnimations[index];

            if (activeAnimations[index] != null)
            {
                StopCoroutine(activeAnimations[index]);
            }

            activeAnimations[index] = StartCoroutine(AnimateToTarget(spriteAnim, targetFrames[index]));
        }

        IEnumerator AnimateToTarget(SpriteAnimate sprite, int targetFrame)
        {
            if (targetFrame < 0 || targetFrame >= sprite.sprites.Length)
                yield break;

            yield return sprite.AnimateToTarget(targetFrame, null, null);

            yield return new WaitForSeconds(1f/sprite.fps);

            yield return sprite.AnimateToTarget(0, null, null);

        }

        void UpdateJitter(float input)
        {
            float chaos = 0.5f + input * 5f;

            foreach(var jit in jitters)
            {
                if(jit != null)
                {
                    jit.chaos = chaos;
                }
            }
        }

        void DisableTricks()
        {
            if(TrickManager.Instance != null)
            {
                TrickManager.Instance.ResetCombo();
            }
        }

        void GetMouseData()
        {
                Vector2 current = Mouse.current.position.ReadValue();

                float delta = MusicScript.NormalizeForAutomation((current - last).magnitude, 0f, 50f);

                deltaSmoothed = Mathf.Lerp(deltaSmoothed, delta, deltaLerp * Time.deltaTime);

                last = current;

                float xnorm = MusicScript.NormalizeForAutomation(current.x, 0f, Screen.width);
                float ynorm = MusicScript.NormalizeForAutomation(current.y, 0f, Screen.height);

                float voronoi = (deltaSmoothed * 2f) + 0.25f;

                float bipolarY = (2f * ynorm) - 1f;

                float dirx = ((1f - xnorm) * 2) - 1f;
                float diry = ((1f - ynorm) * 2) - 1f;

                Vector2 bip = new Vector2(dirx,diry);

                // currentDirection = Vector2.SmoothDamp(currentDirection, bip, ref directionVelocity, Time.deltaTime * BaseDirectionSpeed);

                float xTriangle = Mathf.Abs(2f* (xnorm - Mathf.Floor(xnorm + 0.5f)));
                float bipolarX = (8f * xnorm) - 4f;

                Vector2 normalized = new Vector2(xnorm,ynorm);

                // bg.SetVector("_Direction", currentDirection);

                LevelManager.Instance.currentTrack.setParameterByName("xPos", normalized.x);
                LevelManager.Instance.currentTrack.setParameterByName("yPos", normalized.y);
                LevelManager.Instance.currentTrack.setParameterByName("WetDryMusic", deltaSmoothed);

                UpdateJitter(deltaSmoothed);

        }

        void Background()
        {

            // Vector2 direction = new Vector2(Mathf.PerlinNoise(directionSeed, Time.unscaledTime * BaseDirectionSpeed) - 0.5f, Mathf.PerlinNoise(directionSeed + 100, Time.unscaledTime * BaseDirectionSpeed) - 0.5f);

            // direction.Normalize();

            // targetDirection = direction * BaseDirectionSpeed;

            // currentDirection = Vector2.SmoothDamp(currentDirection, targetDirection, ref directionVelocity, Time.deltaTime * BaseDirectionSpeed);

            // bg.SetVector("_Direction", currentDirection);

            float blend = Mathf.PerlinNoise(Time.unscaledTime * 0.1f, 2f);

            blendSmoothed = Mathf.SmoothDamp(blendSmoothed, blend, ref blendvel, blendLerp * Time.deltaTime);

            bg.SetFloat("_Blend", blendSmoothed);

            bg.SetFloat("_DistortStrength", BaseDistortStrength + distortStrength);

            bg.SetFloat("_DistortSpeed", BaseDistortSpeed + distortSpeed);

        }

        void StartKeyDistortion()
        {
            if(keydistortRoutine != null){
                StopCoroutine(keydistortRoutine);
                BaseValues();
            }
            keydistortRoutine = StartCoroutine(KeyDistortion());
        }

        IEnumerator KeyDistortion()
        {
            float t = 0f;

            float newSeed = Random.Range(0f,1000f);

            while(t < keyDur)
            {
                t += Time.deltaTime;
                float time = t / keyDur;
                float value = KeyAnimation.Evaluate(time);

                distortStrength = TargetDistortStrength * value;
                distortSpeed = TargetDistortSpeed * value;

                yield return null;
            }

            keydistortRoutine = null;
        }

        void BaseValues()
        {
                bg.SetFloat("_DistortStrength", BaseDistortStrength);
                bg.SetFloat("_DistortSpeed", BaseDistortSpeed);
                bg.SetFloat("_FlowStrength", BaseFlowStrength);
                bg.SetFloat("_FlowSpeed", BaseFlowSpeed);
                bg.SetFloat("_PhaseStrength", BasePhaseStrength);
        }
    }
