using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint : MonoBehaviour, IInteractable
{
    public Transform warpPoint;

    public void OnInteract(GameObject player)
    {

        Rigidbody rb = player.GetComponent<Rigidbody>();

        Launcher launch = player.GetComponentInChildren<Launcher>();

        launch.grapple.Release();

        rb.isKinematic = true;

        rb.position = warpPoint.position;

        rb.isKinematic = false;
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