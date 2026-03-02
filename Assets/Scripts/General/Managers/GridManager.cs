using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;


public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public Dictionary<Vector2Int, ItemSlot> Slots = new();

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterSlot(ItemSlot slot)
    {
        Slots[slot.GridPosition] = slot;
    }

    public bool TryGetSlot(Vector2Int position, out ItemSlot slot)
    {
        return Slots.TryGetValue(position, out slot);
    }

    public bool GetNextAvailableSpot(out ItemSlot slot)
    {
        slot = null;
        var available = Slots.Where(kvp => kvp.Value.CurrentItem == null).OrderBy(kvp => kvp.Key.y).ThenBy(kvp => kvp.Key.x).FirstOrDefault();

        if(available.Value == null) return false;
        slot = available.Value;
        return true;
    }

    public bool IsOccupied(Vector2Int position)
    {
        return Slots.TryGetValue(position, out var slot) && slot.CurrentItem != null;
    }

    public void ClearAll()
    {
        foreach(var slot in Slots.Values)
        {
            slot.Clear();
        }
    }
}
