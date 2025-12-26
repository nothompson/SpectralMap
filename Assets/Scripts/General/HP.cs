using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{

    public float currentHP;
    public float maxHP;

    [Range(1, 2)]
    public int type;

    // Start is called before the first frame update
    void Start()
    {
        if (maxHP <= 0f)
        {
            maxHP = 100f;
        }
        currentHP = maxHP;
    }

    public void Damage(float dmg)
    {
        currentHP -= dmg;
        if(type == 1){
            PlayerControlRigid pc = GetComponentInParent<PlayerControlRigid>();
            AudioManager.Instance.Hurt();
            float mult = dmg * 0.05f;
            // Debug.Log(dmg);
            pc.applyShake(1f, mult);
        }
        else
        {
            EnemyAudio ea = GetComponentInParent<EnemyAudio>();
            if(ea != null)
            {
                ea.Hurt();
            }
        }
    }

    public void Heal(float heal)
    {
        currentHP += heal * maxHP;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public IEnumerator DeathRoutine()
    {
        float del = 0.25f;
        WaitForSeconds wait = new WaitForSeconds(del);
        while (true)
        {
            yield return wait;
            DeathCheck();
        }
    }

    public void DeathCheck()
    {
        if (currentHP <= 0)
        {
            if (type == 2)
            {
                Destroy(gameObject);
            }
        }
    }

    public bool Critical()
    {
        float low = maxHP * 0.2f;
        if (currentHP <= low)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
