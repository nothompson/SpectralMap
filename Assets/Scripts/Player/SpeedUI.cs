using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedUI : SpriteUI
{
    private PlayerControlRigid pc;

    public Animator heartAnim;

    override public void Awake()
    {
        pc = GetComponentInParent<PlayerControlRigid>();
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        float speedmult = pc.playerSpeed * 0.075f;

        heartAnim.SetFloat("SpeedMultiplier", speedmult + 1f);

        Calculate(pc.playerSpeed, 35f, 0.8f, 30, 5, false, cap: false);
    }
}
