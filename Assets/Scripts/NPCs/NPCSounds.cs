using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCSounds", menuName = "NPC/NPCSounds")]

public class NPCSounds : ScriptableObject
{
  public bool looping;
  public FMODUnity.EventReference sound;
  
}
