using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using MovementPhysics;

public class Chain : MonoBehaviour
{
    public Rigidbody[] Segments;

    public float Distance;

    public float Strength;

    public float Damping;

    public Vector3 headVelocity;

    void Start()
    {
        if(Segments == null || Segments.Length < 2) return;

        foreach(Rigidbody s in Segments)
        {
            s.transform.SetParent(null);
        }

        //start at 1 since 0 is head
        for (int i = 1; i < Segments.Length; i++)
        {
            Segments[i].rotation = Random.rotation;
        }

    }

    void FixedUpdate() 
    {
        if(Segments == null || Segments.Length < 2) return;
        
        //start at 1 since 0 is head
        for (int i = 1; i < Segments.Length; i++)
        {
            Rigidbody prev = Segments[i - 1];
            Rigidbody cur = Segments[i];

            Vector3 prevPosition = prev.position;
            Vector3 curPosition = cur.position;

            Vector3 dir = prevPosition - curPosition;
            float dist = dir.magnitude;
            if(dist < 0.001f) continue;

            Vector3 dirNorm = dir / dist;
            
            Vector3 target = prevPosition - dirNorm * Distance;

            Vector3 correction = target - curPosition;

            cur.AddForce(correction * Strength, ForceMode.Acceleration);
            cur.linearVelocity *= (1f - Damping * Time.fixedDeltaTime);

            Quaternion segmentRot = Quaternion.LookRotation(dirNorm, Vector3.up);

            cur.MoveRotation(Quaternion.Slerp(cur.rotation, segmentRot, Strength * Time.fixedDeltaTime));

            cur.position = Vector3.Lerp(curPosition, target, Strength);

        }
    }

    public Rigidbody Head => Segments[0];
    public Vector3 HeadPosition => Segments[0].position;

}