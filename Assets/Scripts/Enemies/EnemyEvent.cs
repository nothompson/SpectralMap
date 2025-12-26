using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvent : MonoBehaviour
{
    public Enemy enemy;

    public EnemyAudio enemyAudio;

    public void BeginAttack()
    {
        enemyAudio.Attacking();
    }

    public void OnAttack()
    {
        enemy.OnAttack();
        enemyAudio.Attack();
    }

    public void Footstep()
    {
        enemyAudio.Footstep();
    }

}
