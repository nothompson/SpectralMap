using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

[Serializable]
public class CharacterDeathLog
{
    public string ID;
    public bool dead;
}

[Serializable]
public class DeathData
{
    public List<CharacterDeathLog> CharacterDeaths = new();
}