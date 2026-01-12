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

        InputManager.Instance.inputs.Player.Reset.performed += OnReset;
        InputManager.Instance.inputs.Player.Save.performed += OnSave;
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
        }
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
