using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public Vector3 currentCheckpoint;

    public GameObject player;

    PlayerControlRigid playerControl;

    Grapple grapple;
    MagicManagement magicManagement;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void RegisterPlayer(GameObject go)
    {
        player = go;

        playerControl = player.GetComponent<PlayerControlRigid>();
        magicManagement = player.GetComponent<MagicManagement>();
        grapple = player.GetComponentInChildren<Grapple>();

    }

    public void LoadCurrentCheckpoint()
    {
        if(!File.Exists(GetSavePath())) return;

        string json = File.ReadAllText(GetSavePath());

        CheckpointSaveData data = JsonUtility.FromJson<CheckpointSaveData>(json);

        currentCheckpoint = data.Position;
    }

    public void SaveCurrentCheckpoint(Vector3 position)
    {
        if(currentCheckpoint == position)
        {
            return;
        }

        currentCheckpoint = position;

        FeedManager.Instance.AddToFeed("Checkpoint Saved");

        CheckpointSaveData data = new CheckpointSaveData();

        data.Position = currentCheckpoint;

        string json = JsonUtility.ToJson(data,true);
        File.WriteAllText(GetSavePath(), json);
    }

    public void OnSaveChange()
    {
        currentCheckpoint = Vector3.zero;
        LoadCurrentCheckpoint();
    }

    public void ResetPlayerToCheckpoint()
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

        rb.position = currentCheckpoint;

        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        playerControl.playerVelocity = Vector3.zero;

        magicManagement.magicPoints = magicManagement.maximumMagic;
    }

    string GetSavePath()
    {
        return SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Checkpoint.json");
    }
}