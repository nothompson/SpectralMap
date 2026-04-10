using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TongueHook : EnemyProjectile
{
    public Transform attackPoint;
    public float HookDuration;
    public AnimationCurve HookCurve;
private float cachedMoveSpeed;
private float cachedJumpHeight;
private bool hookActive = false;

public IEnumerator StartHook()
{
    float t = 0f;
    pc.playerVelocity = Vector3.zero;
    cachedMoveSpeed = pc.moveSpeed;
    cachedJumpHeight = pc.jumpHeight;
    pc.grounded = false;
    pc.moveSpeed = 0f;
    pc.jumpHeight = 0f;
    hookActive = true;
     pc.ensared = true;
    Vector3 startPosition = player.transform.position;

    while (t < HookDuration)
    {
        t += Time.deltaTime;
        float elapsed = t / HookDuration;
        float value = HookCurve.Evaluate(elapsed);
        player.transform.position = Vector3.Lerp(startPosition, attackPoint.position, value);
        yield return null;
    }

    RestorePlayer();
}

void RestorePlayer()
{
    if (!hookActive) return;
    hookActive = false;
    pc.moveSpeed = cachedMoveSpeed;
    pc.jumpHeight = cachedJumpHeight;
    pc.ensared = false;
}

void OnDestroy()
{
    if (pc != null)
        RestorePlayer();
}

    public override void OnTriggerEnter(Collider other)
    {
        if (collided) return;

        if (other.gameObject.layer == 3)
        {
            player = other.transform.gameObject;
            pc = player.GetComponent<PlayerControlRigid>();
            playerHealth = player.GetComponent<HP>();

            collided = true;
            StartCoroutine(StartHook());
        }

    }
    
}
