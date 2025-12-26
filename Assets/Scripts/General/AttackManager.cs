using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public static AttackManager am;

    private Dictionary<GameObject, bool> attackKey = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        if (am == null)
            am = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void ChooseAttack(GameObject enemy, int type, GameObject prefab, Transform attackPoint, float attackingCooldown, float damage, float fm, float projSpeed)
    {
        if (!attackKey.ContainsKey(enemy))
        {
            attackKey[enemy] = false;
        }

        if (!attackKey[enemy])
        {
            attackKey[enemy] = true;
            if (type == 1)
            {
                StartCoroutine(MeleeHitbox(enemy, prefab, attackPoint, attackingCooldown, damage, fm));
            }
            else if (type == 2)
            {
                StartCoroutine(RangedHitbox(enemy, prefab, attackPoint, attackingCooldown, damage, fm, projSpeed));
            }
        }
    }

    public IEnumerator MeleeHitbox(GameObject enemy, GameObject prefab, Transform attackPoint, float attackingCooldown, float damage, float fm)
    {
        GameObject hitbox = Instantiate(prefab, attackPoint.position, attackPoint.rotation);
        
        MeleeCollider melee = hitbox.GetComponent<MeleeCollider>();

        if (melee != null)
        {
            melee.damage = damage;
            melee.transform.position = attackPoint.position;
            melee.forceMultiplier = fm;
        }

        Destroy(hitbox, 0.3f);

        yield return new WaitForSeconds(attackingCooldown);
        attackKey[enemy] = false;
    }

    public IEnumerator RangedHitbox(GameObject enemy, GameObject prefab, Transform attackPoint, float attackingCooldown, float damage, float fm, float projSpeed)
    {
        GameObject hitbox = Instantiate(prefab, attackPoint.position, attackPoint.rotation);

        EnemyProjectile proj = hitbox.GetComponent<EnemyProjectile>();

        Rocket rocket = hitbox.GetComponent<Rocket>();

        if (proj != null)
        {
            proj.damage = damage;
            proj.speed *= projSpeed;
            proj.thisEnemy = enemy;
        }
        

        if (rocket != null)
        {
            rocket.forceMultiplier = fm;
            rocket.maximumDamage = damage;
        }



        yield return new WaitForSeconds(attackingCooldown);
        attackKey[enemy] = false;
    }
}

