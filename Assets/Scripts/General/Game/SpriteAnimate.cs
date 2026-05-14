using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SpriteAnimate : MonoBehaviour
{

    [SerializeField] public Sprite[] sprites; 

    [SerializeField] public int startingIndex = 0; 
    [SerializeField] public bool randomStart = false; 

    public int fps = 2;
    
    public Image image;

    [SerializeField] Material material = null;

    public SpriteRenderer sprite;
    public bool worldSpace = false;
    public int length;
    public int index;
    private float _timer;

    public bool isPlaying = true;
    [SerializeField] public bool pingPong = false;
    private int pingPongDir;
    public bool direction = true;

    public UnityEvent OnTargetEvent;


    public Coroutine _current; 

    virtual public void Awake()
    {
        image = GetComponent<Image>();

        sprite = GetComponent<SpriteRenderer>();
        if(sprite == null)
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
        }

        length = sprites.Length;

        if (randomStart)
        {
            startingIndex = (int)Random.Range(0, sprites.Length - 1);
        }

        index = startingIndex;
        _timer = 0f;

        pingPongDir = direction ? 1 : -1;
    }
    void Update()
    {
        if(length < 1) return;
        if (worldSpace)
        {
            if(sprite!= null)
                Animate(worldSpace, direction);
        }
        else
        {
            if(image !=null)
                Animate(worldSpace, direction);
        }
    }

    void Animate(bool world, bool increment = true)
    {
        if(isPlaying){
        
        int dir = increment ? 1 : -1;

        _timer+= Time.unscaledDeltaTime;
        float frameDur = 1f / fps;
        if(_timer >= frameDur)
        {

            _timer -= frameDur;

            if(pingPong)
            {
                index += pingPongDir;
                if(index >= sprites.Length || index < 0)
                    {
                        pingPongDir*= -1; 
                        index = Mathf.Clamp(index, 0, sprites.Length - 1);
                    }
            }
            else
            {
                index += dir;
                if(index >= sprites.Length) index = 0;
                else if(index < 0) index = sprites.Length - 1;
            }
                if(material != null)
                {
                    material.SetTexture("_Input", sprites[index].texture);
                }
                else if (world)
                {
                    sprite.sprite = sprites[index];
                }
                else
                {
                    image.sprite = sprites[index];
                }
        }
        }
    }
    public void SetFrame(int frame)
    {
        isPlaying = false;
        index = Mathf.Clamp(frame, 0, sprites.Length - 1);

           if(material != null)
                {
                    material.SetTexture("_Input", sprites[index].texture);
                }
        else
        {
            if (worldSpace)
            {
                sprite.sprite = sprites[index];
            }
            else{
                image.sprite = sprites[index];
                }
        }
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void AnimateFunction(int targetFrame)
    {
        if(_current != null)
        {
            StopCoroutine(_current);
        }
        _current = AnimateTo(this, targetFrame);
    }


private bool atEnd = false;
public void AnimateToEnd()
{
    isPlaying = false;
    if (_current != null)
    {
        StopCoroutine(_current);
    }

    int targetFrame = atEnd ? 0 : sprites.Length - 1;
    atEnd = !atEnd;
    _current = StartCoroutine(AnimateToTarget(targetFrame));
}

    public Coroutine AnimateTo(MonoBehaviour script, int targetFrame, System.Action <int> onFrameChanged = null, System.Action onTarget = null)
    {
        return script.StartCoroutine(AnimateToTarget(targetFrame, onFrameChanged, onTarget));
    }

    public IEnumerator AnimateToTarget(int targetFrame, System.Action <int> onFrameChanged = null, System.Action onTarget = null)
    {
        if(this == null) yield break;

        while(index != targetFrame)
        {
            if(index < targetFrame)
            {
                SetFrame(index + 1);
            }
            else if (index > targetFrame)
            {
                SetFrame(index - 1);
            }
            onFrameChanged?.Invoke(index);

            yield return new WaitForSecondsRealtime(1f/fps);

        }

        onTarget?.Invoke();
        OnTargetEvent?.Invoke();
    }
}
