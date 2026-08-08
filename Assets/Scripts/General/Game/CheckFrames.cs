using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CheckFrames : MonoBehaviour
{
    [SerializeField] public SpriteAnimate[] spriteAnimates;

    [SerializeField] public int targetFrame;

    private bool actionCalled = false;
    public UnityEvent OnSuccessEvent;

    public UnityEvent OnStartEvent;

    private List<SpriteAnimate> onFrameList = new List<SpriteAnimate>();

    public void Start()
    {
        actionCalled = false;
        StartCoroutine(Checking());
        onFrameList.Clear();

        OnStartEvent?.Invoke();



    }

    public IEnumerator Checking()
    {
        while(true){
        yield return new WaitForSeconds(2f);

        foreach(var s in spriteAnimates)
        {
            if(s.index == targetFrame)
            {
                onFrameList.Add(s);
            }
        }

        if(onFrameList.Count == spriteAnimates.Length)
        {
                if (!actionCalled)
                {
                    OnSuccessEvent?.Invoke();
                    actionCalled = true;
                }
        }

        onFrameList.Clear();
        }
    }
}