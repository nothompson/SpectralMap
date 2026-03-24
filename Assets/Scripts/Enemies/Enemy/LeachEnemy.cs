using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeachEnemy : Enemy
{
    [Header("Leach Settings")]
    [SerializeField] private Chain Body;
    [SerializeField] private float SeekingGracePeriod;
    [SerializeField] private float LifeSpan;

    private Vector3 wishDir;
    [HideInInspector] public Vector3 spawnDir;
    private Vector3 toPlayer;
    private bool seeking;

    private void OnEnable()
    {
        seeking = false;
        spawnDir = Vector3.one;
        StartCoroutine(WaitToSeekPlayer());
        StartCoroutine(AutoDeath());
    }

    public override void References()
    {
        GameObject playerRef = GameObject.FindWithTag("Player");
        if (playerRef != null)
            player = playerRef.transform;
        
        rb.freezeRotation = true;
    }
    
    public override void FixedUpdate()
    {
        Rigidbody head = Body.Head;

        float speed = seeking ? moveSpeed : moveSpeed * 0.3f;

        Vector3 adjusted = new Vector3(player.position.x, player.position.y + 1f, player.position.z);
        toPlayer = (adjusted - Body.HeadPosition).normalized;

        wishDir = seeking ? toPlayer : spawnDir;

        float noiseX = (Mathf.PerlinNoise(Time.time * chaosFrequency + noiseOffset, 0f) * 2f - 1f) * movementChaos;
        float noiseY = (Mathf.PerlinNoise(Time.time * chaosFrequency + noiseOffset + 99f, 0f) * 2f - 1f) * movementChaos;
        float noiseZ = (Mathf.PerlinNoise(Time.time * chaosFrequency + noiseOffset + 33f, 0f) * 2f - 1f) * movementChaos;

        Vector3 chaosDir = (wishDir + new Vector3(noiseX, noiseY, noiseZ)).normalized;

        Vector3 dir = Vector3.Lerp(wishDir, chaosDir, chaosBlend).normalized;

        head.linearVelocity = Vector3.Lerp(head.linearVelocity, dir * speed, Body.Strength * Time.fixedDeltaTime);

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        head.MoveRotation(Quaternion.Slerp(head.rotation, targetRot, Body.Strength * Time.fixedDeltaTime));

        if((adjusted - Body.HeadPosition).magnitude <= MaxRange && !attacking)
        {
            Behaviors[0].Begin();
        }
    }

    void OnDestroy()
    {
        if(Body == null) return;
        foreach(Rigidbody s in Body.Segments)
        {
            if(s != null) Destroy(s.gameObject);
        }
    }

    IEnumerator WaitToSeekPlayer()
    {
        yield return new WaitForSeconds(SeekingGracePeriod);

        seeking = true;
    }

    IEnumerator AutoDeath()
    {
        yield return new WaitForSeconds(LifeSpan);

        hp.Damage(hp.currentHP);
    }
}
