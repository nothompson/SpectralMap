using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteEventHandler : MonoBehaviour
{
    public SpriteAnimate spriteAnimate;
    public SpriteAnimationEvent[] animationEvents;

    private SpriteAnimationEvent restingEvent;
    private SpriteAnimationEvent currentEvent;
    private Coroutine eventCoroutine;

    void Awake()
    {
        foreach(var e in animationEvents)
        {
            if (e.restingAnimation)
            {
                restingEvent = e;
                break;
            }
        }
    }

    void Start()
    {
        if(restingEvent != null)
        {
            ApplyEvent(restingEvent, loop: true);
        }
    }
        public void PlayEvent(string id)
    {
        SpriteAnimationEvent target = null;
        foreach(var e in animationEvents)
        {
            if(id == e.id){
                target = e;
                break;
            }
        }

        if(target == null)
        {
            return;
        }

        if (target.restingAnimation)
        {
            ReturnToResting();
            return;
        }

        if(eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
        }

        currentEvent = target;
        eventCoroutine = StartCoroutine(EventRoutine(target));
    }

    public void ReturnToResting()
    {
        if(eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
            eventCoroutine = null;
        }

        currentEvent = restingEvent;

        if(restingEvent != null)
        {
            ApplyEvent(restingEvent, loop: true);
        }
    }

    private IEnumerator EventRoutine(SpriteAnimationEvent e)
    {
        ApplyEvent(e, loop: false);

        float dur = GetEventDuration(e);
        yield return new WaitForSecondsRealtime(dur);

        currentEvent = restingEvent;

        if(restingEvent != null)
        {
            ApplyEvent(restingEvent, loop: true);
        }

        eventCoroutine = null;
    }

    private void ApplyEvent(SpriteAnimationEvent e, bool loop)
    {
        spriteAnimate.sprites = e.sprites;
        spriteAnimate.length = e.sprites.Length;
        spriteAnimate.fps = e.fps;
        spriteAnimate.pingPong = e.pingPong;

        spriteAnimate.index = 0;
        spriteAnimate.isPlaying = true;
        spriteAnimate.direction = true;
    }

    private float GetEventDuration(SpriteAnimationEvent e)
    {
        if(e.fps <= 0) return 0f;
        float singlePass = (float)e.sprites.Length / e.fps;

        return e.pingPong ? singlePass * 2f : singlePass;
    }

    public bool IsPlayingEvent()
    {
        return eventCoroutine != null;
    }

    public string CurrentEventID()
    {
        return currentEvent != null ? currentEvent.id : null;
    }


}