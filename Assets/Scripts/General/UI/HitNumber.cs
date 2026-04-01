using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;

public class HitNumber : MonoBehaviour
{
    public enum HitType
    {
        Damage,
        Heal,
        Magic
    }
    [SerializeField] public HitType Type;

    [SerializeField] public TextMeshPro text;
    [SerializeField] public SpriteText spriteText;
    [SerializeField] public float lifespan;
    [SerializeField] public AnimationCurve curve;
    [SerializeField] public AnimationCurve sizeCurve;
    private float elapsed;
    private Vector3 velocity;
    
    private Vector3 baseScale;

    public void OnSpawn(float input, Vector3 position)
    {
        elapsed = 0f;

        baseScale = transform.localScale;

        float norm = input / 100f;

        float scaler = sizeCurve.Evaluate(norm);

        transform.localScale = baseScale * scaler;

        transform.position = position;
        string append = Type == HitType.Damage ? "-" : "+";
        spriteText.input = append + Mathf.RoundToInt(input).ToString();
        spriteText.Refresh();
        gameObject.SetActive(true);
        
    }

    public void Update()
    {
        elapsed += Time.deltaTime;

        float t = (elapsed / lifespan);

        text.color = new Color(1f, 1f,1f, curve.Evaluate(t));

        if(elapsed >= lifespan)
        {
            HitNumberManager.Instance.ReturnHitNumber(this);
            transform.localScale = baseScale;
        }
    }
}
