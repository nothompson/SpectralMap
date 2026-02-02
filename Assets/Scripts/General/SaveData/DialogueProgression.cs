using UnityEngine;
using System;

[Serializable]
public class DialogueProgression
{
    [TextArea(3,10)]
    public string[] lines;

    public bool requiresJournalEntry;
    public string prereqId;
    public int prereqIndex;
    public bool addToJournal;
    public string journalID;
    public int journalIndex;
    public bool advance;
    public int lineIndexToAddJournalEntry;
    public bool repeat;

}
