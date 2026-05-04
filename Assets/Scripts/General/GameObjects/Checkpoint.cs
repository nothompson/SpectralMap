using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameObject player;
    PlayerControlRigid playerControl;

    Grapple grapple;
    MagicManagement magicManagement;

    public Vector3 newPosition;

    bool checkpointSet;

    void Start()
    {
        playerControl = player.GetComponent<PlayerControlRigid>();
        magicManagement = player.GetComponent<MagicManagement>();
        grapple = player.GetComponentInChildren<Grapple>();
    }
    public void updateCheckpoint(Transform playerTransform)
    {

        newPosition = playerTransform.position;
    }
    
    public void saveCheckpoint()
    {
        bool onCheckpoint = Physics.CheckSphere(player.transform.position, 10f, playerControl.checkpointMask);

        if (onCheckpoint && !checkpointSet)
        {
            updateCheckpoint(player.transform);
            checkpointSet = true;
        }
        else if (!onCheckpoint)
        {
            checkpointSet = false;
        }
    }


    public void Reset()
    {
        StartCoroutine(ResetRoutine());

    }

    IEnumerator ResetRoutine()
    {
        ResetManager.Instance.StartReset();

        yield return new WaitForEndOfFrame();

        grapple.Release();

        TrickManager.Instance.ResetCombo();
        
        GibsManager.Instance.Gib(player.transform.position, Random.Range(4,10));

        Rigidbody rb = playerControl.rb;

        rb.isKinematic = true;

        rb.position = newPosition;

        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        playerControl.playerVelocity = Vector3.zero;

        magicManagement.magicPoints = magicManagement.maximumMagic;
    }
}
