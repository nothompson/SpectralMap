using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class MeshRotate : MonoBehaviour
{
    public Vector3 spinVelocity;

    public void Update()
    {
        Vector3 spin = Time.deltaTime * spinVelocity;
        transform.Rotate(spin, Space.Self);
    }
}