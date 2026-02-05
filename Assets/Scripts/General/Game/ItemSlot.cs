using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;


public class ItemSlot : MonoBehaviour, IDropHandler
{
    public Vector2Int GridPosition;
    public DraggableItem CurrentItem;

    void Awake()
    {
        GridManager.Instance.RegisterSlot(this);
    }

    public void SetItem(DraggableItem item)
    {
        CurrentItem = item;
        item.Slot = this;
        item.transform.SetParent(transform, false);
        item.rect.localPosition = Vector3.zero;
    }

    public void Clear()
    {
        if(CurrentItem != null) CurrentItem.transform.SetParent(null);
        CurrentItem = null;
    }

    public void OnDrop(PointerEventData input)
    {
        DraggableItem item = input.pointerDrag.GetComponent<DraggableItem>();
        if(item == null) return;

        ItemSlot slot = item.Slot;

        if(CurrentItem == null){
            item.CommitDrop(GridPosition, this);
        }
        else
        {
            DraggableItem other = CurrentItem;

            if(other == item || slot == null) return;

            slot.Clear();
            Clear();

            slot.SetItem(other);
            this.SetItem(item);

            item.Slot = this;
            other.Slot = slot;

            item.ItemData.PositionOnGrid = this.GridPosition;
            other.ItemData.PositionOnGrid = slot.GridPosition;
        }

        InventoryManager.Instance.SaveInventory();  
    }


}