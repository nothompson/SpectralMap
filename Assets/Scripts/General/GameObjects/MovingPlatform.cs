using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] public AnimationCurve Curve;
    [SerializeField] public Transform[] Targets;
    [SerializeField] public float Duration;
    [SerializeField] public GameObject Collider;
    [SerializeField] public bool Init;
    [SerializeField] public bool pingPong;
    public Vector3 PlatformDelta;
    public bool Stopped;

    private Vector3 lastPos;
    private int index;
    private int pingPongDir = 1;

    void Start()
    {
        lastPos = Collider.transform.position;
        if(Init) StartCoroutine(MovingTowardsTarget());
    }

    void FixedUpdate()
    {
        PlatformDelta = Collider.transform.position - lastPos;
        lastPos = Collider.transform.position;
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
                    pingPongDir *= -1;
                    nextIndex = index + pingPongDir;
                }
            }
            else
            {
                nextIndex = (index + 1) % Targets.Length;
            }

            Transform from = Targets[index];
            Transform to = Targets[nextIndex];
            float t = 0f;

            while(t < Duration)
            {
                if(Stopped) yield break;
                t += Time.deltaTime;
                float elapsed = Mathf.Clamp01(t / Duration);
                Collider.transform.position = Vector3.Lerp(from.position, to.position, Curve.Evaluate(elapsed));
                yield return new WaitForFixedUpdate();
            }

            Collider.transform.position = to.position;
            index = nextIndex;
        }
    }
}
