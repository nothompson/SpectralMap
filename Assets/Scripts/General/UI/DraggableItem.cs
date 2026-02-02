using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;


public class DraggableItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Item ItemData;
    public RectTransform rect;
    protected Canvas ParentCanvas;
    protected static DraggableItem draggedItem; 

    [HideInInspector] public Transform draggedParent;
    public Image image;
    private ItemSlot Slot;

    private bool Dragging = false;
    private bool Hovering = false;

    protected virtual void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        ParentCanvas = GetComponentInParent<Canvas>();
        HideDisplay();
    }

    void Start()
    {
        image.sprite = ItemData.ItemDisplay;
    }
    public void OnPointerEnter(PointerEventData input)
    {
        Hovering = true;
        ShowDisplay();
    }
    public void OnPointerExit(PointerEventData input)
    {
        if(!Dragging) {
            Hovering = false;
            HideDisplay();
        }
    }

    public void ShowDisplay()
    {
        if(ItemData.AnimationSprites.Length > 1){
        InventoryManager.Instance.DisplayAnimate.sprites = ItemData.AnimationSprites;
        InventoryManager.Instance.DisplayAnimate.direction = ItemData.AnimationDirection;
        InventoryManager.Instance.DisplayAnimate.fps = ItemData.AnimationFPS;
        InventoryManager.Instance.DisplayAnimate.length = ItemData.AnimationSprites.Length;
        InventoryManager.Instance.DisplayAnimate.Play();
        }
        else
        {
            InventoryManager.Instance.ItemToDisplay.sprite = ItemData.ItemDisplay;
        }

        InventoryManager.Instance.DescriptionText.input = ItemData.Description;
        InventoryManager.Instance.DescriptionText.Refresh();

        InventoryManager.Instance.TitleText.input = ItemData.Name;
        InventoryManager.Instance.TitleText.Refresh();

        InventoryManager.Instance.Display.SetActive(true);
        InventoryManager.Instance.ItemToDisplay.color = new Color(1f,1f,1f,1f);
    }

    public void HideDisplay()
    {
        InventoryManager.Instance.ItemToDisplay.color = new Color(1f,1f,1f,0f);
        InventoryManager.Instance.Display.SetActive(false);

        InventoryManager.Instance.DisplayAnimate.sprites = null;
        InventoryManager.Instance.DisplayAnimate.length = 0;
        InventoryManager.Instance.DisplayAnimate.direction = true;
        InventoryManager.Instance.DisplayAnimate.fps = 0;
        InventoryManager.Instance.DisplayAnimate.isPlaying = false;

        InventoryManager.Instance.DescriptionText.input = null;
        InventoryManager.Instance.DescriptionText.Refresh();

        InventoryManager.Instance.TitleText.input = null;
        InventoryManager.Instance.TitleText.Refresh();
    }

    public void OnPointerDown(PointerEventData input)
    {
        
    }
    public void OnPointerUp(PointerEventData input)
    {
        
    }

    public virtual void OnBeginDrag(PointerEventData input)
    {
        Dragging = true;
        Slot = GetComponentInParent<ItemSlot>();
        Slot?.Clear();
       
        transform.SetParent(ParentCanvas.transform, true);
        transform.SetAsLastSibling();
        image.raycastTarget = false;

        if(Hovering) ShowDisplay();
    }

    public virtual void OnDrag(PointerEventData input)
    {
        rect.position = input.position;
    }

    public virtual void OnEndDrag(PointerEventData input)
    {
        Dragging = false;
        image.raycastTarget = true;

        if(transform.parent == ParentCanvas.transform && Slot != null)
        {
            Slot.SetItem(this);
        }

        if(!Hovering) HideDisplay();
    }

    public void CommitDrop(Vector2Int GridPosition, ItemSlot TargetSlot)
    {
        ItemData.PositionOnGrid = GridPosition;

        Slot?.Clear();
        TargetSlot.SetItem(this);

        InventoryManager.Instance.SaveInventory();
    }
}