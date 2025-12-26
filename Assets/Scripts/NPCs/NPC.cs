using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class NPC : MonoBehaviour, IInteractable
{

    [System.Serializable]
    public class PartConfig
    {
        public string partName;
        public string[] addressableKeys;
        public GameObject mountPoint; 
        public MeshFilter meshFilter;
        public MeshRenderer meshRender;
        
        public BodyConfig[] offsets;

        public HandConfig[] hands;

        public HeadConfig[] heads;
    }

    [System.Serializable]
    public class HeadConfig
    {
        public string addressableKey;
        public Vector3 offset;
        public Vector3 rotation;
    }

    [System.Serializable]
    public class BodyConfig
    {
        public string addressableKey;
        public Vector3 headOffset;
        public Vector3 bodyOffset;
        public Vector3 bodyRotation;
        public Vector3 leftHandOffset;
        public Vector3 rightHandOffset;
        public bool overrideScale;
        public Vector3 headScale;
        public Vector3 scale;
        public float handScale;
        public float colliderHeight;
        public float colliderRadius;
        public Vector3 colliderCenter;
    }
    [System.Serializable]
    public class HandConfig
    {
        public string addressableKey;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector3 offsetPosition;
        public bool overridePosition;
        public Vector3 position;
    }

    [SerializeField] PartConfig[] parts;

    [SerializeField] GameObject head;
    [SerializeField] FOV fov;
    private GameObject hudbox;

    private GameObject Text;

    private SpriteAnimate spriteAnimate;

    private SpriteText dialogue;

    public Coroutine textboxAnimation;

    public NPCDialogue dialogueData;

    int index = 0;

    private List<string> currentWords = new List<string>();
    private List<AsyncOperationHandle<GameObject>> loadedParts = new();

    private Coroutine Speak;

    private MeshJitter meshJitter;

    private Quaternion headRotation;

    Quaternion rotation; 

    bool ableToSeePlayer;

    bool playerHasInteracted = false;

    public async void Start()
    {
        ableToSeePlayer = false;
        await LoadRandomParts();
        headRotation = head.transform.localRotation;

        fov = gameObject.GetComponent<FOV>();
        meshJitter = head.GetComponent<MeshJitter>();

        RefreshMeshJitter();

        meshJitter.rotation = headRotation;

        ableToSeePlayer = true;

        if (dialogueData.AddToJournal)
        {
            dialogueData.added = false;
        }
    }

    private void RefreshMeshJitter()
    {
        MeshJitter[] jitters = GetComponentsInChildren<MeshJitter>();
        foreach(var jit in jitters)
        {
            jit.UpdateBaseValues();
        }
    }

    private async Task LoadRandomParts()
    {
        string spawnedHead = null;
        string spawnedBodyKey = null;
        string spawnedLeftHand = null;
        string spawnedRightHand = null;
        foreach(var config in parts)
        {
            if(config.addressableKeys == null || config.addressableKeys.Length < 1) continue;

            string randomKey = config.addressableKeys[Random.Range(0, config.addressableKeys.Length)];

            if(config.partName == "Head")
            {
                spawnedHead = randomKey;
            }

            if(config.partName == "Body")
            {
                spawnedBodyKey = randomKey;
            }

            if(config.partName == "LeftHand")
            {
                spawnedLeftHand = randomKey;
            }

            if(config.partName == "RightHand")
            {
                spawnedRightHand = randomKey;
            }


            var assetHandle = Addressables.LoadAssetAsync<GameObject>(randomKey);
            await assetHandle.Task;

            loadedParts.Add(assetHandle);

            GameObject prefab = assetHandle.Result;
            GameObject part = Instantiate(prefab, config.mountPoint.transform);

            MeshFilter filter = part.GetComponent<MeshFilter>();
            MeshRenderer render = part.GetComponent<MeshRenderer>();

            if(config.meshFilter != null && filter != null)
            {
                config.meshFilter.sharedMesh = filter.sharedMesh;
            }

            MeshRenderer targetRender = config.mountPoint.GetComponent<MeshRenderer>();
            if(targetRender !=null && render != null)
            {
                targetRender.sharedMaterials = render.sharedMaterials;
            }
        }

        if (!string.IsNullOrEmpty(spawnedBodyKey) || !string.IsNullOrEmpty(spawnedLeftHand) || !string.IsNullOrEmpty(spawnedRightHand))
            {
                PartConfig bodyConfig = System.Array.Find(parts, p=> p.partName == "Body");
                if(bodyConfig?.offsets != null)
                {
                    BodyConfig bodyOffsets = System.Array.Find(bodyConfig.offsets, e => e.addressableKey == spawnedBodyKey);
                    if(bodyOffsets != null)
                    {
                        if(bodyOffsets.bodyOffset != Vector3.zero && bodyConfig.mountPoint != null)
                        {
                            bodyConfig.mountPoint.transform.localPosition += bodyOffsets.bodyOffset;
                            bodyConfig.mountPoint.transform.localRotation = Quaternion.Euler(bodyOffsets.bodyRotation);
                            if (bodyOffsets.overrideScale)
                            {
                                bodyConfig.mountPoint.transform.localScale = bodyOffsets.scale;
                                CapsuleCollider collider = GetComponent<CapsuleCollider>();
                                if(collider !=null){
                                    collider.height = bodyOffsets.colliderHeight;
                                    collider.radius = bodyOffsets.colliderRadius;
                                    collider.center = bodyOffsets.colliderCenter;
                                }
                            }
                        }

                        PartConfig headPart = System.Array.Find(parts, p=>p.partName == "Head");
                        if(headPart != null)
                        {
                            HeadConfig headConfig = System.Array.Find(headPart.heads, e => e.addressableKey == spawnedHead);
                            if(headConfig != null)
                            {
                                headPart.mountPoint.transform.localPosition += bodyOffsets.headOffset + headConfig.offset;
                                headPart.mountPoint.transform.localRotation = Quaternion.Euler(headConfig.rotation);
                                if (bodyOffsets.overrideScale)
                                {
                                    headPart.mountPoint.transform.localScale = bodyOffsets.headScale;
                                }
                            }
                        }


                        PartConfig leftHandConfig = System.Array.Find(parts, _p=>_p.partName == "LeftHand");
                        if(leftHandConfig != null)
                        {
                            leftHandConfig.mountPoint.transform.localPosition += bodyOffsets.leftHandOffset;
                            HandConfig leftHandRotation = System.Array.Find(leftHandConfig.hands, e => e.addressableKey == spawnedLeftHand);
                            if(leftHandRotation != null)
                            {
                                leftHandConfig.mountPoint.transform.localRotation = Quaternion.Euler(leftHandRotation.rotation);
                                leftHandConfig.mountPoint.transform.localScale = leftHandRotation.scale;
                                leftHandConfig.mountPoint.transform.localPosition += leftHandRotation.offsetPosition;
                                if (leftHandRotation.overridePosition)
                                {
                                    leftHandConfig.mountPoint.transform.localPosition = leftHandRotation.position;
                                }
                                if (bodyOffsets.overrideScale)
                                {
                                    leftHandConfig.mountPoint.transform.localScale *= bodyOffsets.handScale;
                                }
                            }
                        }

                        PartConfig rightHandConfig = System.Array.Find(parts, _p=>_p.partName == "RightHand");
                        if(rightHandConfig != null)
                        {
                            rightHandConfig.mountPoint.transform.localPosition += bodyOffsets.rightHandOffset;
                            HandConfig rightHandRotation = System.Array.Find(rightHandConfig.hands, e => e.addressableKey == spawnedRightHand);
                            if(rightHandRotation != null)
                            {
                                rightHandConfig.mountPoint.transform.localRotation = Quaternion.Euler(rightHandRotation.rotation);
                                rightHandConfig.mountPoint.transform.localScale = rightHandRotation.scale;
                                rightHandConfig.mountPoint.transform.localPosition += rightHandRotation.offsetPosition;
                                if (rightHandRotation.overridePosition)
                                {
                                    rightHandConfig.mountPoint.transform.localPosition = rightHandRotation.position;
                                }
                                if (bodyOffsets.overrideScale)
                                {
                                    rightHandConfig.mountPoint.transform.localScale *= bodyOffsets.handScale;
                                }
                            }
                        }
                    }
                }
            }
    }

    public void Update()
    {
        if(ableToSeePlayer){
        if(fov.canSeePlayer){
            Vector3 adjusted = new Vector3(fov.player.transform.position.x, fov.player.transform.position.y + 1.75f, fov.player.transform.position.z);
            Vector3 direction = (adjusted - head.transform.position).normalized;

            Vector3 localDir = Quaternion.Inverse(transform.rotation) * direction;
            
            float yAngle = Mathf.Atan2(localDir.x,localDir.z) * Mathf.Rad2Deg;
            float xAngle = Mathf.Asin(-localDir.y) * Mathf.Rad2Deg;

            yAngle = Mathf.Clamp(yAngle,-90f,90f);
            xAngle = Mathf.Clamp(xAngle,-30f,30f);

            rotation = Quaternion.Euler(xAngle,yAngle,0f) * headRotation;
        }
        else
        {
            rotation = headRotation;
        }

        meshJitter.targetRot = rotation;
        }
    }

    void OnDestroy()
    {
        foreach(var part in loadedParts)
            Addressables.Release(part);
        loadedParts.Clear();
    }

    public void OnInteract(GameObject player)
    {
        //references from interactor(player)
        PlayerControlRigid pcr = player.GetComponent<PlayerControlRigid>();
        if(!fov.canSeePlayer || pcr.paused) return;

        hudbox = player.transform.Find("YawPivot/Camera/overlay/Textbox/hudbox")?.gameObject;

        Text = hudbox.transform.Find("Text")?.gameObject;

        dialogue = Text.GetComponent<SpriteText>();

        hudbox.SetActive(true);

        spriteAnimate = hudbox.GetComponent<SpriteAnimate>();

        //stop previous opening/closing animation 
        if(textboxAnimation != null)
        {
            StopCoroutine(textboxAnimation);
        }
        //if typing, interact again to skip typewriting
        if (dialogue.isTyping)
        {
            dialogue.ShowText(this);
            return;
        }
        //if full text is shown and interact, go to next line(if any)
        else if (dialogue.fullTextShown && dialogueData.dialogue.Length > 0)
        {
            if(index < dialogueData.dialogue.Length - 1){
                index++;
            //if repeat, then cycle back to first line
            if(dialogueData.repeat){
                if(index >= dialogueData.dialogue.Length)
                {
                    index = 0;
                }
            }
            }
            else
            {
                //otherwise, clamp to last line
                if(index >= dialogueData.dialogue.Length)
                {
                    index = dialogueData.dialogue.Length - 1;
                }
            }
        }
        //open hud 
        textboxAnimation = AnimateTextbox(spriteAnimate, spriteAnimate.sprites.Length - 1);
        
        if(AudioManager.Instance != null && !playerHasInteracted){
            playerHasInteracted = true;
            AudioManager.Instance.TextOpen();
        }

        Image image = hudbox.GetComponent<Image>();

        if (dialogueData.AddToJournal && index == dialogueData.indexToAddEntry && !dialogueData.added)
        {
            dialogueData.JournalEntry();
            dialogueData.added = true;
        }
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
        //stop previous animation
        if(textboxAnimation != null)
        {
            StopCoroutine(textboxAnimation);
        }

        //if text has appeared, reset 
        if(Text != null){
            Text.SetActive(false);
            dialogue.StopTypewriter(this);
            if(Speak != null)
            {
                StopCoroutine(Speak);
                Speak = null;
            }
            if(dialogueData.reset){
                index = 0;
            }
            else 
            {
                if(dialogue.fullTextShown)
                    index+=1;
                if(index >= dialogueData.dialogue.Length)
                {
                    index = dialogueData.dialogue.Length - 1;
                }
            }
            dialogue.fullTextShown = false;

        }

        //shrink hud, and disable 
        textboxAnimation = AnimateTextbox(spriteAnimate, 0, disable: true);
        if (playerHasInteracted)
        {
            playerHasInteracted = false;
            if(AudioManager.Instance != null){
                        AudioManager.Instance.TextClose();
            }
        }
    }

    public Coroutine AnimateTextbox(SpriteAnimate sprite, int targetFrame, bool disable = false)
    {
        if(sprite == null) return null;

        return sprite.AnimateTo(
            script: this,
            targetFrame: targetFrame,
            onFrameChanged: frame =>
            {
                if(targetFrame != 0 && frame!= targetFrame)
                {
                    Text.SetActive(false);
                }
            },
            onTarget: () =>
            {
                if(targetFrame != 0)
                {
                    DisplayDialogue();
                }
                else if(disable && targetFrame == 0)
                {
                    hudbox.SetActive(false);
                }
            }
        );
    }

    public void DisplayDialogue()
    {
        //reached target, now can show text
        Text.SetActive(true);
        //clear previous typing
        if(dialogue.typing != null)
        {
        dialogue.StopTypewriter(this);
        }
        if (Speak != null)
        {
            StopCoroutine(Speak);
            Speak = null;
        }
        //get line from index
        dialogue.input = dialogueData.dialogue[index];

        GetWords(dialogueData.dialogue[index]);
        //start typing
        dialogue.StartTypewriter(this, dialogueData.speed);

        Speak = StartCoroutine(SayWords());

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
