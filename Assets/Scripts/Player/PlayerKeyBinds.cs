using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerKeyBinds : MonoBehaviour
{
    public GameObject player;
    public Checkpoint checkpoint;
    MagicManagement playerMagic;
    PlayerControlRigid playerControl;

    public GameObject PauseMenu;

    private bool reloading = false;

    [SerializeField] ManaUI mana;

    void Start()
    {
        playerControl = player.GetComponent<PlayerControlRigid>();
        playerMagic = player.GetComponent<MagicManagement>();

        InputManager.Instance.inputs.Player.Reset.performed += OnReset;
        // InputManager.Instance.inputs.Player.Save.performed += OnSave;
        InputManager.Instance.inputs.Player.Reload.performed += OnReload;
        InputManager.Instance.inputs.Player.AltFire.performed += OnAltFire;
    }

    // Update is called once per frame
    void Update()
    {
        if(DeathManager.PlayerDead) return;
        keyBinds();
    }

    public void OnDisable()
    {
        if(InputManager.Instance.inputs!= null){
            InputManager.Instance.inputs.Player.Reset.performed -= OnReset;
            // InputManager.Instance.inputs.Player.Save.performed -= OnSave;
             InputManager.Instance.inputs.Player.Reload.performed -= OnReload;
              InputManager.Instance.inputs.Player.AltFire.performed -= OnAltFire;
        }
    }
    
    public void keyBinds()
    {
        //debuging/creative mode
        // bool forceReset = InputManager.Instance.inputs.Player.Reset.triggered;

        // bool forceRefill = InputManager.Instance.inputs.Player.Reload.triggered;

        bool pause = InputManager.Instance.inputs.Player.Menu.triggered;

        if (pause)
        {
            if (!PauseManager.Instance.paused)
            {
                PauseManager.Instance.Pause();
                playerControl.paused = true;
            }
            else
            {
                PauseManager.Instance.Unpause();
                playerControl.paused = false;
            }

            if (SettingsMenu.Instance.Container.activeInHierarchy)
            {
                SettingsMenu.Instance.Close();
            }
        }

        // if (forceRefill)
        // {
        //     playerMagic.magicPoints = playerMagic.maximumMagic;
        // }
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        if(DeathManager.PlayerDead) return;
        if (!context.performed) return;

        if(PauseManager.Instance.paused) return;
        
        CheckpointManager.Instance.ResetPlayerToCheckpoint();

    }

    // public void OnSave(InputAction.CallbackContext context)
    // {
    //     if(DeathManager.PlayerDead) return;
    //     if (!context.performed) return;

    //     if(PauseManager.Instance.paused) return;
        
    //     checkpoint.updateCheckpoint(player.transform);

    // }

    public void OnReload(InputAction.CallbackContext context)
    {
        if(DeathManager.PlayerDead) return;
        if(!context.performed) return;

        if(PauseManager.Instance.paused) return;

        if (!ReloadManager.Instance.reloading)
        {
            if(playerMagic.magicPoints >= playerMagic.maximumMagic) return;
            if(PlayerManager.Instance.HoldingObject) return;
            ReloadManager.Instance.StartReload();
            ReloadManager.Instance.reloading = true;
        }
        else
        {
            ReloadManager.Instance.StopReload();
            ReloadManager.Instance.reloading = false;   
        }
    }

    public void OnAltFire(InputAction.CallbackContext context)
    {
        if(DeathManager.PlayerDead) return;
        if (!context.performed) return;
        
        if(!ReloadManager.Instance.reloading) return;

        if(PauseManager.Instance.paused) return;

        bool onbeat = AudioManager.Instance.IsOnBeat();

        ReloadManager.Instance.ReloadAttempt();

        if (onbeat)
        {
            playerMagic.Replenish(25f);
            if(playerMagic.magicPoints >= playerMagic.maximumMagic)
            {
                playerMagic.magicPoints = playerMagic.maximumMagic;
                ReloadManager.Instance.StopReload(true);
            }
            ReloadManager.Instance.StartSuccess();
        }
        else
        {
            playerMagic.Drain(25f);
            if(playerMagic.magicPoints <= 0f)
            {
                playerMagic.magicPoints = 0f;
            }
            ReloadManager.Instance.StartFailure();
        }
    }

}
