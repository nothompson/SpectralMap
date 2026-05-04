using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabObject : MonoBehaviour, IInteractable
{
    public void OnInteract(GameObject player)
    {
        
    }

    public void ExitInteract()
    {
        
    }

    public bool CanInteract()
    {
        return true;
    }

    public InteractionType GetInteractionType() => InteractionType.Grab;
}