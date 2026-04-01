using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class DoorManager : MonoBehaviour
{
    public static DoorManager Instance;

    public DoorObject[] AllDoors;

    public Dictionary<string, DoorObject> DoorLookup = new();

    public Dictionary<string, Door> ActiveDoorScripts = new();

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

    public void OnSaveChange()
    {
        LoadAllDoors();
        LoadDoorProgress();
    }

    public void RegisterDoor(Door door)
    {
        if(door.Data == null) return;

        ActiveDoorScripts[door.Data.ID] = door;
    }

    public void UnRegisterDoor(Door door)
    {
        if(door.Data == null) return;

        bool exists = ActiveDoorScripts.TryGetValue(door.Data.ID, out var d);
        bool matches = d == door;

        if(exists && matches)
        {
            ActiveDoorScripts.Remove(door.Data.ID);
        }
    }

    void LoadAllDoors(){
        AllDoors = Resources.LoadAll<DoorObject>("Doors");
        DoorLookup = AllDoors.ToDictionary(door => door.ID, door => door);
    }

    public void SaveDoorProgress()
    {
        DoorDataBase data = new DoorDataBase();

        foreach(var door in AllDoors)
        {
            DoorData d = new DoorData
            {
                ID = door.ID,
                Opened = door.Opened
            };

            data.Doors.Add(d);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
    }

    public void LoadDoorProgress()
    {
        foreach(var d in AllDoors)
        {
            d.Opened = false;
        }

        if(!File.Exists(GetSavePath())) return;

        string json = File.ReadAllText(GetSavePath());
        DoorDataBase data = JsonUtility.FromJson<DoorDataBase>(json);

        foreach(var d in data.Doors)
        {
            if(!DoorLookup.TryGetValue(d.ID, out var door)) continue;

            door.Opened = d.Opened;
        }
    }

    public void OpenDoorRemotely(string doorID)
    {
        if (DoorLookup.TryGetValue(doorID, out var data))
        {
            if(data.Opened) return;

            if(ActiveDoorScripts.TryGetValue(doorID, out var door)){
                FeedManager.Instance.AddToFeed("Somewhere, a door opened");
                door.Open();
            }
        }
    }

    string GetSavePath()
    {
        return SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Doors.json");
    }
}
