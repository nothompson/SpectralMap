using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIJitter : MonoBehaviour
{
    // Start is called before the first frame update

    RectTransform rectTransform;
    Vector2 placement;

    public float speed = 10f;

    public float chaos = 1f;

    public float randx = 10f;
    public float randy = 1f;

    public bool init = false;


    void Start()
    {
        rectTransform = (RectTransform)transform;
        enabled = false;
        if (init)
        {
            EnableJitter();
        }
    }

    void LateUpdate()
    {
        
        float x = Mathf.PerlinNoise(Time.unscaledTime * 10f, randx) - 0.5f;
        float y = Mathf.PerlinNoise(Time.unscaledTime * 10f, randy) - 0.5f;

        Vector2 pos = new Vector2(x, y) * chaos;

        rectTransform.anchoredPosition = placement + pos;
    }

    public void EnableJitter()
    {
        placement = rectTransform.anchoredPosition;
        enabled = true;
    }
}
