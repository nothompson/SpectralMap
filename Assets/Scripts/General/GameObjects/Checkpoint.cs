using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform RespawnPoint;
    
    public void SaveCheckpoint()
    {
        CheckpointManager.Instance.SaveCurrentCheckpoint(RespawnPoint.position);
    }
}
