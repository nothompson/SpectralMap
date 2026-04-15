using UnityEngine;

public class DrainSpell : EnemyProjectile
{
    Launcher playerLauncher = null;
    public override void OnTriggerEnter(Collider other)
    {
        if (collided) return;

        if (other.gameObject.layer == 3)
        {
            player = other.transform.gameObject;
            pc = player.GetComponent<PlayerControlRigid>();
            playerHealth = player.GetComponent<HP>();

            collided = true;
            StartCoroutine(Hit());
            pc.SpellError(true);


        }
    }
}
