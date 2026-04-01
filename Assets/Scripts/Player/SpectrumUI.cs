using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpectrumUI : SpriteUI
{
    public bool waiting = true;
    override public void Awake()
    {
        // base.Awake();
        // SyncToCurrentLevel();
        Debug.Log("spectrum awake");
    }

    public void OnShow()
    {
        lastvalue = -1f;
        waiting = false;
        Debug.Log(SpectrumManager.Instance.PollutantLevel);
    }

    public void SyncToCurrentLevel()
    {
        if(spriteAnimate == null || spriteAnimate.sprites == null || spriteAnimate.sprites.Length == 0) return;
        float normal = Mathf.Clamp01(1f - ((float)SpectrumManager.Instance.PollutantLevel / SpectrumManager.Instance.MaxPollutantLevel));
        int index = Mathf.FloorToInt(normal * (spriteAnimate.sprites.Length - 1));
        spriteAnimate.startingIndex = index;
        spriteAnimate.SetFrame(index);
        lastindex = spriteAnimate.sprites.Length - 1;
        lastvalue = SpectrumManager.Instance.PollutantLevel;
    }

    void Update()
    {
        if(waiting) return;
        Calculate(SpectrumManager.Instance.PollutantLevel, SpectrumManager.Instance.MaxPollutantLevel, 0f, 10f, 8f, true);
    }
}
