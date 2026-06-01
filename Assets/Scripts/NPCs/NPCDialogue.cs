using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogue", menuName = "NPC/NPCDialogue")]
public class NPCDialogue : ScriptableObject
{
    public List<DialogueProgression> dialogues;
    public float speed = 35f;
    public bool reset;
    public FMODUnity.EventReference soundbank;

    public DialogueProgression GetCurrentDialogue(string npcID)
    {
        if(DialogueManager.Instance == null || dialogues == null || dialogues.Count < 1) return null;

        int prog = DialogueManager.Instance.GetProgress(npcID);

        prog = Mathf.Clamp(prog,0,dialogues.Count - 1);

        for(int i = prog; i < dialogues.Count; i++)
        {
            DialogueProgression dialogue = dialogues[i];

            bool requirements = !dialogue.requiresJournalEntry || !dialogue.requiresItem 
            || (JournalManager.Instance != null &&  JournalManager.Instance.HasJournalEntry(dialogue.prereqId,dialogue.prereqIndex))
            || (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(dialogue.requiredItemID));

            if(requirements) return dialogue;
        }

        for(int i = 0; i< dialogues.Count; i++)
        {
            DialogueProgression dialogue = dialogues[i];

            bool requirements = !dialogue.requiresJournalEntry || !dialogue.requiresItem 
            || (JournalManager.Instance != null &&  JournalManager.Instance.HasJournalEntry(dialogue.prereqId,dialogue.prereqIndex))
            || (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(dialogue.requiredItemID));

            if(requirements) return dialogue;
        }
        return null;
    }

    public void CompleteDialogue(string npcID, DialogueProgression currentDialogue)
    {
        if(currentDialogue == null || DialogueManager.Instance == null) return;

        int prog = DialogueManager.Instance.GetProgress(npcID);
        int next = Mathf.Clamp(prog + 1, 0, dialogues.Count - 1);
        if(next >= dialogues.Count) return;

        DialogueProgression nextDialogue = dialogues[next];

        if(!nextDialogue.requiresJournalEntry && !nextDialogue.requiresItem){
        
        if (currentDialogue.advance)
            {
                DialogueManager.Instance.AdvanceProgress(npcID, dialogues);
            }
            return;
        }

        if((JournalManager.Instance != null
        && JournalManager.Instance.HasJournalEntry(nextDialogue.prereqId, nextDialogue.prereqIndex)) 
        || (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(nextDialogue.requiredItemID)))
        {
            DialogueManager.Instance.AdvanceProgress(npcID, dialogues);
        }
    }

    public void AddJournalEntry(DialogueProgression dialogue)
    {
        if(dialogue == null || !dialogue.addToJournal || JournalManager.Instance == null) return;

        JournalManager.Instance.AddJournalEntry(dialogue.journalID,dialogue.journalIndex);
    }

    public void AddTooth(DialogueProgression dialogue)
    {
        if(dialogue == null || !dialogue.addTooth || ToothManager.Instance == null || dialogue.toothData == null) return;

        ToothManager.Instance.AddTooth(dialogue.toothData);
    }


    public void AddItem(DialogueProgression dialogue)
    {
        if(dialogue == null || !dialogue.addItem || InventoryManager.Instance == null) {
            Debug.Log("tried to add item but failed");
            return;
        }

        InventoryManager.Instance.AddItem(dialogue.itemToAddID,new Vector2Int(0,0));
    }

    public void ChangeSpectrum(DialogueProgression dialogue)
    {
        if(dialogue == null || !dialogue.changeSpectrum || SpectrumManager.Instance == null) return;
        
        if(dialogue.spectralChange > 0)
        {
            SpectrumManager.Instance.PurifySpectrum(dialogue.spectralChange);
        }
        else
        {
            SpectrumManager.Instance.PolluteSpectrum(dialogue.spectralChange);
        }
        
    }



}
