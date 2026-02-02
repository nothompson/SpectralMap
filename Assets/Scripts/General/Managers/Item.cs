using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[CreateAssetMenu(fileName = "Item", menuName = "Player/Item")]
public class Item : ScriptableObject
{
    public string ID;
    public string Name;
    [TextArea(3,10)]
    public string Description;
    public Sprite ItemDisplay;
    public Sprite[] AnimationSprites;
    public bool AnimationDirection;
    public int AnimationFPS;
    public Vector2Int PositionOnGrid;
    public bool IsInInventory;
}

[Serializable]
public class ItemData
{
    public string ID;
    public bool IsInInventory;
    public Vector2Int PositionOnGrid;
}


[Serializable]
public class InventoryData
{
    public List <ItemData> InventoryItems = new();
}
