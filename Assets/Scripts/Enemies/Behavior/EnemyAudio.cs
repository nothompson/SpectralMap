using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    public GameObject enemy;

    public HP hp;

    public Enemy script;

    public EnemySoundbank bank;

    public bool engage = true;

    bool spotted;

    bool dead = false;

    // Start is called before the first frame update
    public virtual void Start()
    {
        script = enemy.GetComponent<Enemy>();
        hp = enemy.GetComponent<HP>();

        StartCoroutine(IdleSounds());
        dead = false;
    }

    public virtual void Update()
    {
        if(engage){
        Engaged();
        }
    }

    public virtual void Footstep()
    {
        if (script.grounded && !script.attacking)
        {
            // step.Play();
            FMODUnity.RuntimeManager.PlayOneShot(bank.step, transform.position);
        }
    }

    public virtual void Hurt()
    {
        // hurt.Play();
        FMODUnity.RuntimeManager.PlayOneShot(bank.hurt, transform.position);
    }

    public virtual void Attacking()
    {
        // attacking.Play();
        FMODUnity.RuntimeManager.PlayOneShot(bank.attacking, transform.position);
    }

    public virtual void Attack()
    {
        // attack.Play();
        FMODUnity.RuntimeManager.PlayOneShot(bank.attack, transform.position);
    }

     public virtual void Attack2()
    {
        // attack.Play();
        FMODUnity.RuntimeManager.PlayOneShot(bank.attack2, transform.position);
    }

      public virtual void Attack3()
    {
        // attack.Play();
        FMODUnity.RuntimeManager.PlayOneShot(bank.attack3, transform.position);
    }


    public virtual void Death()
    {
        // if(hp.currentHP <= 0 && !dead){
        //     // death.Play();
        //     // FMODUnity.RuntimeManager.PlayOneShot(bank.death, transform.position);

        //     AudioManager.Instance.EnemyDeathSound(bank.death, transform);
        //     dead = true;
        // }
    }
    
    public virtual void Engaged()
    {
        if (script.engage && !spotted)
        {
            // agro.Play();
            FMODUnity.RuntimeManager.PlayOneShot(bank.agro, transform.position);
        }
        spotted = script.engage;
    }

    public virtual IEnumerator IdleSounds()
    {
        while (true)
        {
            float newWait = Random.Range(6f,15f);
            // idle.Play();
            FMODUnity.RuntimeManager.PlayOneShot(bank.idle, transform.position);
            yield return new WaitForSeconds(newWait);
        }
    }
}
