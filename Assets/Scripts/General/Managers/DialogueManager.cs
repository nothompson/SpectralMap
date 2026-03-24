using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private Dictionary<string,int> progress = new();
    private Dictionary<string,int> npcDialogueIndex = new();

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
        progress.Clear();
        npcDialogueIndex.Clear();
        LoadProgress();
    }

    public void SaveProgress()
    {
        DialogueSaveData data = new DialogueSaveData();
        foreach(var prog in progress)
        {
            NPCDialogueProgress npc = new NPCDialogueProgress
            {
                //npc id
                ID = prog.Key,
                //what dialogue
                currentProgress = prog.Value,
                //what line of the dialogue
                lineIndex = GetLineIndex(prog.Key)
            };

            data.npcProgress.Add(npc);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
    }

    public void LoadProgress()
    {
        if(!File.Exists(GetSavePath())) return;

        string json = File.ReadAllText(GetSavePath());
        DialogueSaveData data = JsonUtility.FromJson<DialogueSaveData>(json);

        progress.Clear();
        npcDialogueIndex.Clear();

        foreach(var npc in data.npcProgress)
        {
            progress[npc.ID] = npc.currentProgress;
            npcDialogueIndex[npc.ID] = npc.lineIndex;
        }
    }

    public int GetProgress(string npcID)
    {
        if(string.IsNullOrEmpty(npcID)) return 0;
        if(!progress.TryGetValue(npcID, out int prog))
        {
            progress[npcID] = 0;
            return 0;
        }
        return prog;
    }

    public void AdvanceProgress(string npcID, List<DialogueProgression> dialogues)
    {
        if(string.IsNullOrEmpty(npcID)) return;

        if(!progress.ContainsKey(npcID)) progress[npcID] = 0;

        progress[npcID]++;

        if(progress[npcID] >= dialogues.Count - 1)
        {
            progress[npcID] = dialogues.Count - 1;
        }

        SaveProgress();
    }

    public void SetProgress(string npcID, int targetIndex, bool resetLine = true)
    {
        if(string.IsNullOrEmpty(npcID)) return;

        progress[npcID] = Mathf.Max(0,targetIndex);

        if (resetLine)
        {
            npcDialogueIndex[npcID] = 0;
        }

        SaveProgress();
    }

    public int GetLineIndex(string npcID)
    {
        return npcDialogueIndex.TryGetValue(npcID, out int index) ? index : 0;
    }

    public void SetLineIndex(string npcID, int index)
    {
        npcDialogueIndex[npcID] = Mathf.Max(0, index);
        SaveProgress();
    }
    public void ResetLineIndex(string npcID)
    {
        npcDialogueIndex[npcID] = 0;
        SaveProgress();
    } 

    string GetSavePath()
    {
        return SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Dialogue.json");
    }


}
