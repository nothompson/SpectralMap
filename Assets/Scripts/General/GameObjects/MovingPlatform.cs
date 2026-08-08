using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] public AnimationCurve Curve;
    [SerializeField] public Transform[] Targets;
    [SerializeField] public bool ConsistentSpeed = true;
    [SerializeField] public float Duration;
    [SerializeField] public float[] TargetDuration; 
    [SerializeField] public GameObject collider;
    [SerializeField] public bool Init;
    [SerializeField] public bool pingPong;
    [SerializeField] public LayerMask riderMask;
    [SerializeField] public float MomentumCap;
    [SerializeField] public bool Ridable = true;

    public Vector3 PlatformDelta;
    public Vector3 PlatformVelocity;
    public bool Stopped;

    private Vector3 lastPos;
    private int index;
    private int pingPongDir = 1;
    public Rigidbody rb;

    private Collider col;
    private float surfaceY;

    Coroutine MovingRoutine;

    public bool moveTowardsSound = false;   

    public FMODUnity.EventReference moveTowards;

    public bool moveFromSound = false;   

    public FMODUnity.EventReference moveFrom;

    void Start()
    {
        lastPos = collider.transform.position;
        col = collider.GetComponent<Collider>();
        surfaceY = col.bounds.max.y;

        if(Init) MovingRoutine = StartCoroutine(MovingTowardsTarget());
    }

    void FixedUpdate()
    {
        if (Stopped)
        {
            PlatformDelta = Vector3.zero;
            PlatformVelocity = Vector3.zero;
        }
    }

    public void Stop()
    {
        Stopped = true;

    }

    public void StartMoving()
    {
        if(MovingRoutine != null)
        {
            Stopped = false;
            return;
        }
        Stopped = false;
        
        MovingRoutine = StartCoroutine(MovingTowardsTarget());

    }

    public IEnumerator MovingTowardsTarget()
    {
        while(true)
        {
            int nextIndex;
            if(pingPong)
            {
                nextIndex = index + pingPongDir;
                if(nextIndex >= Targets.Length || nextIndex < 0)
                {
                    if(moveTowardsSound)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot(moveTowards, transform.position);
                    }

                    pingPongDir *= -1;
                    nextIndex = index + pingPongDir;
                }
                else
                {
                    if(moveFromSound)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot(moveFrom, transform.position);
                    }
                }
            }
            else
            {
                nextIndex = (index + 1) % Targets.Length;
            }

            Transform from = Targets[index];
            Transform to = Targets[nextIndex];
            float t = 0f;

            float dur = ConsistentSpeed ? Duration : TargetDuration[index];

            while(t < dur)
            {
                if (Stopped)
                {
                    PlatformDelta = Vector3.zero;
                    PlatformVelocity = Vector3.zero;
                    yield break;
                }
                t += Time.fixedDeltaTime;
                float elapsed = Mathf.Clamp01(t / dur);
                Vector3 next = Vector3.Lerp(from.position, to.position, Curve.Evaluate(elapsed));

                PlatformDelta = next - lastPos;
                PlatformVelocity = PlatformDelta / Time.fixedDeltaTime;

                if (PlatformVelocity.magnitude > MomentumCap) PlatformVelocity = PlatformVelocity.normalized * MomentumCap;

                lastPos = next;

                collider.transform.position = next;
                surfaceY = col.bounds.max.y; 

                //avoid tuneling with a larger ttrigger box collider and push up to surface 
                Collider[] riders = Physics.OverlapBox(col.bounds.center + Vector3.up * 2f, new Vector3(col.bounds.extents.x, 2f, col.bounds.extents.z), collider.transform.rotation, riderMask);

                if(Ridable){
                foreach(Collider rider in riders)
                {
                    if(rider.bounds.min.y < surfaceY)
                    {
                        if(rider.transform.parent != null && !rider.transform.parent.CompareTag("MovingPlatform"))
                        {
                            rider.transform.parent.position += Vector3.up * (surfaceY - rider.bounds.min.y);
                        }
                        else
                        {  
                            rider.transform.position += Vector3.up * (surfaceY - rider.bounds.min.y);
                        }
                    }
                }
                }
                yield return new WaitForFixedUpdate();
            }

            Vector3 final = to.position;
            PlatformDelta = final - lastPos;
            PlatformVelocity = PlatformDelta / Time.fixedDeltaTime;

            if (PlatformVelocity.magnitude > MomentumCap) PlatformVelocity = PlatformVelocity.normalized * MomentumCap;

            lastPos = final;
            collider.transform.position = final;
            index = nextIndex;

            PlatformDelta = Vector3.zero;
            PlatformVelocity = Vector3.zero;
            
        }
    }
}
