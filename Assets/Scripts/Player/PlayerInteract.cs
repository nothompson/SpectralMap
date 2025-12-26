using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] LayerMask targetMask;
    [SerializeField] float dist = 3f;

    [SerializeField] private GameObject speakSprite;
    [SerializeField] private GameObject pressSprite;

    private IInteractable currentInteract;
    private InteractionType? currentSprite = null;

    void Update()
    {
        Camera cam = Camera.main;
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            RaycastHit hit;
            bool ableToInteract = Physics.Raycast(ray, out hit, dist, targetMask); // 5 units distance, adjust as needed
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
        }

        currentSprite = type;
    }

    void ClearSprites()
    {
        speakSprite.SetActive(false);
        pressSprite.SetActive(false);

        currentSprite = null;
    }
}
