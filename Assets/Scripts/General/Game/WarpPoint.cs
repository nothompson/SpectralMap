using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint : MonoBehaviour, IInteractable
{
    public Transform warpPoint;

    public void OnInteract(GameObject player)
    {

        Rigidbody rb = player.GetComponent<Rigidbody>();

        rb.isKinematic = true;

        rb.position = warpPoint.position;

        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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