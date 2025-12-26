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

    public SpriteRenderer sprite;
    public bool worldSpace = false;
    private int length;
    public int index;
    private float _timer;

    public bool isPlaying = true;

    public Coroutine _current; 

    virtual public void Awake()
    {
        image = GetComponent<Image>();

        sprite = GetComponent<SpriteRenderer>();

        length = sprites.Length; 

        index = startingIndex;
        _timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(length < 1) return;
        if (worldSpace)
        {
            if(sprite!= null)
                Animate(worldSpace);
        }
        else
        {
            if(image !=null)
                Animate(worldSpace);
        }
    }

    void Animate(bool world)
    {
        if(isPlaying){

        _timer+= Time.unscaledDeltaTime;
        float frameDur = 1f / fps;
        if(_timer >= frameDur)
        {
            _timer -= frameDur;
            index = (index + 1) % length;
                if (world)
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
        image.sprite = sprites[index];
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
