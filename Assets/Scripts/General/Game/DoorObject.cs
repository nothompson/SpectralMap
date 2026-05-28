using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum DoorType
{
    Key,
    OneSided,
    Switch,
    Warp,
}


[CreateAssetMenu(fileName = "DoorObject", menuName = "Player/DoorObject")]
public class DoorObject : ScriptableObject
{
    [Header("Attributes")]
    public string ID;
    public DoorType Type;
    public string WarpLocation;
    [Header("State")]
    public bool Opened;
    [Header("Art")]
    public Sprite[] sprites;
    public FMODUnity.EventReference[] soundbank;
    [Header("Requirements")]
    public bool RemoveItem;
    public string KeyID;
}

[Serializable]
public class DoorData
{
    public string ID;
    public bool Opened;
}

[Serializable]
public class DoorDataBase
{
    public List<DoorData> Doors = new();
}
