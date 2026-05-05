using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] LayerMask targetMask;
    [SerializeField] float dist = 3f;

    [SerializeField] private GameObject speakSprite;
    [SerializeField] private GameObject grabSprite;
    [SerializeField] private GameObject pressSprite;

    private IInteractable currentInteract;
    private InteractionType? currentSprite = null;

    [SerializeField] public GameObject hudbox;

    [SerializeField] public GameObject Text;

    [SerializeField] public SpriteAnimate spriteAnimate;

    [SerializeField] public SpriteText dialogue;

    public IEnumerator textboxAnimation;

    public bool playerHasInteracted;

    public GameObject HeldObject;

    public Transform ObjectAnchor;

    public bool HoldingObject = false;

    void Update()
    {
        if(currentInteract != null && ((MonoBehaviour)currentInteract) == null)
        {
            ClearSprites();
            currentInteract = null;
            return;
        }
            Camera cam = Camera.main;
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            RaycastHit hit;
            bool ableToInteract = Physics.Raycast(ray, out hit, dist, targetMask);
            if (ableToInteract)
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if(interactable != null && interactable.CanInteract())
                {
                    if(currentInteract != interactable)
                    {
                        currentInteract?.ExitInteract(); 
                        currentInteract = interactable;
                    }
                }
                else
                {
                    currentInteract?.ExitInteract(); 
                    ClearSprites();
                    currentInteract = null;
                }
            }
            else
            {
                currentInteract?.ExitInteract(); 
                ClearSprites();
                currentInteract = null;
            }
        
        if(currentInteract != null)
        {
            ShowSprite(currentInteract.GetInteractionType());
            if(InputManager.Instance.inputs.Player.Interact.triggered){
                currentInteract.OnInteract(gameObject);
            }
        }

        if(InputManager.Instance.inputs.Player.Interact.triggered && HoldingObject && currentInteract == null)
        {
            GrabObject go = HeldObject.GetComponent<GrabObject>();
            if(go == null) return;
            go.Drop(this);
        }
    }

    public IEnumerator AnimateTextbox(int targetFrame, bool disable = false)
    {
        if(spriteAnimate == null) yield break;

        yield return spriteAnimate.AnimateTo(
            script: this,
            targetFrame: targetFrame,
            onFrameChanged: frame =>
            {
                if(targetFrame != 0 && frame != targetFrame)
                {
                    Text.SetActive(false);
                }
            },
            onTarget: () =>
            {
                if(targetFrame != 0) currentInteract?.DisplayDialogue();
                else if (disable && targetFrame == 0) hudbox.SetActive(false);
            }
        );
    }

    public void OpenDialogue()
    {
        hudbox.SetActive(true);

        if(textboxAnimation != null) {StopCoroutine(textboxAnimation); textboxAnimation = null;}

        textboxAnimation = AnimateTextbox(spriteAnimate.sprites.Length - 1);
        StartCoroutine(textboxAnimation);

        if(AudioManager.Instance != null && !playerHasInteracted)
        {
            playerHasInteracted = true;
            AudioManager.Instance.TextOpen();
        }
    }

    public void CloseDialogue()
    {
        if(textboxAnimation != null)
        {
            StopCoroutine(textboxAnimation);
            textboxAnimation = null;
        }

        if(Text != null)
        {
            Text.SetActive(false);
            dialogue?.StopTypewriter(this);
        }

        textboxAnimation = AnimateTextbox(0, disable: true);
        StartCoroutine(textboxAnimation);

        if (playerHasInteracted)
        {
            playerHasInteracted = false;
            AudioManager.Instance?.TextClose();
        }
    }

    void ShowSprite(InteractionType type)
    {
        if(currentSprite == type) return;

        ClearSprites();

        switch (type)
        {
            case InteractionType.Talk:
                speakSprite.SetActive(true);
                break;
            case InteractionType.Press:
                pressSprite.SetActive(true);
                break;
            case InteractionType.Grab:
                grabSprite.SetActive(true);
                break;
        }

        currentSprite = type;
    }

    void ClearSprites()
    {
        speakSprite.SetActive(false);
        pressSprite.SetActive(false);
        grabSprite.SetActive(false);

        currentSprite = null;
    }
}
