    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using TMPro;    
    using System.IO;

public static class SaveSystem 
{
    public static int CurrentSlot = 0;

    public static string GetSlotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot_{slot}");
    }

    public static void OnSaveChange()
    {
        if(JournalManager.Instance != null)
            JournalManager.Instance.OnSaveChange();
        
        if(PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnSaveChange();
        }

        if(DoorManager.Instance != null)
        {
            DoorManager.Instance.OnSaveChange();
        }
        if(DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnSaveChange();
        }
        if(DeathManager.Instance != null)
        {
            DeathManager.Instance.OnSaveChange();
        }
        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSaveChange();
        }
        if(SpectrumManager.Instance != null)
        {
            SpectrumManager.Instance.OnSaveChange();
        }
    }
    public static string GetFilePath(int slot, string fileName)
    {
        return Path.Combine(GetSlotPath(slot), fileName);
    }

    public static void EnsureSlotExists(int slot)
    {
        string path = GetSlotPath(slot);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void DeleteSave(int slot)
    {
        string path = GetSlotPath(slot);
        if (Directory.Exists(path))
        {
            Directory.Delete(path,true);
        }
    }


}
