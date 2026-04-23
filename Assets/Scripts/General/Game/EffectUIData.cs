using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Effects/Effect Ui Data")]
public class EffectUIData : ScriptableObject
{
    public string id;
    public string DisplayName;
    public Sprite[] sprites;
    public bool pingPong;
    public int fps; 
}