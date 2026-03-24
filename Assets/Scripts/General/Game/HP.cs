using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{

    [Header("Health Stats")]
    public float currentHP;
    public float maxHP;
    [Header("State")]
    public string characterID;
    public bool dead = false;
    public ObjectType type;

    public enum ObjectType
    {
        Player,
        Enemy,
        NPC
    }

    [Header("Gibs")]
    [SerializeField] private int minGibs;
    [SerializeField] private int maxGibs;

    [Header("Damage Flash")]
    [SerializeField] private bool damageFlashEnable;
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

        AssignID();
    }

    void AssignID()
    {
        NPC npc = GetComponentInParent<NPC>();
        if(npc != null)
        {
            characterID = npc.npcID;
        }

        Enemy enemy = GetComponentInParent<Enemy>();
        if(enemy != null)
        {
            if(!string.IsNullOrEmpty(enemy.EnemyID)){
            characterID = enemy.EnemyID;
            }
        }
    }

    void Update()
    {
        Death();
    }

    public void Damage(float dmg)
    {
        currentHP -= dmg;
        if(type == ObjectType.Player){
            PlayerControlRigid pc = GetComponentInParent<PlayerControlRigid>();
            Profile profile = GetComponentInChildren<Profile>();
            AudioManager.Instance.Hurt();
            float mult = dmg * 0.1f;
            // Debug.Log(dmg);
            pc.applyShake(1f, mult);
            DamageManager.Instance.PlayDamageOverlay(dmg * 0.025f);
            profile.TriggerHurt();
        }
        else if(type == ObjectType.Enemy)
        {
            StartFlash(gameObject);
            EnemyAudio ea = GetComponentInParent<EnemyAudio>();
            if(ea != null)
            {
                ea.Hurt();
            }
            HitNumberManager.Instance.DisplayHitNumber(dmg, transform);
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

    public void Death()
    {

        if(type == ObjectType.Enemy || type == ObjectType.NPC){
        if(currentHP <= 0f)
        {
            if (!dead)
            {
                dead = true;


                Enemy e = GetComponentInParent<Enemy>();
                if(e != null){
                float roll = Random.Range(0f,1f);
                Debug.Log(roll);
                if(roll >= 1.0f - e.ChanceToDropPickup){
                    float pickup = Random.Range(0f,1f);
                    if(pickup <= 0.45f)
                        {    
                            PickupPool.Instance.Get(transform.position, Pickup.PickupType.Health, 0.5f);

                            Debug.Log("health spawned");
                        }
                    else if (pickup <= 0.9f && pickup > 0.45f)
                        {
                            PickupPool.Instance.Get(transform.position, Pickup.PickupType.Magic, 0.5f);

                            Debug.Log("Magic spawned");
                        }
                    else if (pickup > 0.9f)
                        {
                            Debug.Log("pot of greed spawned!");
                        }

                    Debug.Log(pickup);
                }
                }

                GibsManager.Instance.Gib(transform.position, Random.Range(minGibs,maxGibs));
                    if (!string.IsNullOrEmpty(characterID))
                    {
                        DeathManager.Instance.SetDead(characterID);
                    }
                Destroy(gameObject);
            }
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
        if(!damageFlashEnable) return;
        if(flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DamageFlash(damaged));
    }

    public IEnumerator DamageFlash(GameObject damaged)
    {
        SkinnedMeshRenderer skin = damaged.GetComponentInChildren<SkinnedMeshRenderer>();
        if(!skin) yield break;

        Material mat = skin.material;

        if(!mat.HasProperty("_EmissionColor")) yield break;

            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

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
