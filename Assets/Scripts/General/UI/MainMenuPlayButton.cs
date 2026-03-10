using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayButton : SceneTriggerButton
{
    public MainMenu mainMenu;
    [SerializeField] private GameObject SaveSlotContainer;
    private int finished = 0;

    private Coroutine[] letterAnimate;

    private string scene; 

    private bool transitioning = false;

    public override void TriggerSceneChange(string sceneName)
    {

        if(transitioning) return; 

        if (mainMenu.introPlaying)
        {
            StartCoroutine(TitleQueue(sceneName));
            return;
        }
        if(mainMenu.letterAnimations == null || mainMenu.letterAnimations.Length < 1)
        {
            base.TriggerSceneChange(sceneName);
            return;
        }

        transitioning = true;

        scene = sceneName;

        finished = 0;

        letterAnimate = new Coroutine[mainMenu.letterAnimations.Length];


        for(int i = 0; i < mainMenu.letters.Length; i++)
        {
            var letters = mainMenu.letterAnimations[i];
            if(letters == null) continue;

            int lastFrame = letters.sprites.Length - 1;

            letterAnimate[i] = letters.AnimateTo(this,lastFrame,onTarget: ()=> LetterComplete(letters));
        }
    }

    IEnumerator TitleQueue(string sceneName)
    {
        while(mainMenu.introPlaying) yield return null;

        TriggerSceneChange(sceneName);
    }

    public void StartPlaySequence()
    {
        if(transitioning) return;
        StartCoroutine(OutroSequence());
    }

    IEnumerator OutroSequence()
    {
        while(mainMenu.introPlaying || !mainMenu.spriteIntroComplete) yield return null;

        mainMenu.OrganIsPlayable = false;

        AudioManager.Instance.TransitionTexture();
        StartCoroutine(AnimateLettersOut());
        yield return StartCoroutine(AnimateSpritesAndButtonsOut());

        while(finished < mainMenu.letterAnimations.Length) yield return null;

        SaveSlotContainer.SetActive(true);

    }

    IEnumerator AnimateLettersOut()
    {
        finished = 0;
        letterAnimate = new Coroutine[mainMenu.letterAnimations.Length];
        for(int i = 0; i < mainMenu.letters.Length; i++)
        {
            var letter = mainMenu.letterAnimations[i];
            if(letter ==null) continue;
            int lastFrame = letter.sprites.Length -1;
            letterAnimate[i] = letter.AnimateTo(this, lastFrame, onTarget: () =>
            {
                letter.gameObject.SetActive(false);
                finished++;
            });
        }
        yield return null;
    }

    IEnumerator AnimateSpritesAndButtonsOut()
    {
        mainMenu.DisableJitter();
        var rects = mainMenu.spriteRects;
        var baseScales = mainMenu.spriteBaseScales;
        var buttonRects = mainMenu.bRects;
        var basePositions = mainMenu.buttonBasePositions;

        float introDur = mainMenu.introDur;
        float introSpread = mainMenu.introSpread;

        var spriteOffset = new float[rects.Length];
        var buttonOffset = new float[buttonRects.Length];

        var spriteSoundPlayed = new bool[rects.Length];
        var buttonSoundPlayed = new bool[buttonRects.Length];
        for(int i = 0; i < rects.Length; i++)
        {
            spriteOffset[i] = (Random.value * 2f - 1f) * introSpread;
        }
        for(int i = 0; i < buttonRects.Length; i++)
        {
            buttonOffset[i] = (Random.value * 2f - 1f) * introSpread;
        }

        float t = 0f;

        while(t < introDur + introSpread)
        {
            t += Time.unscaledDeltaTime;

            for(int i = 0; i < rects.Length; i++)
            {
                if(rects[i] == null) continue;
                float time = Mathf.Clamp01((t + spriteOffset[i]) / (introDur));

                float curveValue = mainMenu.outroScaleCurve.Evaluate(time);
                rects[i].localScale = baseScales[i] * curveValue;

                // if(!spriteSoundPlayed[i] && curveValue <= 0.05f)
                // {
                //     spriteSoundPlayed[i] = true;
                //     AudioManager.Instance.Apop();
                // }

            }

              for(int i = 0; i < buttonRects.Length; i++)
            {
                if(buttonRects[i] == null) continue;
                float time = Mathf.Clamp01((t + buttonOffset[i]) / (introDur));
                float curveValue = mainMenu.outroPositionCurve.Evaluate(time);
                
                Vector2 offscreen = new Vector2(basePositions[i].x, -400f);
                buttonRects[i].anchoredPosition = basePositions[i] * curveValue;

                if(!buttonSoundPlayed[i] && curveValue >= 1.1f)
                {
                    buttonSoundPlayed[i] = true;
                    AudioManager.Instance.WindSlice();
                }
            }

            yield return null;
        }
                foreach(var r in rects)
            {
                if(r != null) r.localScale = Vector3.zero;
            }
            foreach(var r in buttonRects)
            {
                if(r != null) r.anchoredPosition = new Vector2(r.anchoredPosition.x, -400f);
            }
    }

    private void LetterComplete(SpriteAnimate letter)
    {
        letter.gameObject.SetActive(false);
        
        finished++;

        if(finished >= mainMenu.letterAnimations.Length)
        {
            base.TriggerSceneChange(scene);
        }
    }
}
