using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    public GameObject enemy;

    public HP hp;

    public Enemy script;

    public EnemySoundbank bank;

    bool spotted;

    // Start is called before the first frame update
    public virtual void Start()
    {
        script = enemy.GetComponent<Enemy>();
        hp = enemy.GetComponent<HP>();

        StartCoroutine(IdleSounds());
    }

    public virtual void Update()
    {
        Engaged();
        Death();
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

    public virtual void Death()
    {
        if(hp.currentHP <= 0){
            // death.Play();
            FMODUnity.RuntimeManager.PlayOneShot(bank.death, transform.position);
        }
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
            float newWait = Random.Range(3f,8f);
            // idle.Play();
            FMODUnity.RuntimeManager.PlayOneShot(bank.idle, transform.position);
            yield return new WaitForSeconds(newWait);
        }
    }
}
