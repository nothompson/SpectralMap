using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "JournalEntry", menuName = "Player/JournalEntry")]
public class JournalEntry : ScriptableObject
{
    public string ID; 
    [TextArea(3,10)]
    public List<string> Logs  = new List<string>();
    
}
