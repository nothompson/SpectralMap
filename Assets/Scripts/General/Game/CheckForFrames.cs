using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class CheckForFrames : MonoBehaviour
{
    private bool actionCalled;
    public UnityEvent Event;

    public SpriteAnimate[] SpriteAnimates;

    public int TargetFrame;

    public void Start()
    {
        actionCalled = false;
        StartCoroutine(Checking());
    }

    IEnumerator Checking()
    {
        yield return new WaitForSeconds(1f);

        foreach(var s in SpriteAnimates)
        {
            if(s.index == TargetFrame)
            {
                Debug.Log("hit");
            }
        }
    }

}