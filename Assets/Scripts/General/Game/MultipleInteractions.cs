using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MultipleInteractions : MonoBehaviour
{
    public SpriteAnimate[] SpriteAnimations;

    public bool CheckForFrame(int i)
    {
        foreach(var s in SpriteAnimations)
        {
            if(s.index != i) return false;
        }
        
        return true;
    }

}