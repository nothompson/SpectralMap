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
    }

    public void AttackSound()
    {
        enemyAudio.Attack();
    }

    public void AttackSound2()
    {
        enemyAudio.Attack2();
    }

    
    public void AttackSound3()
    {
        enemyAudio.Attack3();
    }

    public void EndAttack()
    {
        enemy.EndAttack();
    }

    public void Footstep()
    {
        enemyAudio.Footstep();
    }

}
