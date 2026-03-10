    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
public class SaveSlotButton : MonoBehaviour
{
    [SerializeField] private SpriteText SaveName;
    
    public void SetState(SaveData data)
    {
        bool saved = data != null && data.hasData;

        SaveName.input = saved ? data.playerName : "Empty";
        SaveName.Refresh();
    }
}
