using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UserSounds", menuName = "Player/UserSounds")]
public class UserSounds : ScriptableObject
{
    public FMODUnity.EventReference playerLand;

    public FMODUnity.EventReference playerHurt;

    public FMODUnity.EventReference UIOpen;

    public FMODUnity.EventReference UIClose;

    public FMODUnity.EventReference UIClick;

    public FMODUnity.EventReference ConfigHover;

    public FMODUnity.EventReference TextOpen;

    public FMODUnity.EventReference TextClose;

    
    public FMODUnity.EventReference JournalOpen;

    public FMODUnity.EventReference JournalClose;

     public FMODUnity.EventReference JournalPrevious;

    public FMODUnity.EventReference JournalNext;
    public FMODUnity.EventReference JournalEntry;

        
    public FMODUnity.EventReference BodybagOpen;

    public FMODUnity.EventReference BodybagClose;

    
}
