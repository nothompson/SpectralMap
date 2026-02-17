using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Chain : MonoBehaviour
{
    public Rigidbody[] Segments;

    public float Distance;

    public float Strength;

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

            Vector3 toTarget = target - curPosition;
            Vector3 wishVel = toTarget / Time.fixedDeltaTime;
            Vector3 velChange = wishVel - cur.linearVelocity;

            cur.AddForce(velChange * Strength, ForceMode.Acceleration);

            Quaternion targetRot = Quaternion.LookRotation(dirNorm, Vector3.up);

            cur.MoveRotation(Quaternion.Slerp(cur.rotation,targetRot,Strength* Time.fixedDeltaTime));

        }
    }
}