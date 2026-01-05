using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 platformVelocity;
    // Update is called once per frame
    void Start()
    {
        platformVelocity = new Vector3(0.05f, 0, 0);
    }
    void Update()
    {
        transform.position += platformVelocity;
    }
}
