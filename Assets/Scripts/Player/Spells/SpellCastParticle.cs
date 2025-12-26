using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
public class SpellCastParticle : MonoBehaviour
{
    public Vector3 displacement = new Vector3(1f,0f,1f);
    private Transform cam;

    public Vector3 position;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void Update()
    {
        if (cam == null) return;

        position = cam.position 
            + cam.forward * displacement.x 
            + cam.right * displacement.y 
            + cam.up * displacement.z;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(cam.forward, cam.up);
    }
}