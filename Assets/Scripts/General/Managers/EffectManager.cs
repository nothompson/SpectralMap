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

    #region FleshSuit

    public class FleshSuitEffect : IEffect
    {
        public string EffectID => "FleshSuit";
        private float hp;

        public bool isFinished => hp <= 0;

        public FleshSuitEffect(float hp)
        {
            this.hp = hp;
        }

        public float Absorb(float dmg)
        {
            float absorbed = Mathf.Min(hp, dmg);
            hp -= absorbed;
            return absorbed;
        }

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

