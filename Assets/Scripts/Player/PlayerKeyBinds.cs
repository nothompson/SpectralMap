using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    public void keyBinds()
    {
        //debuging/creative mode
        bool forceReset = InputManager.Instance.inputs.Player.Reset.triggered;
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

        if (forceReset)
        {
            checkpoint.Reset();
        }
        if (forceRefill)
        {
            playerMagic.magicPoints = playerMagic.maximumMagic;
        }
    }

}
