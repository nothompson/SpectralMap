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

    public float ForceMultiplier; 

    public bool ProjectileGrappling;

    public bool Forgiveness;

    public bool SpectralFlame;
    public bool Grapple;

    public bool HoldingObject;

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
        ForceMultiplier = 1f;
        ProjectileGrappling = false;
        Forgiveness = false;

        SpectralFlame = false;
        Grapple = false;
    }

    public void CheckItems()
    {
        if (InventoryManager.Instance.HasItem("bezoar"))
        {
            DamageMultiplier = 1.5f;
        }

        if (InventoryManager.Instance.HasItem("lichen"))
        {
            Forgiveness = true;
        }

        if (InventoryManager.Instance.HasItem("spectralflame"))
        {
            SpectralFlame = true;
        }

           if (InventoryManager.Instance.HasItem("slimehook"))
        {
            Grapple = true;
        }
        
    }
    

    
}
