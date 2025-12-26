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

        public GameObject title;
        public GameObject[] letters;
        public SpriteAnimate[] letterAnimations;
        public int[] targetFrames = new int[12];

        private UIJitter[] jitters;

        private Coroutine[] activeAnimations;

        public AnimationCurve scaleCurve;

        public AnimationCurve positionCurve;

        public float introDur;

        public float introSpread;

        public Material bg;

        void Start()
        {
            StopAllCoroutines();

            DisableTricks();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

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
        }

        IEnumerator SpriteIntro()
        {
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
                spriteOffset[i] = bipolar * introSpread;
            }

            for(int i = 0; i < buttons.Length; i++)
            {
                if(buttons[i] == null) continue;

                buttonRects[i] = buttons[i].GetComponent<RectTransform>();
                if(buttonRects[i] == null) continue;

                basePositions[i] = buttonRects[i].anchoredPosition;

                buttonRects[i].anchoredPosition = new Vector2(buttonRects[i].anchoredPosition.x, -400f);
                
                float bipolar = Random.value * 2f - 1f;
                buttonOffset[i] = bipolar * introSpread;
            }

            float t = 0f;

            yield return new WaitForSeconds(0.5f);

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
                }

                for(int i = 0; i < buttonRects.Length; i++)
                {
                    if(buttonRects[i] == null) continue;

                    float time = (t + buttonOffset[i]) / introDur;

                    time = Mathf.Clamp01(time);

                    float curveValue = positionCurve.Evaluate(time);

                    buttonRects[i].anchoredPosition = basePositions[i] * curveValue;
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

            if(letterAnimations != null && letterAnimations.Length > 0)
            {
                StartCoroutine(AnimateTitle());
            }
        }

        public IEnumerator AnimateTitle()
        {
            float letterDelay = 0.05f;

            for(int i = 0; i < letterAnimations.Length; i++)
            {
                letters[i].SetActive(true);
                var anim = letterAnimations[i];

                if(anim == null || anim.sprites == null || anim.sprites.Length == 0) continue;

                int lastFrame = anim.sprites.Length - 1;

                anim.SetFrame(lastFrame);

                StartCoroutine(anim.AnimateToTarget(0));

                yield return new WaitForSeconds(letterDelay);
                
            }
        }

        void Update()
        {
            GetMouseData();
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

                float xTriangle = Mathf.Abs(2f* (xnorm - Mathf.Floor(xnorm + 0.5f)));
                float bipolarX = (8f * xnorm) - 4f;

                bg.SetFloat("_Voronoi", voronoi); 

                bg.SetFloat("_TwirlStrength", bipolarX);

                bg.SetFloat("_YStrength", deltaSmoothed);

                bg.SetFloat("_Y", bipolarY);

                Vector2 normalized = new Vector2(xnorm,ynorm);

                LevelManager.Instance.currentTrack.SetParameter("xPos", normalized.x);
                LevelManager.Instance.currentTrack.SetParameter("yPos", normalized.y);
                LevelManager.Instance.currentTrack.SetParameter("WetDryMusic", deltaSmoothed);

                UpdateJitter(deltaSmoothed);

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
    }
