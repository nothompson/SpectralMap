using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{

    public float currentHP;
    public float maxHP;

    [Range(1, 2)]
    public int type;
    [Header("Damage Flash")]
    [SerializeField] private float targetFlashValue;
    [SerializeField] private float flashDur;
    [SerializeField] private AnimationCurve FlashCurve;
    private Coroutine flashRoutine;

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
            StartFlash(gameObject);
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

    
    public void StartFlash(GameObject damaged)
    {
        if(flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DamageFlash(damaged));
    }

    public IEnumerator DamageFlash(GameObject damaged)
    {
        SkinnedMeshRenderer skin = damaged.GetComponentInChildren<SkinnedMeshRenderer>();
        Material mat = skin.material;

            mat.EnableKeyword("_EMISSION");

            Color emissionColor = mat.GetColor("_EmissionColor");
            
            float t = 0f;

            while(t < flashDur)
            {
                t += Time.deltaTime;

                float elapsed = Mathf.Clamp01(t / flashDur);
                float value = FlashCurve.Evaluate(elapsed);

                emissionColor.r = targetFlashValue * value;
                emissionColor.g = targetFlashValue * value;
                emissionColor.b = targetFlashValue * value;

                mat.SetColor("_EmissionColor", emissionColor);
                
                yield return null; 
            }
    }




}
