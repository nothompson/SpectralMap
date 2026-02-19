using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Grapple : MonoBehaviour
{
    [Header("References")]
    public PlayerControlRigid playerControl;
    public Rigidbody playerBody;
    public Camera playerCam;
    public bool grappleActive; 
    public bool grappleFired => projectile != null;
    public Transform anchor;

    [Header("Settings")]
    public GameObject Segment;
    [SerializeField] public float grappleDistance;
    [SerializeField] public float steerStrength;
    [SerializeField] public float segmentSpacing;
    [SerializeField] private int constraints;
    [SerializeField] public float collisionRadius;
    [SerializeField] private float pullStrength;
    [Range(0f,1f)]
    [SerializeField] private float resolution;
    [SerializeField] private float retractSpeed;
    [SerializeField] public LayerMask targetMask;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float releaseDur;
    private int releasePointCount = 0;
    private float releaseProgress = 0f;
    private bool releasing = false;

    public GameObject proj;
    [SerializeField] public float projectileSpeed;

    private List<RopePoint> points = new();
    private List<Transform> segments = new();
    private List<Vector3> renderPositions = new();

    private List<Transform> trail = new();

    private Transform grappleTarget;
    private Vector3 grapplePoint;
    private Vector3 grappleLocal;

    private float grappleLength; 
    private float targetLength;
    private Vector3 fixedAnchor;

    private SphereCollider probe;
    private GrappleProjectile projectile;

    struct RopePoint
    {
        public Vector3 position;
        public Vector3 previousPosition;
        public Vector3 renderPosition;
        public bool locked; 

        public RopePoint(Vector3 pos, bool locked)
        {
            position = pos;
            previousPosition = pos;
            renderPosition = pos;
            this.locked = locked;
        }
    }

    void Awake()
    {
            GameObject probeObj = new GameObject("probe");

            probe = probeObj.AddComponent<SphereCollider>();
            probe.isTrigger = true;
            probe.center = Vector3.zero;
            probe.radius = collisionRadius;
    }

    void OnDestroy()
    {
        if(probe != null)
        {
            Destroy(probe.gameObject);
        }
    }

    void FixedUpdate()
    {
        if(points.Count > 0)
        {
            RopePoint a = points[0];
            a.position = anchor.position;
            a.previousPosition = anchor.position;
            points[0] = a;
        }

        if (releasing)
        {
            releaseProgress += Time.fixedDeltaTime / releaseDur;

            int target = Mathf.Max(1, Mathf.CeilToInt(Mathf.Lerp(releasePointCount, 1, releaseProgress)));
            while(points.Count > target)
            {
                if(points.Count > 1)
                {
                    points.RemoveAt(points.Count - 1);
                    renderPositions.RemoveAt(renderPositions.Count - 1);
                }
                if(segments.Count > 0)
                {
                    Destroy(segments[^1].gameObject);
                    segments.RemoveAt(segments.Count - 1);
                }
            }

            if(releaseProgress >= 1f || points.Count <= 1)
            {
                foreach(var seg in segments) Destroy(seg.gameObject);
                segments.Clear();
                points.Clear();
                renderPositions.Clear();
                releasing = false;
                return;
            }

            if(points.Count >= 2)
            {
                grappleLength = (points.Count - 1) * segmentSpacing;
                
                if(renderPositions.Count == points.Count)
                    for(int i = 0; i < points.Count; i++)
                        renderPositions[i] = points[i].renderPosition;

                SimulateReleasing(anchor.position);
            }

        return;
        }

        if (grappleFired)
        {
            SimulateConnecting();
            return;
        }

        if(!grappleActive) return;

        if(grappleTarget != null)
        {
            Fireball fireball = grappleTarget.gameObject.GetComponentInChildren<Fireball>();
            if(fireball != null) fireball.grappled = true;

            grapplePoint = grappleTarget.TransformPoint(grappleLocal);
        }

        if(grappleTarget == null)
        {
            Release();  
            return;
        }

        if (renderPositions.Count == points.Count){
            for (int i = 0; i < points.Count; i++)
            {
                renderPositions[i] = points[i].renderPosition;
            }
        }
    
        Retract();
        SimulateFinal(grapplePoint, anchor.position);
        ApplyPlayerConstraint();
    }

    void LateUpdate()
    {
        if (grappleActive || grappleFired || releasing) UpdateGrapple(true);

    }

    public void TryGrapple(Transform attackPoint)
    {
        if(grappleActive || grappleFired) return;

        Vector3 dir = playerCam.transform.forward;

        Vector3 from = attackPoint.position;

        Shoot(from, dir);

    }


    public void TryGrappleInstant(Transform attackPoint)
    {
        if(Physics.Raycast(playerCam.transform.position, playerCam.transform.forward,
            out RaycastHit hit, grappleDistance, targetMask))
        {
            grappleTarget = hit.collider.transform;

            grappleLocal = grappleTarget.InverseTransformPoint(hit.point);

            grapplePoint = hit.point;

            grappleLength = hit.distance;

            targetLength = hit.distance * resolution;

            InitializeRope();
            
            grappleActive = true;
        }
    }

    public void Release()
    {
        grappleActive = false;
        grappleTarget = null;

        if(projectile != null)
        {
            Destroy(projectile.gameObject);
            projectile = null;
        }

        // foreach(var seg in segments)
        // {
        //     Destroy(seg.gameObject);
        // }

        releasing = true;
        releaseProgress = 0f;
        releasePointCount = points.Count;  
        

        // segments.Clear();
        // points.Clear();
        // renderPositions.Clear();
    }

    public void OnProjectileHit(Transform target, Vector3 hitPoint, float distance)
    {
        projectile = null;

        grappleTarget = target;

        grappleLocal = grappleTarget.InverseTransformPoint(hitPoint);

        grapplePoint = hitPoint;

        grappleLength = distance;

        targetLength = distance * resolution;

        // InitializeRope();

        TransitionToFinal();

        grappleActive = true;
    }

    public void OnProjectileMiss()
    {
        projectile = null;
        Release();
    }

    void Shoot(Vector3 pos, Vector3 dir)
    {
        // points.Add(new RopePoint(anchor.position, true));
        // renderPositions.Add(anchor.position);
        releasing = false;
        releaseProgress = 0f;
        grappleActive = false;
        grappleTarget = null;

        foreach(var s in segments) Destroy(s.gameObject);
        segments.Clear();
        points.Clear();
        renderPositions.Clear();

        points.Add(new RopePoint(anchor.position, true));
        renderPositions.Add(anchor.position);

        GameObject projObj = Instantiate(proj, pos, Quaternion.LookRotation(dir));
        projectile = projObj.AddComponent<GrappleProjectile>();
        projectile.Init(this, dir, playerBody.gameObject.transform);

    }

    void SimulateConnecting()
    {
        if(projectile == null) return;

        if(renderPositions.Count == points.Count)
        {
            for(int i = 0; i < points.Count; i++)
            {
                renderPositions[i] = points[i].renderPosition;
            }
        }

        Vector3 headPosition = projectile.transform.position;
        float dist = Vector3.Distance(anchor.position, headPosition);
        int wishPoints = Mathf.FloorToInt(dist / segmentSpacing) + 1;
        wishPoints = Mathf.Max(wishPoints, 1);

        while(points.Count < wishPoints)
        {
            points.Add(new RopePoint(headPosition, false));
            renderPositions.Add(headPosition);

            if(points.Count >= 2)
            {
                var s = Instantiate(Segment, transform).transform;
                s.rotation = Random.rotation;
                segments.Add(s);
            }
        }

        while(points.Count > wishPoints && points.Count > 1)
        {
            points.RemoveAt(points.Count - 1);
            renderPositions.RemoveAt(renderPositions.Count - 1);
            if(segments.Count > 0)
            {
                Destroy(segments[^1].gameObject);
                segments.RemoveAt(segments.Count - 1);
            }
        }

        RopePoint anchorPoint = points[0];
        anchorPoint.position = anchor.position;
        anchorPoint.previousPosition = anchor.position;
        anchorPoint.locked = true;
        points[0] = anchorPoint;

        RopePoint head = points[^1];
        head.position = headPosition;
        head.previousPosition = headPosition;
        head.locked = false;
        points[^1] = head;

        for(int i = 1; i < points.Count - 1; i++)
        {
            RopePoint p = points[i];
            Vector3 temp = p.position;
            p.position += p.position - p.previousPosition;
            p.position += Physics.gravity * (Time.fixedDeltaTime * Time.fixedDeltaTime);
            p.previousPosition = temp;
            points[i] = p;
        }

        grappleLength = dist;

        for(int iteration = 0; iteration < constraints; iteration++)
        {
            SolveDistanceConstraint();
        }
        for(int i = 0; i < points.Count; i++)
        {
            RopePoint p = points[i];
            p.renderPosition = p.position;
            points[i] = p;
        }
    }

    void SimulateReleasing(Vector3 anchorPos)
    {
        if(points.Count < 2) return;

        RopePoint first = points[0];
        first.position = anchorPos;
        first.locked = true;
        points[0] = first;

        RopePoint last = points[^1];
        last.locked = false;
        points[^1] = last;

        for(int i = 1; i < points.Count; i++)
        {
            RopePoint p = points[i];
            Vector3 temp = p.position;
            p.position += p.position - p.previousPosition;
            p.position += Physics.gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
            p.previousPosition = temp;
            points[i] = p;
        }

        for(int iteration = 0; iteration < constraints; iteration++)
        {
            SolveDistanceConstraint();
            SolveCollisions();
        }

        for(int i = 0; i < points.Count; i++)
        {
            RopePoint p = points[i];
            p.renderPosition = p.position;
            points[i] = p;
        }
    }

    void ClearGrapple()
    {
        foreach(var seg in segments) Destroy(seg.gameObject);
        segments.Clear();
        points.Clear();
        renderPositions.Clear();
    }

    void InitializeRope()
    {
        foreach(var seg in segments) Destroy(seg.gameObject);
        points.Clear();
        segments.Clear();
        renderPositions.Clear();

        int segmentCount = Mathf.CeilToInt(grappleLength / segmentSpacing);

        Vector3 dir = (grapplePoint - anchor.position).normalized;

        for(int i = 0; i <= segmentCount; i++)
        {
            Vector3 pos = anchor.position + dir * segmentSpacing * i;
            bool locked = (i == 0);
            points.Add(new RopePoint(pos, locked));
            renderPositions.Add(pos);

            if(i < segmentCount)
            {
                Transform segment = Instantiate(Segment, transform).transform;
                segments.Add(segment);
                segments[i].rotation = Random.rotation;
            }
            
        }
    }

    void TransitionToFinal()
    {
        int targetSegments = Mathf.CeilToInt(grappleLength / segmentSpacing);
        int targetPoints = targetSegments + 1;

        while(points.Count > targetPoints)
        {
            points.RemoveAt(points.Count - 1);
            renderPositions.RemoveAt(renderPositions.Count - 1);
        }
        while(segments.Count > targetSegments)
        {
            Destroy(segments[^1].gameObject);
            segments.RemoveAt(segments.Count - 1);
        }

        // Vector3 dir = (grapplePoint - anchor.position).normalized;

        // while(points.Count < targetPoints)
        // {
        //     int i = points.Count
        // }

        RopePoint anchorPoint = points[0];
        anchorPoint.position = anchor.position;
        anchorPoint.previousPosition = anchor.position;
        anchorPoint.locked = true;
        points[0] = anchorPoint;

        RopePoint head = points[^1];
        head.position = grapplePoint;
        head.previousPosition = grapplePoint;
        head.locked = true;
        points[^1] = head;

        for(int i = 0; i < points.Count; i++)
        {
            RopePoint p = points[i];
            p.renderPosition = p.position;
            points[i] = p;
        }

    }

    void Retract()
    {
        if(playerControl.grounded) return;
        grappleLength = Mathf.MoveTowards(grappleLength, targetLength, retractSpeed * Time.fixedDeltaTime);
    }

    void SimulateFinal(Vector3 headPos, Vector3 anchorPos)
    {
        if(points.Count < 2) return;

        RopePoint first = points[0];
        first.position = anchorPos;
        points[0] = first;

        RopePoint last = points[^1];
        last.position = headPos;
        last.locked = true;
        points[^1] = last;

        for(int i = 1; i < points.Count - 1; i++)
        {
            RopePoint p = points[i];

            Vector3 temp = p.position;

            Vector3 velocity = p.position - p.previousPosition;

            p.position += velocity;
            p.position += Physics.gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;

            p.previousPosition = temp;

            points[i] = p;
        }

        for(int iteration = 0; iteration < constraints; iteration++)
        {
            SolveDistanceConstraint();
            SolveCollisions();
        }

        for(int i = 0; i < points.Count; i++)
        {
            RopePoint p = points[i];
            p.renderPosition = p.position;
            points[i] = p;
        }
    }

    void SolveDistanceConstraint()
    {
        float segmentLength = grappleLength / (points.Count - 1);

        for(int i = 0; i < points.Count - 1; i++)
        {
            RopePoint a = points[i];
            RopePoint b = points[i + 1];

            Vector3 delta = b.position - a.position;
            float dist = delta.magnitude;

            if(dist < 0.0001f) continue;

            float error = dist - segmentLength;

            Vector3 correction = delta.normalized * error * 0.5f;

            if (!a.locked)
            {
                a.position += correction;
            }
            if (!b.locked)
            {
                b.position -= correction;
            }
            points[i] = a;
            points[i + 1] = b;
        }
    }

void SolveCollisions()
    {
        for(int i = 1; i < points.Count - 1; i++)
        {
            RopePoint p = points[i];

            Collider[] hits = Physics.OverlapSphere(
                p.position,
                collisionRadius,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );

            foreach(var hit in hits)
            {
                Vector3 dir;
                float dist;

                bool overlapped = Physics.ComputePenetration(
                    probe,
                    p.position,
                    Quaternion.identity,
                    hit,
                    hit.transform.position,
                    hit.transform.rotation,
                    out dir,
                    out dist
                );

                if(overlapped && dist > 0f)
                {
                    p.position += dir * dist;

                    p.previousPosition = p.position;
                }

            }

            points[i] = p;
        }
    }


    void ApplyPlayerConstraint()
    {
        Vector3 toPlayer = playerBody.position - grapplePoint;
        float dist = toPlayer.magnitude;

        if(dist <= 0.001f) return;

        Vector3 dir = toPlayer.normalized;

        if(dist >= grappleLength - 0.05f)
        {
            float radial = Vector3.Dot(playerControl.playerVelocity, dir);

            if(radial > 0f) playerControl.playerVelocity += -dir * radial;

            playerControl.playerVelocity += -dir * pullStrength * (dist - grappleLength);
        }
    }

    void UpdateGrapple(bool interpolate)
    {
        if(segments.Count == 0 || points.Count < 2) return;
        float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        for(int i = 0; i < segments.Count; i++)
        {
            Vector3 a = interpolate ? Vector3.Lerp(renderPositions[i], points[i].renderPosition, alpha) : points[i].position;
            Vector3 b = interpolate ? Vector3.Lerp(renderPositions[i + 1], points[i + 1].renderPosition, alpha) : points[i + 1].position;

            Vector3 mid = (a + b) * 0.5f;

            segments[i].position = mid;
        }
    }

    void UpdateLength()
    {
        int wishCount = Mathf.Max(1, Mathf.CeilToInt(grappleLength / segmentSpacing));

        int segmentCount = points.Count - 1;    

        if(wishCount == segmentCount) return;

        if(wishCount < segmentCount)
        {
            int remove = segmentCount - wishCount;

            for(int i = 0; i < remove; i++)
            {
                if(segments.Count > 0)
                {
                    Destroy(segments[^1].gameObject);
                    segments.RemoveAt(segments.Count - 1);
                }

                if(points.Count > 2)
                {
                    points.RemoveAt(points.Count - 2);
                }
            }
        }
    }
}