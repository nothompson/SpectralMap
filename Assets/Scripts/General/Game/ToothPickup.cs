using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class ToothPickup : MonoBehaviour
{
    public ToothObject ToothData;
    public string ID;
    public bool Added = false;

    public void Awake()
    {
        if(ToothData != null && ToothData.Added)
        {
            Destroy(gameObject);
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            ToothManager.Instance.AddTooth(ToothData);
            Destroy(gameObject);
        }
    }
}