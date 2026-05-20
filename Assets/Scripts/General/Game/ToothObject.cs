using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[CreateAssetMenu(fileName = "ToothObject", menuName = "Player/ToothObject")]
public class ToothObject : ScriptableObject
{
    [Header("Key")]
    public string ID;
    [Header("State")]
    public bool Added;
}