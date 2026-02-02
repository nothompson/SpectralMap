using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;
    private Dictionary<string,bool> deathLog = new();
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
        Load();
    }

    public void Save()
    {
        DeathData data = new DeathData();
        foreach(var death in deathLog)
        {
            CharacterDeathLog characterDeath = new CharacterDeathLog
            {
                ID = death.Key,
                dead = death.Value
            };
            data.CharacterDeaths.Add(characterDeath);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
    }

    public void Load()
    {
        if(!File.Exists(GetSavePath())) return;

        string json = File.ReadAllText(GetSavePath());
        DeathData data = JsonUtility.FromJson<DeathData>(json);
        deathLog.Clear();

        if(data?.CharacterDeaths != null){
        foreach(var death in data.CharacterDeaths)
        {
            deathLog[death.ID] = death.dead;
        }
        }
    }

    public bool CheckIfDead(string ID)
    {
        if(string.IsNullOrEmpty(ID)) return false;
        if(!deathLog.TryGetValue(ID, out bool dead))
        {
            deathLog[ID] = false;
            return false;
        }
        return dead;
    }

    public void SetDead(string ID, bool dead = true, bool autoSave = true)
    {
        if(string.IsNullOrEmpty(ID)) return;

        deathLog[ID] = dead;

        if (autoSave)
        {
            Save();
        }

        EventManager.Instance.OnKill(ID);
    }


    string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "DeathLog.json");
    }
}
