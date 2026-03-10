
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using TMPro;    
    using System.IO;


[CreateAssetMenu(fileName = "PlayerCharacterSprites", menuName = "Player/PlayerCharacterSprites")]
public class PlayerCharacterSprites : ScriptableObject
{
    [SerializeField] public Sprite[] EyeSprites;
    [SerializeField] public Sprite[] EyeHurtSprites;
    [SerializeField] public Sprite[] MouthSprites;
    [SerializeField] public Sprite[] MouthHurtSprites;
}
