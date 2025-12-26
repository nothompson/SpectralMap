using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : SpriteUI
{
    private HP hp;

    override public void Awake()
    {
        hp = GetComponentInParent<HP>();
        base.Awake();
    }
    
    void Update()
    {
        Calculate(hp.currentHP, hp.maxHP, 2f, 1f, 10f, true);
    }
}
