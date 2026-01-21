using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteUI : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private Vector3 placement;
    public Image image;

    public SpriteText spriteText;

    private RectTransform rect;


    private int lastindex = 0;
    private float lastvalue = 0;
    Coroutine LerpingRoutine;

    [SerializeField] private SpriteAnimate spriteAnimate = null;


    virtual public void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        if(spriteText == null) spriteText = GetComponentInChildren<SpriteText>();

        placement = rect.anchoredPosition;

        if(spriteAnimate != null) sprites = spriteAnimate.sprites;
    }

    virtual public void Calculate(float current, float max, float chaos, float randx, float randy, bool up, bool cap = true, string colorHex = null)
    {

        int spriteSize = sprites.Length;

        float normal = up ? 1f - (current / max) : (current / max); 

        int index = Mathf.FloorToInt(normal * (spriteSize - 1));

        if(cap){
            if(current > max){
                current = max;
            }
        }

        if(Mathf.Approximately(current,lastvalue)) return;

        lastvalue = current;

        if (normal <= 1.0f && index != lastindex)
        {
            lastindex = index;
            if(spriteAnimate != null)
            {
                spriteAnimate.AnimateFunction(index);
            }
            else
            {
            image.sprite = sprites[index];
            }
        }

        if(spriteText != null)  
        {
            spriteText.input = Mathf.RoundToInt(current).ToString();

            spriteText.Refresh();
        }
    }

    void Jitter(int index, float randx, float randy, float chaos)
    {
        float x = Mathf.PerlinNoise(Time.time * 10f, randx) - 0.5f;
        float y = Mathf.PerlinNoise(Time.time * 10f, randy) - 0.5f;

        Vector3 pos = new Vector3(x, y, 0) * (1 + index * chaos);

        rect.anchoredPosition = placement + pos;
    }

    public void LerpTowards(int current, int target)
    {
        if(LerpingRoutine != null) StopCoroutine(LerpingRoutine);
        LerpingRoutine = StartCoroutine(Lerp(current, target));
    }

    IEnumerator Lerp(int current, int target)
    {
        float t = 0f;
        float dur = 1f;
        int maxindex = sprites.Length - 1;
        current = Mathf.Clamp(current,0,maxindex);
        target = Mathf.Clamp(target,0,maxindex);
        while(t < dur)
        {
            t += Time.deltaTime;
            float time = t / dur;
            float frame = Mathf.Lerp(current, target, time);
            int index = Mathf.RoundToInt(frame);
            if(index != lastindex){
                lastindex = index;
                image.sprite = sprites[index];
            }

            yield return null;
        }
        image.sprite = sprites[target];
        lastindex = target;
        LerpingRoutine = null;
    }


}
