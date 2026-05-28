using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] public NamedEntity NamedEntity;
    [SerializeField] public ModularBody modularBody;
    public string npcID;

    [SerializeField] GameObject head;
    [SerializeField] FOV fov;

    public NPCDialogue dialogueData;

    public DialogueProgression currentDialogue;

    private List<string> currentWords = new List<string>();

    private IEnumerator Speak;

    private MeshJitter meshJitter;

    private Quaternion headRotation;
    private Quaternion headTargetRotation;

    Quaternion rotation; 

    bool ableToSeePlayer;

    bool playerHasInteracted = false;

    private PlayerInteract playerInteract;

    private Quaternion neckRestRotation;

    public FMOD.Studio.EventInstance speechInstance;

    public bool usingModularBody = true;

    public bool reload = false;

    public void Awake()
    {
        npcID = NamedEntity.Name;
    }

    public async void Start()
    {
        if (DeathManager.Instance.CheckIfDead(npcID))
        {
            Destroy(gameObject);
            return;
        }
        ableToSeePlayer = false;

        if(!usingModularBody) {
            ableToSeePlayer = true;
            return;
            }

        modularBody.OnPartsLoaded += OnBodyReady;
        await modularBody.LoadRandomParts();

        if(reload)
        StartCoroutine(ReloadPartsRoutine());

    }

    private IEnumerator ReloadPartsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            yield return ReloadParts();
        }
    }

    private IEnumerator ReloadParts()
    {
        ableToSeePlayer = false;
        modularBody.UnloadParts();

        modularBody.OnPartsLoaded += OnBodyReady;
        var task = modularBody.LoadRandomParts();
        yield return new WaitUntil(() => task.IsCompleted);
    }

    private void OnBodyReady()
    {
        headRotation = head.transform.localRotation;
        headTargetRotation = headRotation;

        RefreshMeshJitter();

        ableToSeePlayer = true;
    }

    private void RefreshMeshJitter()
    {
        MeshJitter[] jitters = GetComponentsInChildren<MeshJitter>();
        foreach(var jit in jitters)
        {
            jit.UpdateBaseValues();
        }
    }

    public void Update()
{
    if (!ableToSeePlayer) return;

    if(!usingModularBody) return;

    if (fov.canSeePlayer)
    {
        Vector3 adjusted = new Vector3(
            fov.player.transform.position.x,
            fov.player.transform.position.y + 1f,
            fov.player.transform.position.z);

        Vector3 worldDir = (adjusted - head.transform.position).normalized;

        Vector3 localDir = Quaternion.Inverse(transform.rotation) * worldDir;

        float yAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        float xAngle = Mathf.Asin(Mathf.Clamp(-localDir.y, -1f, 1f)) * Mathf.Rad2Deg;

        yAngle = Mathf.Clamp(yAngle, -80f, 80f);
        xAngle = Mathf.Clamp(xAngle, -50f, 50f);

        Quaternion clampedWorld = transform.rotation
            * Quaternion.AngleAxis(yAngle, Vector3.up)
            * Quaternion.AngleAxis(xAngle, Vector3.right);

        headTargetRotation = clampedWorld;
    }
    else
    {
        // headTargetRotation = head.transform.parent != null
        //     ? head.transform.parent.rotation * headRotation
        //     : transform.rotation * headRotation;
        headTargetRotation = transform.rotation * headRotation;
    }

    head.transform.rotation = Quaternion.Slerp(
        head.transform.rotation,
        headTargetRotation,
        Time.deltaTime * 5f);
}

    void OnDestroy()
    {
        if(playerInteract != null)
        {
            playerInteract.CloseDialogue();
        }
    }

    public void OnInteract(GameObject player)
{
    PlayerControlRigid pcr = player.GetComponent<PlayerControlRigid>();
    if(!fov.canSeePlayer || pcr.paused) return;
    playerInteract = player.GetComponent<PlayerInteract>();
    if(playerInteract == null) return;
    
    DialogueProgression next = dialogueData.GetCurrentDialogue(npcID);
    if(currentDialogue != next) {
        currentDialogue = next;
        DialogueManager.Instance.ResetLineIndex(npcID);
        playerInteract.dialogue.fullTextShown = false;
    }

    // Get index AFTER potential reset
    int dialogueIndex = DialogueManager.Instance.GetLineIndex(npcID);

    if(dialogueIndex == currentDialogue.lineIndexToAddJournalEntry && currentDialogue.addToJournal)
    {
        dialogueData.AddJournalEntry(currentDialogue);
    }
    if(dialogueIndex == currentDialogue.lineIndexToAddItem && currentDialogue.addItem)
    {
        dialogueData.AddItem(currentDialogue);
    }

    if(dialogueIndex == currentDialogue.lineIndexToAddTooth && currentDialogue.addTooth)
    {
        dialogueData.AddTooth(currentDialogue);
    }

    if (playerInteract.dialogue.isTyping)
    {
        playerInteract.dialogue.ShowText(playerInteract);
        return;
    }   

    if (playerInteract.dialogue.fullTextShown)
    {
        dialogueIndex++;
        if(dialogueIndex >= currentDialogue.lines.Length)
        {
            dialogueData.CompleteDialogue(npcID, currentDialogue);
            currentDialogue = dialogueData.GetCurrentDialogue(npcID);
            // Reset to 0 always — repeat check is just for non-advancing dialogues
            dialogueIndex = 0;
            DialogueManager.Instance.ResetLineIndex(npcID);
        }
        else
        {
            DialogueManager.Instance.SetLineIndex(npcID, dialogueIndex);
        }
    }

    playerInteract.OpenDialogue();
}

    public InteractionType GetInteractionType()
    {
        return InteractionType.Talk;
    }

    public bool CanInteract()
    {
        return fov.canSeePlayer;
    }

    public void ExitInteract()
    {
        if(playerInteract == null) return;

        playerInteract.CloseDialogue();
        playerInteract = null;
    }

    public void DisplayDialogue()
    {
        if(playerInteract == null) return;

        //reached target, now can show text
        playerInteract.Text.SetActive(true);

        var dialogue = playerInteract.dialogue;
        //clear previous typing
        if(dialogue.typing != null)
        {
            dialogue.StopTypewriter(playerInteract);
        }
        if (Speak != null)
        {
            StopCoroutine(Speak);
            Speak = null;
        }
        int dialogueIndex = DialogueManager.Instance.GetLineIndex(npcID);

        dialogueIndex = Mathf.Clamp(dialogueIndex, 0, currentDialogue.lines.Length - 1);
        DialogueManager.Instance.SetLineIndex(npcID, dialogueIndex);

        //get line from index
        dialogue.input = currentDialogue.lines[dialogueIndex];

        GetWords(dialogue.input);
        //start typing
        dialogue.StartTypewriter(playerInteract, dialogueData.speed);

        Speak = SayWords();
        StartCoroutine(Speak);

        dialogue.fullTextShown = false;

    }

    public void GetWords(string input)
    {
        currentWords.Clear();
        string[] words = input.Split(" ");
        foreach (string word in words)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                currentWords.Add(word.Trim());
            }
        }
    }

    private IEnumerator SayWords()
    {
        string[] words = currentWords.ToArray();
        foreach(string word in words)
        {
            if(dialogueData.soundbank.IsNull) yield break;

            FMODUnity.RuntimeManager.PlayOneShotAttached(dialogueData.soundbank, gameObject);

            yield return new WaitForSeconds(Mathf.Clamp(word.Length * 0.12f, 0.3f,0.7f));
        }
    }

}
