using UnityEngine;

public abstract class AttackBehavior : MonoBehaviour
{
    [SerializeField] public GameObject AttackPrefab;

    [SerializeField] public float Range;

    [SerializeField] public float Cooldown;
    [SerializeField] public float Damage;
    [SerializeField] public float Force;
    [SerializeField] public string AnimationEvent;
    [SerializeField] public bool Stationary = false;


    public Transform AttackPoint;
    [HideInInspector] public GameObject Owner;
// 
    public virtual void InitBehavior(GameObject enemy, Transform point)
    {
        Owner = enemy;

        AttackPoint = point;
    }

    public bool Ready(float distance)
    {
        return distance <= Range;
    }

    public abstract float Begin();

}