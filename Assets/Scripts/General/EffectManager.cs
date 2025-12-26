using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager effectManager;

    private Dictionary<GameObject, Effect> debuffKey = new Dictionary<GameObject, Effect>();
    private Dictionary<GameObject, Effect> buffKey = new Dictionary<GameObject, Effect>();
    private void Awake()
    {
        if(effectManager == null)
        {
            effectManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        Debuffs();
        Buffs();
    }

    #region Effect Class
    private class Effect
    {
        //references
        public PlayerControlRigid pc;
        public HP hp;
        public Launcher launch;
        public Fireball fireball;
        public MagicManagement magic;
        public Enemy e;
        public MeleeCollider melee;
        public EnemyProjectile projectile;
        public Rocket rocket;

        public bool player;
        //movement effects
        public float slowTimer;
        public float slowMultiplier;
        public bool slowed;
        public float boostTimer;
        public float boostMultiplier;
        public bool boosted;
        // combat effects
        public float rageTimer;
        public float rageMultiplier;
        public bool enraged;
        public float weakTimer;
        public float weakMultiplier;
        public bool weakened;
        //copies of og 
        float speed;
        float jump;
        float attackSpeed;
        float projSpeed;
        float damage;
        float force;
        float forceMultiplier;
        float maxHP;
        float currentHP;

        //states 
        public float confuseTimer;
        public bool confused;

        public void Spawn(GameObject target, float duration, float multiplier)
        {
            if (target.CompareTag("Player"))
            {
                pc = target.GetComponent<PlayerControlRigid>();
                if (pc == null) return;
                player = true;
                //grab references
                hp = target.GetComponent<HP>();
                launch = target.GetComponentInChildren<Launcher>();
                magic = target.GetComponent<MagicManagement>();

                //copy of initial parameters 
                speed = pc.moveSpeed;
                jump = pc.jumpHeight;

                maxHP = hp.maxHP;
                currentHP = hp.currentHP;

                projSpeed = launch.fireballSpeed;
            }
            else
            {
                e = target.GetComponent<Enemy>();
                if (e == null) return;
                player = false;
                hp = target.GetComponent<HP>();
                if (e.attackType < 2)
                {
                    GameObject meleeRef = e.attackPrefab;
                    melee = meleeRef.GetComponent<MeleeCollider>();
                }
                else
                {
                    GameObject projRef = e.attackPrefab;
                    projectile = projRef.GetComponent<EnemyProjectile>();
                }

                //copy of initial parameters 
                speed = e.moveSpeed;
                jump = e.jumpHeight;
                attackSpeed = e.attackingCooldown;

                currentHP = hp.currentHP;
                maxHP = hp.maxHP;

                damage = e.damage;
                if (melee != null)
                {
                    forceMultiplier = melee.forceMultiplier;
                }

            }

            //slow
            slowTimer = duration;
            slowMultiplier = multiplier;

            //confuse
            confuseTimer = duration;

            //speed
            boostTimer = duration;
            boostMultiplier = multiplier;

            //rage
            rageTimer = duration;
            rageMultiplier = multiplier;

            weakTimer = duration;
            weakMultiplier = multiplier;
        }
        #endregion

        #region Slow
        public void ApplySlow()
        {

            if (!slowed) return;

            if (player && pc != null)
            {
                pc.moveSpeed = speed * slowMultiplier;
                pc.jumpHeight = jump * slowMultiplier;
            }

            else if (e != null)
            {
                e.moveSpeed = speed * slowMultiplier;
                e.jumpHeight = jump * slowMultiplier;
                e.attackingCooldown = attackSpeed / slowMultiplier;
            }
        }

        public void ClearSlow()
        {
            if (player && pc != null)
            {
                pc.moveSpeed = speed;
                pc.jumpHeight = jump;
            }
            else if (e != null)
            {
                e.moveSpeed = speed;
                e.jumpHeight = jump;
                e.attackingCooldown = attackSpeed;
            }
            slowed = false;
        }
        #endregion
        #region Boost
        public void ApplyBoost()
        {
            if (!boosted) return;

            if (player && pc != null && launch != null)
            {
                pc.moveSpeed = speed * boostMultiplier;
                launch.shootMultiplier = boostMultiplier;
                launch.fireballSpeed = projSpeed * boostMultiplier;
            }
            else if (e != null)
            {
                e.moveSpeed = speed * boostMultiplier;
                e.attackingCooldown = attackSpeed / boostMultiplier;

                if (projectile != null)
                {
                    e.projSpeedMultiplier = boostMultiplier;
                }
            }
        }

        public void ClearBoost()
        {
            if (player && pc != null && launch != null)
            {
                pc.moveSpeed = speed;
                launch.shootMultiplier = 1f;
                launch.fireballSpeed = projSpeed * 1f;
            }
            else if (e != null)
            {
                e.moveSpeed = speed;
                e.attackingCooldown = attackSpeed;
                e.projSpeedMultiplier = 1f;
            }
            boosted = false;
        }
        #endregion

        #region Confuse
        public void ApplyConfuse()
        {
            if (!confused) return;

            if (player && pc != null)
            {
                pc.confused = true;
            }
        }

        public void ClearConfuse()
        {
            if (player && pc != null)
            {
                pc.confused = false;
            }

            confused = false;
        }
        #endregion

        #region Weak
        public void ApplyWeak()
        {
            if (!weakened) return;

            float weakSpeed = Mathf.Pow(weakMultiplier, weakMultiplier);
            if (player && pc != null && launch != null && hp != null)
            {
                pc.moveSpeed = speed * weakSpeed + 0.1f;
                launch.forceMultiplier = weakMultiplier;
                launch.damageMultiplier = weakMultiplier;
                hp.currentHP -= 0.05f / weakMultiplier;
            }

            else if (e != null && hp != null)
            {
                hp.currentHP -= 0.05f / weakMultiplier;
                e.moveSpeed = speed * weakSpeed + 0.1f;
                e.damage = damage * weakMultiplier;
                e.forceMultiplier = weakMultiplier;
            }

        }
        public void ClearWeak()
        {
            if (player && pc != null && launch != null)
            {
                pc.moveSpeed = speed;
                launch.forceMultiplier = 1f;
                launch.damageMultiplier = 1f;
            }
            else if (e != null)
            {
                e.moveSpeed = speed;
                e.damage = damage;
                e.forceMultiplier = 1f;
            }
        }
        #endregion

        #region Rage
        public void ApplyRage()
        {

            if (!enraged) return;

            if (player && hp != null && launch != null)
            {
                launch.forceMultiplier = rageMultiplier * 0.5f;
                launch.damageMultiplier = rageMultiplier;
                hp.currentHP = currentHP * rageMultiplier;
                if (hp.currentHP * rageMultiplier > maxHP)
                {
                    hp.maxHP = currentHP * rageMultiplier;
                }
            }

            else if (e != null)
            {
                if (hp != null)
                {
                    e.damage = damage * rageMultiplier;
                    e.forceMultiplier = rageMultiplier;
                    hp.currentHP = currentHP * rageMultiplier;
                    if (hp.currentHP * rageMultiplier > maxHP)
                    {
                        hp.maxHP = currentHP * rageMultiplier;
                    }
                }
            }
        }
        public void ClearRage()
        {
            if (player && hp != null && launch != null)
            {
                launch.forceMultiplier = 1f;
                launch.damageMultiplier = 1f;
                hp.maxHP = maxHP;
                if (hp.currentHP * rageMultiplier > maxHP)
                {
                    hp.currentHP = hp.maxHP;
                }
            }

            else if (e != null)
            {
                e.damage = damage;
                e.forceMultiplier = 1f;
                e.projSpeedMultiplier = 1f;
                hp.maxHP = maxHP;
                if (hp.currentHP * rageMultiplier > maxHP)
                {
                    hp.currentHP = hp.maxHP;
                }
            }
            enraged = false;
        }
        #endregion
    }
    
    #region Debuffs
    public void Slow(GameObject target, float duration, float multiplier)
    {
        if (buffKey.TryGetValue(target, out Effect buff))
        {
            buff.ClearBoost();
            buffKey.Remove(target);
        }

        if (!debuffKey.ContainsKey(target))
        {
            Effect debuff = new Effect();
            debuff.Spawn(target, duration, multiplier);
            debuff.slowed = true;
            debuffKey[target] = debuff;
        }
    }

    public void Confuse(GameObject target, float duration, float multiplier)
    {
        if (!debuffKey.ContainsKey(target))
        {
            Effect debuff = new Effect();
            debuff.Spawn(target, duration, multiplier);
            debuff.confused = true;
            debuffKey[target] = debuff;
        }
    }

    public void Weak(GameObject target, float duration, float multiplier)
    {
        if (buffKey.TryGetValue(target, out Effect buff))
        {
            buff.ClearRage();
            buffKey.Remove(target);
        }

        if (!debuffKey.ContainsKey(target))
        {
            Effect debuff = new Effect();
            debuff.Spawn(target, duration, multiplier);
            debuff.weakened = true;
            debuffKey[target] = debuff;
        }
    }

    public void Debuffs()
    {
        List<GameObject> removed = new List<GameObject>();

        foreach (var dbf in debuffKey)
        {
            var target = dbf.Key;
            var debuff = dbf.Value;

            //Slow
            if (debuff.slowed)
            {
                debuff.slowTimer -= Time.deltaTime;
                if (debuff.slowTimer > 0)
                {
                    debuff.ApplySlow();
                }
                else
                {
                    debuff.ClearSlow();
                    removed.Add(target);
                }
            }

            //Confuse
            if (debuff.confused)
            {
                debuff.confuseTimer -= Time.deltaTime;
                if (debuff.confuseTimer > 0)
                {
                    debuff.ApplyConfuse();
                }
                else
                {
                    debuff.ClearConfuse();
                    removed.Add(target);
                }
            }
            //Weaken
            if (debuff.weakened)
            {
                debuff.weakTimer -= Time.deltaTime;
                if (debuff.weakTimer > 0)
                {
                    debuff.ApplyWeak();
                }
                else
                {
                    debuff.ClearWeak();
                    removed.Add(target);
                }
            }
        }

        //remove debuff when timer is up
        foreach (var t in removed)
        {
            debuffKey.Remove(t);
        }
    }
    #endregion

    #region Buffs
    public void Boost(GameObject target, float duration, float multiplier)
    {
        if (debuffKey.TryGetValue(target, out Effect debuff))
        {
            debuff.ClearSlow();
        }

        if (!buffKey.TryGetValue(target, out Effect boost))
        {
            boost = new Effect();
            boost.Spawn(target, duration, multiplier);
            buffKey[target] = boost;
        }

        if (!boost.boosted) 
        {
            boost.boosted = true;
        }
    
    }

    public void Rage(GameObject target, float duration, float multiplier)
    {
        //implement after making weak debuff 

        // if (debuffKey.TryGetValue(target, out Effect debuff))
        // {
        //     debuff.ClearWeak();
        //     debuffKey.Remove(target);
        // }
        
        if (!buffKey.TryGetValue(target, out Effect rage))
        {
            rage = new Effect();
            rage.Spawn(target, duration, multiplier);
            buffKey[target] = rage;
        }

        if (!rage.enraged)
        {
            rage.enraged = true;
        }
    }
    
    public void Buffs()
    {
        List<GameObject> removed = new List<GameObject>();
        foreach (var bff in buffKey)
        {
            var target = bff.Key;
            var buff = bff.Value;

            if (buff.boosted)
            {
                buff.boostTimer -= Time.deltaTime;
                if (buff.boostTimer > 0)
                {
                    buff.ApplyBoost();
                }
                else
                {
                    buff.ClearBoost();
                    removed.Add(target);
                }
            }

            if (buff.enraged)
            {
                buff.rageTimer -= Time.deltaTime;
                if (buff.rageTimer > 0)
                {
                    buff.ApplyRage();
                }
                else
                {
                    buff.ClearRage();
                    removed.Add(target);
                }
            }

        }
        foreach (var t in removed)
        {
            buffKey.Remove(t);
        }
    }
    #endregion
}

