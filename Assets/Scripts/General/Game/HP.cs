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
        if(dead) return;
        if(type == ObjectType.Player){
            PlayerControlRigid pc = GetComponent<PlayerControlRigid>();
            Profile profile = GetComponentInChildren<Profile>();
            AudioManager.Instance.Hurt();
            float mult = dmg * 0.1f;
            pc.applyShake(1f, mult);
            DamageManager.Instance.PlayDamageOverlay(dmg * 0.025f);
            
            dmg = EffectManager.Instance.ProcessDamage(gameObject, dmg);

            if(dmg <= 0f) return;
            profile.TriggerHurt();
            currentHP -= dmg;
            HitNumberManager.Instance.DisplayHitNumber(dmg, transform, HitNumber.HitType.Damage);
            return;
        }
        else if(type == ObjectType.Enemy)
        {
            StartFlash(gameObject);
            EnemyAudio ea = GetComponentInParent<EnemyAudio>();
            if(ea != null)
            {
                ea.Hurt();
            }
        }

        HitNumberManager.Instance.DisplayHitNumber(dmg, transform, HitNumber.HitType.Damage);
        currentHP -= dmg;
    }

    public void Heal(float heal)
    {
        if(dead) return;
        float x = heal * maxHP;
        currentHP += x;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        HitNumberManager.Instance.DisplayHitNumber(x, transform, HitNumber.HitType.Heal);
    }

    public void Death()
    {
        if(type == ObjectType.Player)
        {
            if(currentHP <= 0f)
            {
                if (!dead)
                {
                    dead = true;

                    DeathManager.PlayerDead = true;

                    CrosshairManager.Instance.Deactivate();

                    TrickManager.Instance.StopFalling();

                    ReloadManager.Instance.StopReload();

                    DamageManager.Instance.PlayDamageOverlay(0f);

                    EffectManager.Instance.EffectCanvas.SetActive(false);

                    TrickManager.Instance.ResetCombo();

                    SpectrumManager.Instance.ProfileBackground.SetActive(false);

                    float cachedforce = GibsManager.Instance.explosionForce;
                    GibsManager.Instance.explosionForce = 0.125f;

                    List<Transform> gibs = GibsManager.Instance.Gib(transform.position, Random.Range(minGibs,maxGibs), false);
        
                    Camera.main.transform.parent = null;

                    Transform overlay = Camera.main.transform.GetChild(0);
                    foreach(Transform child in overlay)
                    {
                        child.gameObject.SetActive(false);
                    }

                    if(gibs!=null && gibs.Count > 0)
                    {
                        GameObject gib = gibs[Random.Range(0, gibs.Count)].gameObject;
                        gib.transform.localEulerAngles = new Vector3(0f,0f,0f);
                        if (gib.TryGetComponent<Rigidbody>(out Rigidbody rb))
                        {
                            rb.freezeRotation = true;
                        }
                        Camera.main.transform.SetParent(gib.transform);

                        GibsManager.Instance.explosionForce = cachedforce;
                    }
                }
            }    
        }

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
                    // if(pickup <= 0.45f)
                    //     {    
                    //         PickupPool.Instance.Get(transform.position, Pickup.PickupType.Health, 0.5f);

                    //         Debug.Log("health spawned");
                    //     }
                    // else if (pickup <= 0.9f && pickup > 0.45f)
                    //     {
                    //         PickupPool.Instance.Get(transform.position, Pickup.PickupType.Magic, 0.5f);

                    //         Debug.Log("Magic spawned");
                    //     }
                    if (pickup >= 0.0f)
                        {
                            PickupPool.Instance.Get(transform.position, Pickup.PickupType.Greed, 0.5f);
                        }

                    Debug.Log(pickup);
                }
                }

                if(type == ObjectType.NPC)
                    {
                    SpectrumManager.Instance.PolluteSpectrum(SpectrumManager.Instance.NPCKill);
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
