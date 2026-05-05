    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using TMPro;    
    using System.IO;


public class SaveSlotManager : MonoBehaviour
{
    public static SaveSlotManager Instance;

    public SaveData[] saves;
    [Range(0,2)]
    [SerializeField] public int Save;
    void Awake(){

        if(Instance == null)
            {
            Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SaveSystem.CurrentSlot = Save;

            SaveSystem.EnsureSlotExists(Save);
            LoadAllSlots();
        if (!saves[Save].hasData)
        {
            saves[Save].hasData = true;
            SaveSlot(Save);
        }
            
}

void Start()
    {
        SaveSystem.OnSaveChange();
        InventoryManager.Instance.AddItem("spectralflame", new Vector2Int(0,0));
        InventoryManager.Instance.AddItem("slimehook", new Vector2Int(0,0));
        InventoryManager.Instance.AddItem("lichen", new Vector2Int(0,0));
    }

        
    void LoadAllSlots()
    {
        saves = new SaveData[3];
        for(int i = 0; i < 3; i++)
        {
            string path = SaveSystem.GetFilePath(i, "Save.json");
            saves[i] = File.Exists(path) ? JsonUtility.FromJson<SaveData>(File.ReadAllText(path)) : new SaveData();
        }
    }

    void SaveSlot(int index)
    {
        SaveSystem.EnsureSlotExists(index);
        string path = SaveSystem.GetFilePath(index, "Save.json");
        File.WriteAllText(path, JsonUtility.ToJson(saves[index], true));
    }



}
