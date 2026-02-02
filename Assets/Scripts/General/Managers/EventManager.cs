using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
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
    }

    public void OnKill(string CharacterID)
    {
        if(string.IsNullOrEmpty(CharacterID)) return;
        switch (CharacterID)
        {
            case "daniel":
                JournalManager.Instance.AddJournalEntry("danieldeath",0);  
                break;
            case "BigJerry":
                Debug.Log("killed big jerry");
                break;; 
        }
    }
}
