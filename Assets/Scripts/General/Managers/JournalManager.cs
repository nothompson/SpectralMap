using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance;

    public GameObject Container;
    public GameObject SubContainer;
    public SpriteAnimate journalSprite;
    [SerializeField] private AnimationCurve transitionCurve;

    public TMP_Text leftSide;
    public TMP_Text rightSide;

    public TMP_Text leftPagination;
    public TMP_Text rightPagination;

    private SpriteText leftText;
    private SpriteText rightText; 
    private SpriteText leftPageNumber;
    private SpriteText rightPageNumber;

    Coroutine transitionRoutine;

    public bool animating = false;

    private JournalEntry[] allEntries;

    public Dictionary<string, HashSet<int>> AddedEntries;

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

        leftText = leftSide.GetComponent<SpriteText>();

        rightText = rightSide.GetComponent<SpriteText>();

        leftPageNumber = leftPagination.GetComponent<SpriteText>();

        rightPageNumber = rightPagination.GetComponent<SpriteText>();
    }

    public void OnSaveChange()
    {
        LoadAllJournalEntries();
        TrackAddedEntries();
        ClearJournal();
        LoadJournal();
    }

    void LoadAllJournalEntries()
    {
        allEntries = Resources.LoadAll<JournalEntry>("JournalEntries");
    }
    
    void TrackAddedEntries()
    {
        AddedEntries = new Dictionary<string, HashSet<int>>();
    }

    public void AddJournalEntry(string ID, int index)
    {
        JournalEntry entry = allEntries.FirstOrDefault(j => j.ID == ID);
        if(entry == null) return;

        if(!AddedEntries.ContainsKey(ID)) AddedEntries[ID] = new HashSet<int>();

        if(!AddedEntries[ID].Add(index)) return;
        
        AddText(entry.Logs[index]);
        UpdatePagination();

        UpdateFeed();

        EventManager.Instance.OnJournalEntry(ID, index);

        SaveJournal();
    }

    void UpdateFeed()
    {
        FeedManager.Instance.AddToFeed("Your Journal Has Been Updated");
        AudioManager.Instance.JournalEntry();
    }

    void SaveJournal()
    {
        JournalSaveData data = new JournalSaveData();

        foreach(var log in AddedEntries)
        {
            JournalRecord record = new JournalRecord();
            record.ID = log.Key;
            record.addedEntries = log.Value.ToList();
            data.Records.Add(record);
        }

        string savedJson = JsonUtility.ToJson(data,true);
        File.WriteAllText(GetSavePath(),savedJson);
    }

    void LoadJournal()
    {
        ClearJournal();
        AddedEntries = new Dictionary<string, HashSet<int>>();
        UpdatePagination();
        string filePath = GetSavePath();
        if(!File.Exists(filePath)) return;

        string jsonInput = File.ReadAllText(filePath);
        JournalSaveData data = JsonUtility.FromJson<JournalSaveData>(jsonInput);

        AddedEntries.Clear();


        foreach(var record in data.Records)
        {
            if(string.IsNullOrEmpty(record.ID)) continue;
            
            AddedEntries[record.ID] = new HashSet<int>(record.addedEntries);
            JournalEntry entry = allEntries.FirstOrDefault(j => j.ID == record.ID);
            
            if(entry == null) continue;

            foreach(int index in record.addedEntries.OrderBy(i => i))
            {
                if(index >= 0 && index < entry.Logs.Count)
                {
                    AddText(entry.Logs[index]);
                }
            }
        }

    }

    public void SearchEntries(string input)
    {
        SetText("");
        string filePath = GetSavePath();
        if(!File.Exists(filePath)) return;

        string jsonInput = File.ReadAllText(filePath);
        JournalSaveData data = JsonUtility.FromJson<JournalSaveData>(jsonInput);

        AddedEntries.Clear();


        Regex regex = new Regex(input, RegexOptions.IgnoreCase);

        foreach(var record in data.Records)
        {
            if(string.IsNullOrEmpty(record.ID)) continue;
            
            AddedEntries[record.ID] = new HashSet<int>(record.addedEntries);
            JournalEntry entry = allEntries.FirstOrDefault(j => j.ID == record.ID);
            
            if(entry == null) continue;

            foreach(int index in record.addedEntries.OrderBy(i => i))
            {
                if(index >= 0 && index < entry.Logs.Count)
                {
                    if(regex.IsMatch(entry.Logs[index]))
                    {
                        AddText(entry.Logs[index]);
                    }
                }
            }
        }
        UpdatePagination();
    }

    public void ClearSearch()
    {
        LoadJournal();
    }

    public void ClearJournal()
    {
        leftText.input = "";
        rightText.input = "";

        leftText.Refresh();
        rightText.Refresh();

          leftSide.text = "";
        rightSide.text = "";
        leftSide.ForceMeshUpdate();
        rightSide.ForceMeshUpdate();
        
    }

    public bool HasJournalEntry(string id, int index)
    {
        if(!AddedEntries.ContainsKey(id)) return false;

        return AddedEntries[id].Contains(index);
    }

    void SetText(string input)
    {
        leftText.input = input;
        rightText.input = input;

        leftText.Refresh();
        rightText.Refresh();
    }

    string GetSavePath()
    {
        return SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Journal.json");
    }

    public void Open()
    {
        if(animating) return;

        if(SettingsMenu.Instance.animating || InventoryManager.Instance.animating) return;

        PauseManager.Instance.TriggerRaycasts(false);

        SubContainer.SetActive(false);

        Container.SetActive(true);

        journalSprite.index = journalSprite.sprites.Length - 1;

        StartTransition(true);

        StartCoroutine(journalSprite.AnimateToTarget(0, null, () =>
        {
            animating = false;
            SubContainer.SetActive(true);
        }));
        animating = true;
        
        AudioManager.Instance.JournalOpen();
    }

    public void Close()
    {
        if(animating) return;

        PauseManager.Instance.TriggerRaycasts(true);

        journalSprite.index = 0;

        StartTransition(false);

        SubContainer.SetActive(false);
        StartCoroutine(journalSprite.AnimateToTarget(journalSprite.sprites.Length - 1, null, () =>
        {
            Container.SetActive(false);
            animating = false;
        }));
        animating = true;
        AudioManager.Instance.JournalClose();
    }


    public void AddText(string input)
    {
        leftText.input += input + "\n\n";
        rightText.input += input + "\n\n";

        leftText.Refresh();
        rightText.Refresh();
    }

    private void UpdatePagination()
    {
        leftPageNumber.input = leftSide.pageToDisplay.ToString();
        rightPageNumber.input = rightSide.pageToDisplay.ToString();

        leftPageNumber.Refresh();
        rightPageNumber.Refresh();
    }

    public void PreviousPage()
    {
        if(leftSide.pageToDisplay <= 1)
        {
            leftSide.pageToDisplay = 1;
            return;
        }

        if(leftSide.pageToDisplay -2 > 1)
        {
            leftSide.pageToDisplay -= 2;
        }
        else
        {
            leftSide.pageToDisplay = 1;
        }

        rightSide.pageToDisplay = leftSide.pageToDisplay + 1;

        AudioManager.Instance.JournalPrevious();

        UpdatePagination();
    }

    public void NextPage()
    {
        if(rightSide.pageToDisplay >= rightSide.textInfo.pageCount) return;

        if(leftSide.pageToDisplay >= leftSide.textInfo.pageCount - 1)
        {
            leftSide.pageToDisplay = leftSide.textInfo.pageCount - 1;
            rightSide.pageToDisplay = leftSide.pageToDisplay + 1;
        }
        else
        {
            leftSide.pageToDisplay += 2;
            rightSide.pageToDisplay = leftSide.pageToDisplay + 1;
        }

        AudioManager.Instance.JournalNext();

        UpdatePagination();
    }

    public void StartTransition(bool intro)
    {
        if(transitionRoutine != null) {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
            }
        transitionRoutine = StartCoroutine(Transition(intro));
    }

    IEnumerator Transition(bool intro)
    {
        float t = 0f;
        float dur = 0.5f;

        Vector3 target = intro ? Vector3.one : Vector3.zero;
        RectTransform rect = Container.GetComponent<RectTransform>();
        Vector3 starting;

        if (intro)
        {
            rect.localScale = Vector3.zero;
            starting = Vector3.one;
        }
        else
        {
            starting = rect.localScale;
        }
        while(t < dur)
        {
            t += Time.unscaledDeltaTime;
            float time = Mathf.Clamp01(t / dur);
            float elapsed = intro ? time : 1f - time;
            float value = transitionCurve.Evaluate(elapsed);

            rect.localScale = starting * value;
            yield return null;
        }
        rect.localScale = target;
        transitionRoutine = null;
    }
}
