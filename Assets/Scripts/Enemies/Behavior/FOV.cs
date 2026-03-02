using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//from youttube tutorial : https://www.youtube.com/watch?v=j1-OyLo77ss

public class FOV : MonoBehaviour
{
    public float radius;

    [Range(0, 360)]
    public float angle;

    public GameObject player;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeePlayer;

    public bool playerInRange;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());
    }

    //coroutine, not every frame
    private IEnumerator FOVRoutine()
    {
        float del = 0.5f;
        WaitForSeconds wait = new WaitForSeconds(del);
        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;

            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle * 0.5f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeePlayer = true;
                }
                else
                    canSeePlayer = false;
            }
            else
                canSeePlayer = false;

            playerInRange = true;
        }
        else if (canSeePlayer)
        {
            canSeePlayer = false;
        }

        else
        {
            playerInRange = false;
        }
    }
}
