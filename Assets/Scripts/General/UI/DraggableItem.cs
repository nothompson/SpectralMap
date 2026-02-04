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
IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Item ItemData;
    public RectTransform rect;
    protected Canvas ParentCanvas;
    protected static DraggableItem draggedItem; 

    [HideInInspector] public Transform draggedParent;
    public Image image;
    public ItemSlot Slot;

    public bool Dragging = false;
    public bool Hovering = false;

    private UIHoverJuice hover;
    
    private Coroutine Wait;
    private int waitID = 0;
    private bool waiting = false;
    public bool OptionsMenuActive = false;

    private Vector2 OptionsMenuAnchor;

    protected virtual void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        ParentCanvas = GetComponentInParent<Canvas>();
        hover = GetComponent<UIHoverJuice>();
        HideDisplay();
    }

    void Start()
    {
        image.sprite = ItemData.ItemDisplay;
    }
    public void OnPointerEnter(PointerEventData input)
    {
        if(InventoryManager.Instance.CurrentlyDragging) return;
        Hovering = true;
        hover.StartHover(true);
        ShowDisplay();
        AudioManager.Instance.UIClick();
    }
    public void OnPointerExit(PointerEventData input)
    {
        if(InventoryManager.Instance.CurrentlyDragging) return;
        
        if(!Dragging) {
            Hovering = false;
            hover.StartHover(false);
            HideDisplay();
        }
    }

    public void OnPointerClick(PointerEventData input)
    {
        if(InventoryManager.Instance.OptionsMenu.activeInHierarchy) InventoryManager.Instance.OptionsMenu.SetActive(false);

        if(input.button != PointerEventData.InputButton.Right) return;
        
        RectTransform optionRect = InventoryManager.Instance.OptionsMenu.GetComponent<RectTransform>();
        optionRect.position = input.position;
        InventoryManager.Instance.OptionsMenu.SetActive(true);
        OptionsMenuActive = true;
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
            InventoryManager.Instance.DisplayAnimate.sprites = null;
            InventoryManager.Instance.DisplayAnimate.length = 0;
            InventoryManager.Instance.DisplayAnimate.direction = true;
            InventoryManager.Instance.DisplayAnimate.fps = 0;
            InventoryManager.Instance.DisplayAnimate.isPlaying = false;

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
        if(InventoryManager.Instance.OptionsMenu.activeInHierarchy) return;

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
        if(InventoryManager.Instance.OptionsMenu.activeInHierarchy) InventoryManager.Instance.OptionsMenu.SetActive(false);
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
        InventoryManager.Instance.CurrentlyDragging = false;
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