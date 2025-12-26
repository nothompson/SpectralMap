using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogue", menuName = "NPC/NPCDialogue")]
public class NPCDialogue : ScriptableObject
{
    [TextArea(3,10)]
    public string[] dialogue;
    public float speed;
    public bool reset;
    public bool repeat;
    public FMODUnity.EventReference soundbank;
    public bool AddToJournal;
    public int indexToAddEntry;
    [TextArea(3,10)]
    public string TextToJournal;
    public bool added;
    public void JournalEntry()
    {
        if (AddToJournal && JournalManager.Instance != null && !added)
        {
            FeedManager.Instance.AddToFeed("Your Journal Has Been Updated");
            JournalManager.Instance.AddText(TextToJournal);
        }
        else
        {
            Debug.Log("failed journal entry");
        }
    }
}
