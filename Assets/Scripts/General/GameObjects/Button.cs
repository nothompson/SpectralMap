using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{

    public FMODUnity.StudioEventEmitter beep;
    public void OnInteract(GameObject interact)
    {
        beep.Play();

        LevelManager.Instance.LoadScene("MainMenu");


    }

    public void ExitInteract()
    {
        
    }

    public bool CanInteract()
    {
        return true;
    }

    public InteractionType GetInteractionType()
    {
        return InteractionType.Press;
    }
}
