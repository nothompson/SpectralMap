using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionType
{
    None,
    Talk,
    Press,
}

public interface IInteractable
{
    void OnInteract(GameObject interactor);

    void ExitInteract();

    bool CanInteract();
    
    InteractionType GetInteractionType();
}
