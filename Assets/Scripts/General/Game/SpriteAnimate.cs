using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimate : MonoBehaviour
{

    [SerializeField] public Sprite[] sprites; 

    [SerializeField] private int startingIndex = 0; 

    public int fps = 2;
    
    public Image image;

    [SerializeField] Material material = null;

    public SpriteRenderer sprite;
    public bool worldSpace = false;
    public int length;
    public int index;
    private float _timer;

    public bool isPlaying = true;
    public bool direction = true;

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

        index = startingIndex;
        _timer = 0f;
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
            index += dir;

            if(index >= length) index = 0;
            else if(index < 0) index = length - 1;
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
        index = Mathf.Clamp(frame, 0, length - 1);

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
    }
}
