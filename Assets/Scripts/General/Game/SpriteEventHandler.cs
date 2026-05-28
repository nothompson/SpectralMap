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
    private SpriteAnimationEvent nextEvent;
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

        nextEvent = GetSequenceEnd(target);

        currentEvent = target;
        eventCoroutine = StartCoroutine(EventRoutine(target));
    }

    public SpriteAnimationEvent GetSequenceEnd(SpriteAnimationEvent e)
    {
        while(e != null && !e.restingAnimation)
        {
            e = e.next;
        }
        return e;
    }

    public void ReturnToResting()
    {
        if(eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
            eventCoroutine = null;
        }

        SpriteAnimationEvent target = nextEvent != null ? nextEvent : restingEvent;
        nextEvent = null;
        SetResting(target);
    }

    private IEnumerator EventRoutine(SpriteAnimationEvent e)
    {
        while (e != null && !e.restingAnimation)
        {
            ApplyEvent(e, loop: false);
            float dur = GetEventDuration(e);
            yield return new WaitForSecondsRealtime(dur);
            e = e.next;
        }

        SpriteAnimationEvent landing = nextEvent != null ? nextEvent : restingEvent;
        SetResting(landing);
        nextEvent = null;
        eventCoroutine = null;
    }

    private void SetResting(SpriteAnimationEvent e)
    {
        restingEvent = e;
        currentEvent = e;
        if(e != null)
        {
            ApplyEvent(e, loop: true);
        }
    }

    private void ApplyEvent(SpriteAnimationEvent e, bool loop)
    {
        spriteAnimate.loop = loop;
        spriteAnimate.isPlaying = false;
        spriteAnimate.sprites = e.sprites;
        spriteAnimate.length = e.sprites.Length;
        spriteAnimate.fps = e.fps;
        spriteAnimate.pingPong = e.pingPong;

        spriteAnimate.index = 0;
        spriteAnimate.direction = true;
        spriteAnimate.isPlaying = true;
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