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
    public Transform anchor;

    [Header("Settings")]
    public GameObject Segment;
    [SerializeField] private float grappleDistance;
    [SerializeField] private float segmentSpacing;
    [SerializeField] private int constraints;
    [SerializeField] private float collisionRadius;
    [SerializeField] private float pullStrength;
    [Range(0f,1f)]
    [SerializeField] private float resolution;
    [SerializeField] private float retractSpeed;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask collisionMask;
    private List<RopePoint> points = new();
    private List<Transform> segments = new();

    private Vector3 grapplePoint;
    private float grappleLength; 
    private float targetLength;

    private SphereCollider probe;

    private Vector3 fixedAnchor;

    struct RopePoint
    {
        public Vector3 position;
        public Vector3 previousPosition;
        public bool locked; 

        public RopePoint(Vector3 pos, bool locked)
        {
            position = pos;
            previousPosition = pos;
            this.locked = locked;
        }
    }

    void Awake()
    {
        GameObject pro = new GameObject("probe");
        pro.hideFlags = HideFlags.HideAndDontSave;

        probe = pro.AddComponent<SphereCollider>();
        probe.radius = collisionRadius;
    }

    void FixedUpdate()
    {
        if(!grappleActive) return;

        Retract();
        // UpdateLength();

        fixedAnchor = anchor.position;
        

        SimulateRope();
        ApplyPlayerConstraint();
    }

    void LateUpdate()
    {
        if (grappleActive)
        {
            UpdateGrapple();
        }
    }

    public void TryGrapple()
    {
        if(Physics.Raycast(playerCam.transform.position, playerCam.transform.forward,
            out RaycastHit hit, grappleDistance, targetMask))
        {
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

        foreach(var seg in segments)
        {
            Destroy(seg.gameObject);
        }

        segments.Clear();
        points.Clear();
    }

    void InitializeRope()
    {
        points.Clear();
        segments.Clear();

        int segmentCount = Mathf.CeilToInt(grappleLength / segmentSpacing);

        Vector3 dir = (grapplePoint - anchor.position).normalized;

        for(int i = 0; i <= segmentCount; i++)
        {
            Vector3 pos = anchor.position + dir * segmentSpacing * i;
            bool locked = (i == 0);
            points.Add(new RopePoint(pos, locked));

            if(i < segmentCount)
            {
                Transform segment = Instantiate(Segment, transform).transform;
                segments.Add(segment);
                segments[i].rotation = Random.rotation;
            }
            
        }
    }

    void Retract()
    {
        if(playerControl.grounded) return;
        grappleLength = Mathf.MoveTowards(grappleLength, targetLength, retractSpeed * Time.fixedDeltaTime);
    }

    void SimulateRope()
    {
        RopePoint first = points[0];
        first.position = grapplePoint;
        points[0] = first;

        RopePoint last = points[^1];
        last.position = fixedAnchor;
        last.locked = true;
        points[^1] = last;

        for(int i = 1; i < points.Count - 1; i++)
        {
            RopePoint p = points[i];

            Vector3 velocity = p.position - p.previousPosition;

            p.previousPosition = p.position;

            p.position += velocity;
            p.position += Physics.gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;

            points[i] = p;
        }

        for(int iteration = 0; iteration < constraints; iteration++)
        {
            SolveDistanceConstraint();
            SolveCollisions();
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

    void UpdateGrapple()
    {
        for(int i = 0; i < segments.Count; i++)
        {
            Vector3 a = points[i].position;
            Vector3 b = points[i + 1].position;

            Vector3 mid = (a + b) * 0.5f;
            Vector3 dir = b - a;

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