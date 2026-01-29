using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

[System.Serializable]
public class NPCDialogueProgress
{
    public string ID;
    public int currentProgress;
    public int lineIndex;
}

[System.Serializable]
public class DialogueSaveData
{
    public List<NPCDialogueProgress> npcProgress = new();
}