using UnityEngine;
using System;

[Serializable]
public class DialogueProgression
{
    [TextArea(3,10)]
    public string[] lines;

    [Header("Requirements")]
    public bool requiresJournalEntry;
    public string prereqId;
    public int prereqIndex;

    public bool requiresItem;
    public string requiredItemID;
    [Header("Additions")]
    public bool addToJournal;
    public string journalID;
    public int journalIndex;
    public int lineIndexToAddJournalEntry;
    public bool addItem;
    public string itemToAddID;
    public int lineIndexToAddItem;

    public bool addTooth;

    public ToothObject toothData;
    public int lineIndexToAddTooth;

    [Header("Dialogue Options")]
    public bool advance;
    public bool repeat;

}
