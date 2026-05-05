using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    public GameObject CursorCanvas;
    public RectTransform CursorRect;
    public SpriteAnimate CursorAnimate;
    public Sprite[] Idle;
    public Sprite[] Hovering;
    public Sprite[] Clicking;
    public bool CursorActive;
    bool clicking = false;

    // Reusable list to avoid allocations every frame
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Cursor.visible = false;
    }

    bool IsPointerOverRaycastTarget()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject.transform.IsChildOf(CursorCanvas.transform)) continue;

            Graphic graphic = result.gameObject.GetComponent<Graphic>();
            if (graphic != null && graphic.raycastTarget)
            {
                return true;
            }
        }

        return false;
    }

    public void Update()
    {
        if (!CursorActive) return;

        CursorRect.position = Mouse.current.position.ReadValue();

        if (InputManager.Instance.inputs.Player.Fire.IsPressed())
        {
            clicking = true;
            CursorAnimate.sprites = Clicking;
            CursorAnimate.length = Clicking.Length;
        }
        else
        {
            clicking = false;
        }

        if (clicking) return;

        if (IsPointerOverRaycastTarget())
        {
            CursorAnimate.sprites = Hovering;
            CursorAnimate.length = Hovering.Length;
        }
        else
        {
            CursorAnimate.sprites = Idle;
            CursorAnimate.length = Idle.Length;
        }
    }

    public void ForceUpdate()
    {
        CursorRect.position = Mouse.current.position.ReadValue();
    }

    public void TriggerCursor(bool dir)
    {
        CursorActive = dir;
        CursorCanvas.SetActive(dir);
    }
}