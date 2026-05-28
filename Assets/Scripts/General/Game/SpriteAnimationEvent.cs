
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SpriteAnimationEvent", menuName = "Scriptable Objects/SpriteAnimationEvent")]
public class SpriteAnimationEvent : ScriptableObject
{
    public string id;
    public bool restingAnimation;
    public Sprite[] sprites;
    public int fps;
    public bool pingPong;

    public SpriteAnimationEvent next;

}