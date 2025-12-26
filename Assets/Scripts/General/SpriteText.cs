using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpriteText : MonoBehaviour
{
    [TextArea(10,20)]
    public string input;

    public float cps;

    float _timer;

    public Coroutine typing = null;

    public bool isTyping = false;
    public bool fullTextShown = false;

    public bool init = false;

    private TMP_Text target;

    public Dictionary<char,int> charToSpriteIndex = new Dictionary<char, int>()
    {
        {'0', 0},
        {'1', 1},
        {'2', 2},
        {'3', 3},
        {'4', 4},
        {'5', 5},
        {'6', 6},
        {'7', 7},
        {'8', 8},
        {'9', 9},
        {'A', 10},
        {'B', 11},
        {'C', 12},
        {'D', 13},
        {'E', 14},
        {'F', 15},
        {'G', 16},
        {'H', 17},
        {'I', 18},
        {'J', 19},
        {'K', 20},
        {'L', 21},
        {'M', 22},
        {'N', 23},
        {'O', 24},
        {'P', 25},
        {'Q', 26},
        {'R', 27},
        {'S', 28},
        {'T', 29},
        {'U', 30},
        {'V', 31},
        {'W', 32},
        {'X', 33},
        {'Y', 34},
        {'Z', 35},
        {'.', 36},
        {',', 37},
        {':', 38},
        {';', 39},
        {'?', 40},
        {'!', 41},
        {'-', 42},
        {'+', 43},
        {'>', 44},
        {'<', 45},
        {'$', 46},
        {'\'', 47},
        {'"', 48},
        
    };

    void Awake()
    {
        target = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("TMP_Text target is not assigned on " + gameObject.name);
        }

        if(init == true)
        {
            Refresh();
        }
    }

    public string Convert(string input, string colorHex = null)
    {
        if(string.IsNullOrEmpty(input)) return string.Empty;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //convert each character clamped to uppercase to sprite sheet font
        foreach(char c in input)
        {
            char lookup = char.ToUpperInvariant(c);

            if (c == '\n')
            {
                sb.Append("\n");  // Preserve new line in output string
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if(c != '\n')
                    sb.Append(" ");
            }
            else if(charToSpriteIndex.TryGetValue(lookup, out int index))
            {
                if (!string.IsNullOrEmpty(colorHex))
                {
                    sb.Append($"<color=#{colorHex}><sprite={index}></color>");
                }
                else
                {
                    sb.Append($"<sprite={index}>");
                }
            }
            //use default font if not found in sprite sheet (WIP)
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public void Refresh(string colorHex = null)
    {
        //ususally called on update 
        if (target != null){
            target.text = Convert(input, colorHex);
            }
    }

    public IEnumerator Typewriter(float cps = 5, string colorHex = null)
    {
        if(target == null || string.IsNullOrEmpty(input))
        {
            yield break;
        }

        float frameDur = 1f / cps;

         isTyping = true;
         fullTextShown = false;

        for(int i = 1; i<=input.Length; i++)
        {
            string character = input.Substring(0,i);
            target.text = Convert(character, colorHex);
            yield return new WaitForSeconds(frameDur);
        }
        isTyping = false;
        fullTextShown = true;
    }

    public Coroutine StartTypewriter(MonoBehaviour script, float cps = 5, string colorHex = null)
    {
        fullTextShown = false;
        StopTypewriter(script);
        typing = script.StartCoroutine(Typewriter(cps,colorHex));
        return typing;
    }

    public void StopTypewriter(MonoBehaviour script)
    {
        if(typing != null)
        {
            script.StopCoroutine(typing);
            typing = null;
        }
        isTyping = false;
    }

    public void ShowText(MonoBehaviour script, string colorHex = null)
    {
        StopTypewriter(script);
        fullTextShown = true;
        if(target != null)
        {
            target.text = Convert(input, colorHex);
        }
    }
}
