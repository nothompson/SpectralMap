using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteUI : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    public float X;
    public float Y;
    private Vector2 placement;
    public Image image;

    private SpriteText spriteText;


    virtual public void Awake()
    {
        image = GetComponent<Image>();
        spriteText = GetComponentInChildren<SpriteText>();
            if (spriteText == null)
            Debug.LogWarning("SpriteText component not found in children.");

        placement = new Vector2(X, Y);
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

        if (normal <= 1.0f)
        {
            image.sprite = sprites[index];
        }

        RectTransform rt = (RectTransform)transform;

        float x = Mathf.PerlinNoise(Time.time * 10f, randx) - 0.5f;
        float y = Mathf.PerlinNoise(Time.time * 10f, randy) - 0.5f;

        Vector2 pos = new Vector2(x, y) * (1 + index * chaos);

        rt.anchoredPosition = placement + pos;

        if(spriteText != null)
        {
            spriteText.input = Mathf.RoundToInt(current).ToString();

            spriteText.Refresh("FF0000");
        }
    }
}
