using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public float DamageMultiplier; 
    public bool ProjectileGrappling;
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadDefaults();
    }

    public void OnSaveChange()
    {
        LoadDefaults();
    }

    void LoadDefaults()
    {
        DamageMultiplier = 1f;
        ProjectileGrappling = false;
    }

    public void CheckItems()
    {
        if (InventoryManager.Instance.HasItem("bezoar"))
        {
            DamageMultiplier = 1.5f;
        }
    }
    

    
}
