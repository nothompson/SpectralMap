using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySoundbank", menuName = "Enemies/EnemySoundbank")]

public class EnemySoundbank : ScriptableObject
{
    public FMODUnity.EventReference step;

    public FMODUnity.EventReference agro;

    public FMODUnity.EventReference hurt;

    public FMODUnity.EventReference idle;

    public FMODUnity.EventReference attacking;

    public FMODUnity.EventReference attack;

    public FMODUnity.EventReference death;
    
}
