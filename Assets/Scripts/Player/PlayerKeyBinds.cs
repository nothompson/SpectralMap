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
        InputManager.Instance.inputs.Player.Save.performed += OnSave;
        InputManager.Instance.inputs.Player.Reload.performed += OnReload;
        InputManager.Instance.inputs.Player.Fire.performed += OnFire;
    }

    // Update is called once per frame
    void Update()
    {
        keyBinds();
    }

        void OnDisable()
    {
        if(InputManager.Instance.inputs!= null){
            InputManager.Instance.inputs.Player.Reset.performed -= OnReset;
            InputManager.Instance.inputs.Player.Save.performed -= OnSave;
             InputManager.Instance.inputs.Player.Reload.performed -= OnReload;
              InputManager.Instance.inputs.Player.Fire.performed -= OnFire;
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
        }

        // if (forceRefill)
        // {
        //     playerMagic.magicPoints = playerMagic.maximumMagic;
        // }
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        checkpoint.Reset();

    }

    public void OnSave(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        checkpoint.updateCheckpoint(player.transform);

    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        if (!HUDManager.Instance.reloading)
        {
            HUDManager.Instance.StartReload();
            HUDManager.Instance.reloading = true;
        }
        else
        {
            HUDManager.Instance.StopReload();
            HUDManager.Instance.reloading = false;   
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if(!HUDManager.Instance.reloading) return;

        bool onbeat = AudioManager.Instance.IsOnBeat();

        if (onbeat)
        {
            playerMagic.magicPoints += 25f;
            if(playerMagic.magicPoints >= playerMagic.maximumMagic)
            {
                playerMagic.magicPoints = playerMagic.maximumMagic;
            }
        }
        else
        {
            mana.Error();
            playerMagic.magicPoints -= 5f;
            if(playerMagic.magicPoints <= 0f)
            {
                playerMagic.magicPoints = 0f;
            }
        }

    }

        //     bool onbeat = AudioManager.Instance.IsOnBeat();

        // if (onbeat)
        // {
        //     Debug.Log("on beat!");  
        // }
        // else
        // {
        //     Debug.Log("off beat :(");
        // }

}
