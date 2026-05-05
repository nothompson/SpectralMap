using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerRaycasts : MonoBehaviour
{
    public Graphic[] Targets;

    public void Trigger(bool dir)
    {
        if(Targets.Length < 1) return;
        
        foreach(var g in Targets) {
            if(g == null) continue;
            g.raycastTarget = dir;
        }
    }
}