using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayButton : SceneTriggerButton
{
    public MainMenu mainMenu;

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
