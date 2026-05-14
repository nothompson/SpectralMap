using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;
    
    public GameObject EffectCanvas; 
    public GameObject FleshSuitCanvas; 
    public SpriteUI[] FleshSuitHuds;

    public GridLayoutGroup EffectGrid;

    public GameObject effectUIPrefab;

    public AnimationCurve ScaleCurve;

    public AnimationCurve FadeCurve;

    public Dictionary<GameObject, EffectContainer> Effects = new();

    public List<EffectUIData> effectUIDataList;
    private Dictionary<string, EffectUIData> lookup;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            lookup = effectUIDataList.ToDictionary(x => x.id);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #region General
    public interface IEffect
    {
        string EffectID{get;}
        void OnApply();
        void OnRemove();
        void Tick(float deltaTime);
        bool isFinished {get;}
    }

    public class EffectContainer
    {
        public List<IEffect> activeEffects = new();

        public Dictionary<IEffect, GameObject> uiMap = new();
        public T GetEffect<T>() where T : class, IEffect
        {
            return activeEffects.OfType<T>().FirstOrDefault();
        }
    }

    public void AddEffect(IEffect effect, GameObject target, float duration = 1f)
        {
            var container = Get(target);

            container.activeEffects.Add(effect);
            effect.OnApply();


            var data = GetUIData(effect.EffectID);
            if(data == null) return;

            GameObject ui = GameObject.Instantiate(effectUIPrefab, EffectGrid.transform);

            ui.SetActive(false);

            EffectUI effectUI = ui.GetComponent<EffectUI>();

            effectUI.EffectDuration = duration;
            
            SpriteAnimate uiSprites = ui.GetComponent<SpriteAnimate>();

            SpriteText text = ui.GetComponentInChildren<SpriteText>();

            text.input = data.DisplayName;

            text.Refresh();

            uiSprites.sprites = data.sprites;

            uiSprites.length = data.sprites.Length;

            uiSprites.fps = data.fps;

            uiSprites.pingPong = data.pingPong;

            container.uiMap[effect] = ui;

            ui.SetActive(true);
        }

        public void RemoveEffect(IEffect effect, GameObject target)
        {
            var container = Get(target);
            if(!container.activeEffects.Contains(effect)) return;

            effect.OnRemove();
            container.activeEffects.Remove(effect);

            if(container.uiMap.TryGetValue(effect, out var ui))
            {
                Destroy(ui);
                container.uiMap.Remove(effect);
            }
        }

    public EffectContainer Get(GameObject go)
    {
        if(!Effects.TryGetValue(go, out var e))
        {
            e = new EffectContainer();
            Effects[go] = e;
        }
        return e;
    }

    public void Update()
    {
        bool hasFlesh = false;

        foreach(var kvp in Effects)
        {
            var target = kvp.Key;
            var container = kvp.Value;

            for(int i = container.activeEffects.Count - 1; i >= 0; i--)
            {
                var e = container.activeEffects[i];
                e.Tick(Time.deltaTime);

                if (e.isFinished)
                {
                    RemoveEffect(e, target);
                }
            }

            var flesh = container.GetEffect<FleshSuitEffect>();
                if(flesh != null)
                {
                    hasFlesh = true;
                    FleshSuitHuds[0].Calculate(flesh.CurrentHP, flesh.MaxHP, 0f,0f,0f, true);
                    FleshSuitHuds[1].Calculate(flesh.CurrentHP, flesh.MaxHP, 0f,0f,0f, true);
                }
            
            FleshSuitCanvas.SetActive(hasFlesh);
        }
    }

    public float ProcessDamage(GameObject target, float dmg)
    {
        var container = Get(target);

        var flesh = container.GetEffect<FleshSuitEffect>();
        if (flesh != null)
        {
            float absorbed = flesh.Absorb(dmg);
            dmg -= absorbed;
        }
        return dmg;
    }

    public EffectUIData GetUIData(string id)
    {
        lookup.TryGetValue(id, out var data);
        return data;
    }

    #endregion

    #region PotOfGreed

    public void PotOfGreed(GameObject target)
    {
        var pool = new System.Action[]
        {
            () => Confuse(target,5f),
            () => Guilt(target,10f),
            () => Ectoplasm(target,15f),
            () => Shapeless(target,15f),
            () => Transience(target,15f),
            () => Overgrowth(target, 15f),
            () => Polluted(target, 10f),
            () => Infected(target, 10f),
        };

        int index = Random.Range(0, pool.Length);
        pool[index].Invoke();
    }
    #endregion

    #region Confuse
    public class ConfuseEffect: IEffect
    {
        public string EffectID => "Confuse";
        private PlayerControlRigid pc;
        private float duration;
        private float timer;
        public bool isFinished => timer <= 0f;

        public ConfuseEffect(GameObject target, float duration)
        {
            this.duration = duration;
            this.timer = duration;

            pc = target.GetComponent<PlayerControlRigid>();
        }

        public void OnApply()
        {
            if(pc != null)
            {
                pc.confused = true;
            }
        }

        public void Tick(float dt)
        {
            timer -= dt;
        }

        public void OnRemove()
        {
            if(pc!= null)
            {
                pc.confused = false;
            }
        }
    }
    public void Confuse(GameObject target, float duration)
    {
        var container = Get(target);

        var existing = container.GetEffect<ConfuseEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new ConfuseEffect(target, duration), target, duration);
    }
    #endregion

    #region Guilt

    public class GuiltEffect: IEffect
    {
        public string EffectID => "Guilt";
        private Launcher launcher;

        private float duration;
        private float timer;
        public bool isFinished => timer <= 0f;

        private float cachedDamageMult;
        private float cachedForceMult;

        public GuiltEffect(GameObject target, float duration)
        {
            this.duration = duration;
            this.timer = duration;
            launcher = target.GetComponentInChildren<Launcher>();
        }

        public void OnApply()
        {
            cachedDamageMult = PlayerManager.Instance.DamageMultiplier;
            cachedForceMult = PlayerManager.Instance.ForceMultiplier;
        }
        public void Tick(float dt)
        {
            timer -= dt;
            PlayerManager.Instance.DamageMultiplier = 0f;
            PlayerManager.Instance.ForceMultiplier = 2.5f;
        }
        public void OnRemove()
        {
            PlayerManager.Instance.DamageMultiplier = cachedDamageMult;
            PlayerManager.Instance.ForceMultiplier = cachedForceMult;
            PlayerManager.Instance.CheckItems();
        }
    }

    public void Guilt(GameObject target, float duration)
    {
        var container = Get(target);

        var existing = container.GetEffect<GuiltEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new GuiltEffect(target, duration), target, duration);
    }

    #endregion

    #region Ectoplasm

    public class EctoplasmEffect : IEffect
    {
        public string EffectID => "ectoplasm";

        public Launcher launcher;
        private float duration;
        private float timer;
        private float cachedCostMult; 
        public bool isFinished => timer <= 0f;

        public EctoplasmEffect(GameObject target, float duration)
        {
            launcher = target.GetComponentInChildren<Launcher>();
            this.duration = duration;
            this.timer = duration;
        }

        public void OnApply()
        {
            if(launcher != null)
            {
                cachedCostMult = launcher.costMultiplier;

            }
        }

        public void Tick(float dt)
        {
            timer -= dt;
            if(launcher != null)
            {
                launcher.costMultiplier = 0f;
            }
        }

        public void OnRemove()
        {
            if(launcher != null)
            {
                launcher.costMultiplier = cachedCostMult;
            }
        }

    }

    public void Ectoplasm(GameObject target, float duration)
    {
        var container = Get(target);

        var existing = container.GetEffect<EctoplasmEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new EctoplasmEffect(target, duration), target, duration);
    }

    #endregion

    #region Transience

    public class TransienceEffect : IEffect
    {
        public string EffectID => "transience";

        public Launcher launcher;
        public PlayerControlRigid pc;
        private float duration;
        private float timer;

        private float cachedJumpHeight;
        private float cachedMoveSpeed;
        private float cachedDamageMult;

        private float newDamageMult;
        private float cachedForceMult;
        public bool isFinished => timer <= 0f;

        public TransienceEffect(GameObject target, float duration)
        {
            launcher = target.GetComponentInChildren<Launcher>();
            pc = target.GetComponent<PlayerControlRigid>();
            this.duration = duration;
            this.timer = duration;
        }

        public void OnApply()
        {
            cachedJumpHeight = pc.jumpHeight;
            cachedMoveSpeed = pc.moveSpeed;
            cachedDamageMult = PlayerManager.Instance.DamageMultiplier;
            cachedForceMult = PlayerManager.Instance.ForceMultiplier;
            newDamageMult = cachedDamageMult + 0.5f;
        }
        public void Tick(float dt)
        {
            timer -= dt;
            PlayerManager.Instance.DamageMultiplier = newDamageMult;
            PlayerManager.Instance.ForceMultiplier = 1.5f;
            pc.jumpHeight = cachedJumpHeight + 2f;
            pc.moveSpeed = cachedMoveSpeed + 10f;
        }
        public void OnRemove()
        {
            pc.jumpHeight = cachedJumpHeight;
            pc.moveSpeed = cachedMoveSpeed;
            PlayerManager.Instance.DamageMultiplier = cachedDamageMult;
            PlayerManager.Instance.ForceMultiplier = cachedForceMult;
            PlayerManager.Instance.CheckItems();
        }

    }

    public void Transience(GameObject target, float duration)
    {
        var container = Get(target);

        var existing = container.GetEffect<TransienceEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new TransienceEffect(target, duration), target, duration);
    }

    #endregion

    #region Shapeless

    public class ShapelessEffect : IEffect
    {
        public string EffectID => "shapeless";

        private HP hp;
        private float duration;
        private float timer;
        public bool isFinished => timer <= 0f;

        public ShapelessEffect(GameObject target, float duration)
        {
            hp = target.GetComponent<HP>();
            this.duration = duration;
            this.timer = duration;
        }

        public void OnApply()
        {
            if(hp != null)
            {
                hp.immune = true;
            }
        }

        public void Tick(float dt)
        {
            timer -= dt;
        }

        public void OnRemove()
        {
            if(hp != null){
                hp.immune = false;
            }
        }
    }

    public void Shapeless(GameObject target, float duration)
    {
        var container = Get(target);

        var existing = container.GetEffect<ShapelessEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new ShapelessEffect(target, duration), target, duration);
    }

    #endregion

    #region Overgrowth

    public class OvergrowthEffect : IEffect
    {
        public string EffectID => "Overgrowth";

        private HP hp;

        private MagicManagement mana;
        private float duration;
        private float timer;

        private float hpPerSecond => 5f;
        private float manaPerSecond => 10f;
        public bool isFinished => timer <= 0f;

        public OvergrowthEffect(GameObject target, float duration)
        {
            hp = target.GetComponent<HP>();
            mana = target.GetComponent<MagicManagement>();
            this.duration = duration;
            this.timer = duration;
        }

        public void OnApply()
        {
            
        }

        public void Tick(float dt)
        {
            timer -= dt;
            hp.currentHP = Mathf.Min(hp.currentHP + hpPerSecond * dt, hp.maxHP);
            mana.magicPoints = Mathf.Min(mana.magicPoints + manaPerSecond * dt, mana.maximumMagic);
        }

        public void OnRemove()
        {
            
        }
    }

    public void Overgrowth(GameObject target, float duration)
    {
        var container = Get(target);

        var existing = container.GetEffect<OvergrowthEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new OvergrowthEffect(target, duration), target, duration);
    }

    #endregion

    #region Polluted

    public class PollutedEffect : IEffect
    {
        public string EffectID => "Polluted";
        private HP hp;
        private float damagePerTick => 2.5f;
        private float tickAccumulator;
        private float duration;
        private float timer;
        public bool isFinished => timer <= 0f;

        public PollutedEffect(GameObject target, float duration)
        {
            hp = target.GetComponent<HP>();
            this.timer = duration;
            this.duration = duration;
        }

        public void OnApply()
        {
            
        }

        public void Tick(float dt)
        {
            timer -= dt;
            tickAccumulator += dt;

            if(tickAccumulator >= 1f)
            {
                tickAccumulator -= 1f;
                hp.Damage(damagePerTick);
            }
        }

        public void OnRemove()
        {

        }
    }

    public void Polluted(GameObject target, float duration)
    {
       var container = Get(target);

        var existing = container.GetEffect<PollutedEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new PollutedEffect(target, duration), target, duration); 
    }

   
    #endregion

    #region Infected

    public class InfectedEffect : IEffect
    {
        public string EffectID => "Infection";        
        public Launcher launcher;
        public PlayerControlRigid pc;
        private float duration;
        private float timer;

        private float cachedJumpHeight;
        private float cachedMoveSpeed;
        private float cachedDamageMult;

        private float newDamageMult;
        private float cachedForceMult;
        public bool isFinished => timer <= 0f;

        public InfectedEffect(GameObject target, float duration)
        {
            launcher = target.GetComponentInChildren<Launcher>();
            pc = target.GetComponent<PlayerControlRigid>();
            this.duration = duration;
            this.timer = duration;
        }

        public void OnApply()
        {
            cachedJumpHeight = pc.jumpHeight;
            cachedMoveSpeed = pc.moveSpeed;
            cachedDamageMult = PlayerManager.Instance.DamageMultiplier;
            cachedForceMult = PlayerManager.Instance.ForceMultiplier;
            newDamageMult = cachedDamageMult - 0.5f;
        }
        public void Tick(float dt)
        {
            timer -= dt;
            PlayerManager.Instance.DamageMultiplier = newDamageMult;
            PlayerManager.Instance.ForceMultiplier = 0.75f;
            pc.jumpHeight = cachedJumpHeight - 3f;
            pc.moveSpeed = cachedMoveSpeed - 4f;
        }
        public void OnRemove()
        {
            pc.jumpHeight = cachedJumpHeight;
            pc.moveSpeed = cachedMoveSpeed;
            PlayerManager.Instance.DamageMultiplier = cachedDamageMult;
            PlayerManager.Instance.ForceMultiplier = cachedForceMult;
            PlayerManager.Instance.CheckItems();
        }
    }

    public void Infected(GameObject target, float duration)
    {
       var container = Get(target);

        var existing = container.GetEffect<InfectedEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new InfectedEffect(target, duration), target, duration); 
    }
    
    #endregion

    #region TongueTied

    public class EnsareEffect : IEffect
    {
        public string EffectID => "Ensare";
        public PlayerControlRigid pc;
        private float duration;
        private float timer;
        public bool isFinished => timer <= 0f;

        private float cachedJumpHeight;
        private float cachedMoveSpeed;

        public EnsareEffect(GameObject target, float duration)
        {
            pc = target.GetComponent<PlayerControlRigid>();
            this.duration = duration;
            this.timer = duration;
        }

        public void OnApply()
        {
            if(pc != null)
            {
                cachedJumpHeight = pc.jumpHeight;
                cachedMoveSpeed = pc.moveSpeed;
                pc.ensared = true;
                pc.grounded = false;
            }
        }

        public void Tick(float dt)
        {
            timer -= dt;
            if(pc!= null)
            {
                pc.playerVelocity = Vector3.zero;
                pc.moveSpeed = 0f;
                pc.jumpHeight = 0f;
            }
        }

        public void OnRemove()
        {
            if(pc != null){
                pc.jumpHeight = cachedJumpHeight;
                pc.moveSpeed = cachedMoveSpeed;
                pc.ensared = false;
            }
        }

    }

    public void Ensare(GameObject target, float duration)
    {
        var container = Get(target);

        var existing = container.GetEffect<EnsareEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new EnsareEffect(target, duration), target, duration);
    }

    #endregion

    #region FleshSuit

    public class FleshSuitEffect : IEffect
    {
        public string EffectID => "FleshSuit";
        private float hp;
        private float maxHP;

        public bool isFinished => hp <= 0;

        public FleshSuitEffect(float hp)
        {
            this.hp = hp;
            this.maxHP = hp;
        }

        public float Absorb(float dmg)
        {
            float absorbed = Mathf.Min(hp, dmg);
            hp -= absorbed;
            return absorbed;
        }

        public float CurrentHP => hp;
        public float MaxHP => maxHP;

        public void OnApply(){}
        public void Tick(float dt) {}
        public void OnRemove() {}
    }

    public void FleshSuit(GameObject target, float hp)
    {
        var container = Get(target);
        var existing = container.GetEffect<FleshSuitEffect>();
        if(existing != null)
        {
            return;
        }
        AddEffect(new FleshSuitEffect(hp), target);
    }
    #endregion
}

