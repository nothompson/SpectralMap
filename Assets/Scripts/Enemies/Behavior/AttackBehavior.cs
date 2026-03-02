using UnityEngine;

public abstract class AttackBehavior : MonoBehaviour
{
    [SerializeField] public GameObject AttackPrefab;

    [SerializeField] public float Range;

    [SerializeField] public float Cooldown;
    [SerializeField] public float Damage;
    [SerializeField] public float Force;

    public Transform AttackPoint;
    public GameObject Owner;

    public void InitBehavior(GameObject enemy, Transform point)
    {
        Owner = enemy;

        AttackPoint = point;
    }

    public abstract bool Ready(float distance);

    public abstract float Begin();

}