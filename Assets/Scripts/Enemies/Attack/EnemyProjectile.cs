using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IKnockback
{
    [Header("References")]
    public GameObject player;
    public PlayerControlRigid pc;

    public GameObject enemy;
    public HP playerHealth;
    public Rigidbody rb;

    public GameObject thisEnemy;

    public LayerMask targetMask;
    [Header("General")]
    public float speed;
    public float damage;
    public float autoTimer = 5f;
    [Header("State")]
    public bool reflected = false;
    public bool support = false;
    public bool collided;
    public HP enemyHP;

    public virtual void Start()
    {
        //assign each prefab a rigid body
        rb = GetComponent<Rigidbody>();
        //move based on forward direction and speed param
        rb.linearVelocity = transform.forward * speed;
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (collided) return;

        if (other.gameObject.layer == 3 && other.transform.parent != null)
        {
            player = other.transform.parent.gameObject;
            pc = other.GetComponentInParent<PlayerControlRigid>();
            playerHealth = other.GetComponentInParent<HP>();

            collided = true;
            StartCoroutine(Hit());
        }
        if ((reflected || support) && other.gameObject.layer == 11)
        {
            if (support && other.transform.parent.gameObject != thisEnemy)
            {
                enemy = other.transform.parent.gameObject;
                enemyHP = other.GetComponentInParent<HP>();
                collided = true;
                StartCoroutine(Hit());
            }
            else if (reflected)
            {
                enemy = other.transform.parent.gameObject;
                enemyHP = other.GetComponentInParent<HP>();
                collided = true;
                StartCoroutine(Hit());
            }
    
        }
    }

    public virtual IEnumerator Hit()
    {
        if (collided && !reflected)
        {
            if (playerHealth != null)
            {
                playerHealth.Damage(damage);
            }
        }

            else if (collided && reflected)
            {
                if (enemyHP != null)
                {
                    enemyHP.Damage(damage * 2f);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        yield return new WaitForSeconds(0.01f);
        Destroy(gameObject);
    } 

    public virtual void Update()
    {
        AutoDestroy();
    }

    public void AutoDestroy()
    {
        Destroy(gameObject, autoTimer);
    }

    public void AddKnockback(Vector3 origin)
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 dir = ray.direction;

        rb.linearVelocity = dir * (speed * 2f);

        reflected = true;
    }
}
