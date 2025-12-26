using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonSounds : MonoBehaviour
{
    public void OnClickPlaySound(string buttonType = "open")
    {
        if(AudioManager.Instance == null) return;

        if (buttonType == "open")
            AudioManager.Instance.UIOpen();
        
        if (buttonType == "close")
            AudioManager.Instance.UIClose();

        if (buttonType == "click")
            AudioManager.Instance.UIClick();
        
    }

    public void EnterHover(string hoverType = null)
    {
        if(AudioManager.Instance == null) return;

        if (hoverType == "config")
            AudioManager.Instance.StartConfigHover();

        if (hoverType == "journal")
            AudioManager.Instance.JournalOpen();
    }

    public void ExitHover(string exitType = null)
    {
        if(AudioManager.Instance == null) return;

        if (AudioManager.Instance.ConfigInstance.isValid()){
            if(!SettingsMenu.Instance.canvas.activeInHierarchy){
                AudioManager.Instance.ConfigInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                AudioManager.Instance.ConfigInstance.release();
            }
            
        }

        if (exitType == "journal" &&  !JournalManager.Instance.canvas.activeInHierarchy)
            AudioManager.Instance.JournalClose();
    }
}
