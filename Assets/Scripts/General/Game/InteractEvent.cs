using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.Events;


public class InteractEvent : MonoBehaviour, IInteractable
{
    public UnityEvent Interaction;
    public bool Disable = false;
    bool disabled = false;
    public void OnInteract(GameObject player)
    {
        Interaction?.Invoke();
        if (Disable)
        {
            disabled = true;
        }
    }
    public void ExitInteract()
    {
        
    }

    public bool CanInteract()
    {
        if(disabled) return false;
        else return true;
    }

    public InteractionType GetInteractionType() => InteractionType.Press;
}