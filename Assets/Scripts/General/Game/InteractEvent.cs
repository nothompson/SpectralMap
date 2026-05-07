using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.Events;


public class InteractEvent : MonoBehaviour, IInteractable
{
    public UnityEvent Interaction;
    public void OnInteract(GameObject player)
    {
        Interaction?.Invoke();
    }
    public void ExitInteract()
    {
        
    }

    public bool CanInteract()
    {
        return true;
    }

    public InteractionType GetInteractionType() => InteractionType.Press;
}