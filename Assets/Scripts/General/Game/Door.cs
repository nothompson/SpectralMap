using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] public DoorObject Data;
    [SerializeField] public SpriteAnimate Animation;
    [SerializeField] public string ID;
    [SerializeField] private string SceneToLoad;
    [SerializeField] private SpriteRenderer Sprite;
    [SerializeField] public BoxCollider boxCollider;

    public void Awake()
    {
        ID = Data.ID;
    }
    public void Start()
    {
        Animation.sprites = Data.sprites;
        Animation.length = Data.sprites.Length;
        StartCoroutine(DelayedApplyState());
        DoorManager.Instance.RegisterDoor(this);
    }
    public bool CanInteract()
    {
        bool interact = !Data.Opened;
        return interact;
    }
    public void ExitInteract(){}

    public InteractionType GetInteractionType()
    {
        return InteractionType.Press;
    }

    public void OnInteract(GameObject player)
    {
        Debug.Log("interact");
        if(!Data.Opened){
        switch (Data.Type)
        {
            case DoorType.Switch:
                //Switch means it is opened remotely. could be button or npc interaction or on event or whatever
                FeedManager.Instance.AddToFeed("Locked, must be opened remotely");
                FMODUnity.RuntimeManager.PlayOneShotAttached(Data.soundbank[0], gameObject);
                //when interacting with door, add to feed opened remotely
                break;

            case DoorType.Key:
                Debug.Log("door is key type");
                bool key = InventoryManager.Instance.HasItem(Data.KeyID);
                bool itemExists = InventoryManager.Instance.ItemLookup.TryGetValue(Data.KeyID, out var keyItem);
                if(!itemExists) {
                    Debug.Log("invalid id!");
                    return;
                    }
                    if (key)
                    {
                        Debug.Log("required key is in inventory, opening");
                        if(Data.RemoveItem) InventoryManager.Instance.RemoveItem(Data.KeyID);
                        Open();
                    }
                    else
                    {
                        Debug.Log("needs key still");
                        FeedManager.Instance.AddToFeed($"Requires {keyItem.Name}");
                        FMODUnity.RuntimeManager.PlayOneShotAttached(Data.soundbank[0], gameObject);
                    }
                break;

            case DoorType.OneSided:
                Vector3 dir = (player.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dir);
                if(dot > 0)
                    {
                        FeedManager.Instance.AddToFeed("Does not open from this side");
                        FMODUnity.RuntimeManager.PlayOneShotAttached(Data.soundbank[0], gameObject);
                    }
                    else
                    {
                        Open();
                    }
                //if interact from in front, add to feed doesnt open from this side
                //otherwise, open()
                break;
            case DoorType.Warp:
                if (!string.IsNullOrEmpty(Data.WarpLocation))
                {
                    FMODUnity.RuntimeManager.PlayOneShotAttached(Data.soundbank[1], gameObject);
                        StartCoroutine(Animation.AnimateToTarget(
                        Animation.sprites.Length - 1,
                        onFrameChanged: null,
                        onTarget: () =>
                        {
                            LevelManager.Instance.LoadScene(Data.WarpLocation);
                        }
                    ));
                }
                break;
        }
        }
    }

    public void Open()
    {
        //Disable box collider and trigger sprite animate animate to index
        Debug.Log("starting open");
        if(Data.Opened) return;

        Debug.Log("not open, animating door");
        
        FMODUnity.RuntimeManager.PlayOneShotAttached(Data.soundbank[1], gameObject);

        StartCoroutine(Animation.AnimateToTarget(
            Animation.sprites.Length - 1,
            onFrameChanged: null,
            onTarget: () =>
            {
                Debug.Log("finished opening door");
                Data.Opened = true;
                DoorManager.Instance.SaveDoorProgress();
                ApplyState();
            }
            ));
    }

    public IEnumerator DelayedApplyState()
    {
        yield return null;
        ApplyState();
    }

    public void ApplyState()
    {
        Animation.index = Data.Opened ? Data.sprites.Length - 1 : 0;
        Sprite.sprite = Data.Opened ? Data.sprites[Data.sprites.Length - 1] : Data.sprites[0];
        boxCollider.enabled = !Data.Opened;
    }

    void OnDisable()
    {
        DoorManager.Instance.UnRegisterDoor(this);
    }
}
