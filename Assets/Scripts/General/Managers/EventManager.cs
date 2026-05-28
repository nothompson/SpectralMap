using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    private GameObject Player;
    private PlayerControlRigid pcr;
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

    public void RegisterPlayer(GameObject player)
    {
        Player = player;
        pcr = Player.GetComponent<PlayerControlRigid>();
    }
    #region General Events
    public void OnKill(string CharacterID)
    {
        if(string.IsNullOrEmpty(CharacterID)) return;
        switch (CharacterID)
        {
            case "daniel":          
                JournalManager.Instance.AddJournalEntry("danieldeath",0);  
                break;
        }
    }

    public void OnAddItem(string itemID)
    {
        if(string.IsNullOrEmpty(itemID)) return;
        switch (itemID)
        {
            case "testitem":
                Debug.Log("i got the googa shrmirt. i should return to daniel");
                break;
        }
    }

    public void OnJournalEntry(string ID, int index)
    {
        if(ID == "infected" && index == 0)
        {
            DialogueManager.Instance.SetProgress("cartographer", 3);
        }

        if(ID == "bloatfruits" && index == 1)
        {
            SpectrumManager.Instance.PurifySpectrum(10);
        }
    }

    public void OnRemoveItem(string itemID)
    {
        if(string.IsNullOrEmpty(itemID)) return;
    }

    public void OnInteract(string ID)
    {
        if(string.IsNullOrEmpty(ID)) return;
        switch (ID)
        {
            case "lockedDoor":
                FeedManager.Instance.AddToFeed("Locked, needs key");
                break;
            case "doorSwitch":
                FeedManager.Instance.AddToFeed("A door opened somewhere");
                break;
            case "onesidedDoor":
                FeedManager.Instance.AddToFeed("Does not open from this side");
                break;
        }
    }

    public bool OnTrick(TrickManager.TrickType trickType)
    {
        return TrickManager.Instance.currentTricks.Any(ty=> ty.Type == trickType);
    }

    public void OnPollute(int current)
    {
        if(current > SpectrumManager.Instance.MaxPollutantLevel * 0.75f)
        {
            InventoryManager.Instance.AddItem("bezoar", new Vector2Int(0,0));
        }
    }

    public void OnPurify(int current)
    {
        
    }

    public bool OnSuccessiveTricks(TrickManager.TrickType trickType, int threshold)
    {
        var tricks = TrickManager.Instance.trickHistory;
        int count = 0;

        for(int i = tricks.Count - 1; i >= 0; i--)
        {
            if(tricks[i].Type == trickType)
            {
                count++;
                if(count >= threshold)
                {
                    return true;
                }
            }
            else
            {
                break;
            }
        }

        return false;
    }

    public bool OnCombo(TrickManager.TrickType[] trickTypes)
    {
        var tricks = TrickManager.Instance.trickHistory;
        if(trickTypes.Length > tricks.Count)
        {
            return false;
        }
        int start = tricks.Count - trickTypes.Length;

        for(int i = 0; i < trickTypes.Length; i++)
        {
            var trick = tricks[start + i];

            if(trick.Type != trickTypes[i]) return false;
        }
        return true;
    }

    public bool OnFinalScore(int threshold)
    {
        return TrickManager.Instance.FinalScore >= threshold;
    }

    public bool OnAccumulatedPoints(int threshold)
    {
        return TrickManager.Instance.storedPoints >= threshold;
    }

    #endregion

    #region Specific Events

    void Update()
    {
        BegsplatChallenge();
    }

    public void TriggerInfectQuest()
    {
        DialogueManager.Instance.SetProgress("infect0", 1);
    }

    public void BegsplatChallenge()
    {
        if(pcr == null) return;

        if(JournalManager.Instance.HasJournalEntry("begsplatchallenge", 1)) return;
        
        if(JournalManager.Instance.HasJournalEntry("begsplatchallenge",0) && !JournalManager.Instance.HasJournalEntry("begsplatchallenge", 1))
        {
            if (OnFinalScore(121393))
            {
                JournalManager.Instance.AddJournalEntry("begsplatchallenge",1);
                DialogueManager.Instance.SetProgress("begsplat", 2);
            }
        }
    }
    #endregion
}
