using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using UnityEngine.Events;



public abstract class AttackBehavior : MonoBehaviour
{
    [SerializeField] public GameObject AttackPrefab;
    [System.Serializable]
    public class FireEvent : UnityEvent<Transform> {}
    public FireEvent OnFire;

    [SerializeField] public float Range;

    [SerializeField] public float Cooldown;
    [SerializeField] public float Damage;
    [SerializeField] public float Force;
    [SerializeField] public string AnimationEvent;
    [SerializeField] public bool Stationary = false;
    [SerializeField] public bool Support = false;

    [SerializeField] public bool BurstMode = false;
    [SerializeField] public int BurstCount = 3;

    [SerializeField] public Transform AttackPoint;
    [HideInInspector] public GameObject Owner;
    [HideInInspector] public bool onCooldown = false;
// 
    public virtual void InitBehavior(GameObject enemy, Transform point)
    {
        Owner = enemy;

        AttackPoint = point;
    }

    public bool Ready(float distance)
    {
        if(onCooldown) return false;
        return distance <= Range;
    }

    public abstract void Fire();

    public void StartCooldown()
    {
        Debug.Log("starting cooldown");
        StartCoroutine(CooldownRoutine());
    }
    public IEnumerator CooldownRoutine()
    {
        Debug.Log("on cooldown");
        onCooldown = true;
        yield return new WaitForSeconds(Cooldown);
        onCooldown = false;
        Debug.Log("off cooldown!");
    }

}