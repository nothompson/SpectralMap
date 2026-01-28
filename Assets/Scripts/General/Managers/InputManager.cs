using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public InputSystem_Actions inputs;
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

        inputs = new InputSystem_Actions();

        if (!string.IsNullOrEmpty(SettingsMenu.SettingsData.inputBindings))
        {
            inputs.LoadBindingOverridesFromJson(SettingsMenu.SettingsData.inputBindings);
        }
        inputs.Player.Enable();


    }

    void OnDestroy()
    {
        if(inputs != null)
        {
            inputs.Player.Disable();
            inputs.Dispose();
            inputs = null;
        }
    }
}
