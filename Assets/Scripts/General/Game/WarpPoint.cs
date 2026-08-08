using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint : MonoBehaviour, IInteractable
{
    public Transform warpPoint;

    public void OnInteract(GameObject player)
    {

        Rigidbody rb = player.GetComponent<Rigidbody>();

        PlayerControlRigid pcr = player.GetComponent<PlayerControlRigid>();

        Launcher launch = player.GetComponentInChildren<Launcher>();

        launch.grapple.Release();

        rb.isKinematic = true;

        pcr.paused = true;

        rb.position = warpPoint.position;

        Vector3 currentVel = pcr.playerVelocity;
        float currentSpeed = currentVel.magnitude;

        Vector3 localForward = pcr.YawPivot.transform.parent.InverseTransformDirection(warpPoint.forward);

        float yaw = Quaternion.LookRotation(localForward).eulerAngles.y;

        pcr.SetYaw(yaw);

        Vector3 newVel = warpPoint.forward * currentSpeed;


        rb.isKinematic = false;

        pcr.playerVelocity = newVel;
        pcr.paused = false;
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