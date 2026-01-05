using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public GameObject player;
    public HP playerHealth;
    public MagicManagement playerMagic;
    public LayerMask playerMask;

    public FMODUnity.StudioEventEmitter pickupSound;

    [Range(1,3)]
    public int type;
    [Range(0, 1)]
    public float size;

    public float spinSpeed = 5f;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerHealth = player.GetComponent<HP>();
        playerMagic = player.GetComponent<MagicManagement>();
    }

    void Update()
    {
        float spin = Time.deltaTime * spinSpeed;
        Vector3 angle = transform.localEulerAngles;
        angle.y += spin;
        transform.localEulerAngles = new Vector3(angle.x, angle.y, angle.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            if (type == 1 && playerHealth.currentHP < playerHealth.maxHP)
            {
                playerHealth.Heal(size);
                pickupSound.Play();
                Destroy(gameObject);
            }

            if (type == 2 && playerMagic.magicPoints < playerMagic.maximumMagic)
            {
                float regen = playerMagic.maximumMagic * size;
                playerMagic.magicPoints += regen;
                pickupSound.Play();
                Destroy(gameObject);
            }

            if (type > 2)
            {
                // float roll = Random.Range(0f, 1f);
                EffectManager.effectManager.Boost(player, 20f, 3.0f);
                pickupSound.Play();
                Destroy(gameObject);
            }
        }

        if (other.gameObject.layer == 11)
        {
            Enemy ai = other.GetComponentInParent<Enemy>();
            GameObject enemy = ai != null ? ai.gameObject : other.gameObject;
            HP enemyHealth = other.GetComponentInParent<HP>();

            if (enemyHealth != null)
            {
                if (type == 1 && enemyHealth.currentHP < enemyHealth.maxHP)
                {
                    ai.critical = false;
                    ai.engage = true;
                    enemyHealth.Heal(size);
                    Destroy(gameObject);
                }
            }
            
            if (type > 2)
            {
                // float roll = Random.Range(0f, 1f);
                EffectManager.effectManager.Weak(enemy, 20f, 0.5f);
                Destroy(gameObject);
            }
 
        }
    }
}
