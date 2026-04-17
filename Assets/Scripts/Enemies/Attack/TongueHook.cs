using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TongueHook : EnemyProjectile
{
    public Transform attackPoint;
    public float HookDuration;
    public float steerStrength;
    public AnimationCurve HookCurve;
    private float cachedMoveSpeed;
    private float cachedJumpHeight;
    private bool hookActive = false;

    public Vector3 target;

    public float segmentSpacing = 0.3f;
    public int maxSegments = 30;
    public string segmentPoolID = "TongueSegment";

    private List<Transform> segments = new();

    float t = 0f;

    bool missed = false;

    public void FixedUpdate()
    {
        if (missed || collided) return;

    Vector3 vel = rb.linearVelocity;

    if (vel.sqrMagnitude < 0.001f)
        vel = transform.forward * speed;

    Vector3 wishDir = (enemy.player.position - transform.position).normalized;
    Vector3 currentDir = vel.normalized;

    Vector3 steer = wishDir - currentDir;

    Vector3 forwardBias = transform.forward * 0.3f;

    Vector3 accel = (steer + forwardBias) * steerStrength * speed;

    vel += accel * Time.fixedDeltaTime;

    vel = vel.normalized * speed;

    rb.linearVelocity = vel;
    }

    public override void Update()
    {
        if(!hookActive){
            t += Time.deltaTime;
        }
        if(t >= autoTimer)
        {
            Retract();
        }


        if (missed)
        {
            if(Vector3.Distance(transform.position, attackPoint.position) <= 3f)
            {
                Destroy(gameObject);
            }
        }

        if(collided) return;
        
        Vector3 a = attackPoint.position;
        Vector3 b = hookActive ? target : transform.position;

        

        SyncSegments(a,b);
        RenderSegments(a,b);
    }

    void Retract()
    {
        missed = true;
        rb.linearVelocity = transform.forward * 0f;
        transform.position = Vector3.Lerp(transform.position, attackPoint.position, Time.deltaTime * 10f);
    }

    public IEnumerator StartHook()
    {
        EffectManager.Instance.Ensare(player, HookDuration);
        float t = 0f;
        hookActive = true;
        Vector3 startPosition = player.transform.position;

        while (t < HookDuration)
        {
            t += Time.deltaTime;
            float elapsed = t / HookDuration;
            float value = HookCurve.Evaluate(elapsed);
            player.transform.position = Vector3.Lerp(startPosition, attackPoint.position, value);
            transform.position = Vector3.Lerp(startPosition, attackPoint.position, value);
            target = player.transform.position;
            
            SyncSegments(attackPoint.position,transform.position);
            RenderSegments(attackPoint.position,transform.position);

            yield return null;
        }
        ClearSegments();
        RestorePlayer();
        Destroy(gameObject);
    }

    void RestorePlayer()
    {
        if (!hookActive) return;
        hookActive = false;
    }

    void AddSegment()
    {
        var obj = PrefabPool.Instance.Get(segmentPoolID);
        obj.transform.rotation = Random.rotation;
        segments.Add(obj.transform);
    }

    void RemoveSegment()
    {
        if(segments.Count == 0) return;

        var t = segments[^1];
        segments.RemoveAt(segments.Count - 1);
        PrefabPool.Instance.Return(segmentPoolID, t.gameObject);
    }

    void ClearSegments()
    {
        for(int i = 0; i < segments.Count; i++)
        {
            PrefabPool.Instance.Return(segmentPoolID, segments[i].gameObject);
        }
        segments.Clear();
    }

    void OnDestroy()
    {
        ClearSegments();
        
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
                if(pc.ensared) {Retract(); return;}
                if(missed) return;
                StartCoroutine(StartHook());
            }

            if(other.gameObject.layer == 7)
            {
                Retract();
            }

    }

    void SyncSegments(Vector3 start, Vector3 end)
    {
        float dist = Vector3.Distance(start, end);

        int desired = Mathf.Clamp(Mathf.CeilToInt(dist / segmentSpacing), 1, maxSegments);

        while(segments.Count < desired) AddSegment();

        while(segments.Count > desired) RemoveSegment();
    }

    void RenderSegments(Vector3 start, Vector3 end)
    {
        int count = segments.Count;
        float totalDist = Vector3.Distance(start, end);

        for(int i = 0; i < count; i++)
        {
        float distAlong = i * segmentSpacing;
        float t = (totalDist > 0f) ? Mathf.Clamp01(distAlong / totalDist) : 0f;

        Vector3 pos = Vector3.Lerp(start, end, t);

            Transform seg = segments[i];
            seg.position = pos;

            if(i < count - 1)
            {
                Vector3 next = Vector3.Lerp(start, end, (float)(i + 1) / (count - 1));
            }
        }
    }
        
}
