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

    void Start()
    {
        playerControl = player.GetComponent<PlayerControlRigid>();
        playerMagic = player.GetComponent<MagicManagement>();
    }

    // Update is called once per frame
    void Update()
    {
        keyBinds();
    }

    void OnEnable()
    {
        InputManager.Instance.inputs.Player.Reset.performed += OnReset;
        InputManager.Instance.inputs.Player.Save.performed += OnSave;
    }

        void OnDisable()
    {
        InputManager.Instance.inputs.Player.Reset.performed -= OnReset;
        InputManager.Instance.inputs.Player.Save.performed -= OnSave;
    }
    
    public void keyBinds()
    {
        //debuging/creative mode
        // bool forceReset = InputManager.Instance.inputs.Player.Reset.triggered;

        bool forceRefill = InputManager.Instance.inputs.Player.Reload.triggered;

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

        if (forceRefill)
        {
            playerMagic.magicPoints = playerMagic.maximumMagic;
        }
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

}
