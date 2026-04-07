using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PredictedRangedBehavior : AttackBehavior
{

public override float Fire()
    {
        return Cooldown;
    }
}
