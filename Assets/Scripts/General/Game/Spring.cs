using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public PlayerControlRigid pc;

    [Header("Params")]
    public float k = 0.1f;

    [Header("State")]
    public bool active;


    // Start is called before the first frame update
    void Start()
    {
        pc = player.GetComponent<PlayerControlRigid>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        float displacement = distance * -1f;

    }

}
