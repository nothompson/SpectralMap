using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class RandomForce : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Start()
    {

        if(rb != null)
        {
            rb.AddForce(Random.insideUnitSphere * 20f, ForceMode.Impulse);
        }
    }
}