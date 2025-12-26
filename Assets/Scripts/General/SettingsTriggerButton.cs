using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsTriggerButton : MonoBehaviour
{
    public void TriggerSettingsOpen()
    {
        SettingsMenu.Instance.Open();
    }
    public void TriggerSettingsClose()
    {
        SettingsMenu.Instance.Close();
    }
}

