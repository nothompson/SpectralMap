using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToJournal : MonoBehaviour
{

    public int AddIndex;
    [System.Serializable]
    public class Entry
    {
        public string logID;
        public int index;
    }


    public Entry[] entries;

    public void Add(int i)
    {
        if(JournalManager.Instance == null) return;

        JournalManager.Instance.AddJournalEntry(entries[i].logID, entries[i].index);
    }
}