    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class SaveData
{
    public bool hasData = false;
    public string playerName = "";
}

public class SaveSlots : MonoBehaviour
{
    [SerializeField] public GameObject[] objects;
    [SerializeField] public RectTransform[] SlotRects;

    [SerializeField] public RectTransform[] DeleteRects;

    [SerializeField] public RectTransform TextRect;

    [SerializeField] private AnimationCurve ScaleCurve;
    [SerializeField] private AnimationCurve OutroScaleCurve;
    [SerializeField] private AnimationCurve PositionCurve;
    [SerializeField] private AnimationCurve OutroPositionCurve;

    [SerializeField] private float introDur;

    [SerializeField] private GameObject CharacterCreator;

    public int selectedSlot = -1;
    private SaveData[] saves;
    private const int SLOTS = 3;

    public int slotToDelete = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        LoadAllSlots();
        RefreshSlot();
        StartCoroutine(AnimateSequence(true));

        for(int i = 0; i < SlotRects.Length; i++)
        {
            UIHoverJuice hover = SlotRects[i].GetComponent<UIHoverJuice>();
            if(hover != null)
            {
                hover.disabled = true;
            }
        }
    }

    void LoadAllSlots()
    {
        saves = new SaveData[SLOTS];
        for(int i = 0; i < SLOTS; i++)
        {
            string path = SaveSystem.GetFilePath(i, "Save.json");
            saves[i] = File.Exists(path) ? JsonUtility.FromJson<SaveData>(File.ReadAllText(path)) : new SaveData();
        }
    }

    void SaveSlot(int index)
    {
        SaveSystem.EnsureSlotExists(index);
        string path = SaveSystem.GetFilePath(index, "Save.json");
        File.WriteAllText(path, JsonUtility.ToJson(saves[index], true));
    }

    // void DeleteSlot(int index)
    // {
    //     if(index < 0 || index > SLOTS) return;
    //     string path = GetSavePath(index);
    //     if(File.Exists(path)) File.Delete(path);
    //     saves[index] = new SaveData();
    // }

    void RefreshSlot()
    {
        for(int i = 0; i < SlotRects.Length; i++)
        {
            if(i >= SLOTS) continue;
            var slot = SlotRects[i].GetComponent<SaveSlotButton>();
            if(slot != null)
            {
                slot.SetState(saves[i]);
            }
        
        }

        for(int i = 0; i < DeleteRects.Length; i++)
        {
            if (saves[i].hasData)
            {
                DeleteRects[i].gameObject.SetActive(true);
            }
            else
            {
                DeleteRects[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectSlot(int index)
    {
        if(index < 0 || index > SLOTS) return;
        selectedSlot = index;

        if (saves[index].hasData)
        {
            ContinueSave();
        }
        else
        {
            StartCharacterCreation();
        }
    }
    public void StartCharacterCreation()
    {
        if(selectedSlot < 0) return;
        saves[selectedSlot].hasData = true;
        SaveSlot(selectedSlot);
        OutroSequence(true);
        SaveSystem.CurrentSlot = selectedSlot;
        SaveSystem.OnSaveChange();
        //if save slot has no save data associated with it, then set character creation active and begin writing into save data
    }

    public void ContinueSave(bool intro = false)
    {
        SaveSystem.CurrentSlot = selectedSlot;
        SaveSystem.OnSaveChange();

        if(!intro){
            OutroSequence(intro);
        }
        //if save slot has save data associated with it, then load into scene
    }
    public void OutroSequence(bool cc)
    {
        StartCoroutine(AnimateSequence(false, cc));
    }

    public void SetSlot(int slot)
    {
        slotToDelete = slot;
    }

    public void DeleteSaveSlot()
    {
        SaveSystem.DeleteSave(slotToDelete);
        saves[slotToDelete].hasData = false;
        RefreshSlot();
    }   

    IEnumerator AnimateSequence(bool dir, bool cc = false)
    {
        var baseScales = new Vector3[SlotRects.Length];

        var basePosition = TextRect.anchoredPosition;
        TextRect.anchoredPosition = dir ? new Vector2(basePosition.x, -400f) : basePosition;

        float offset = 0.15f;

        for(int i = 0; i < SlotRects.Length; i++)
        {
            baseScales[i] = new Vector3(1f,1f,1f);

            SlotRects[i].localScale = dir ? Vector3.zero : baseScales[i];

            DeleteRects[i].localScale = dir ? Vector3.zero : baseScales[i];
        }

        float dur = introDur + offset * (SlotRects.Length - 1);
        float t = 0f;

        var played = new bool[SlotRects.Length];

        bool wordSound = false;

        bool transitionSound = false;

        while(t < dur)
        {
            t += Time.unscaledDeltaTime;
            for(int i = 0; i < SlotRects.Length; i++)
            {
                float delay = offset * i;
                float dTime = Mathf.Clamp01((t - delay) / introDur);

                float s = dir ? ScaleCurve.Evaluate(dTime) : ScaleCurve.Evaluate(1f - dTime);

                SlotRects[i].localScale = baseScales[i] * s;

                DeleteRects[i].localScale = baseScales[i] * s;

                if(!played[i] && dir && s >= 1.0f)
                {
                    played[i] = true;
                    AudioManager.Instance.Pop();
                }

                if(!dir && cc && !transitionSound)
                {
                    transitionSound = true;
                    AudioManager.Instance.TransitionTexture();
                }
            }


            float time = Mathf.Clamp01(t / introDur);

            float p = dir ? PositionCurve.Evaluate(time) : PositionCurve.Evaluate(1f - time);
            TextRect.anchoredPosition = new Vector2(basePosition.x, basePosition.y * p);

            if (!wordSound)
            {
                wordSound = true;
                AudioManager.Instance.WindSlice();
            }

            yield return null;
        }

        for(int i = 0; i < SlotRects.Length; i++)
        {
            SlotRects[i].localScale = dir ? baseScales[i] : Vector3.zero;
            DeleteRects[i].localScale = dir ? baseScales[i] : Vector3.zero;
            UIHoverJuice hover = SlotRects[i].GetComponent<UIHoverJuice>();
            UIHoverJuice hoverDel = DeleteRects[i].GetComponent<UIHoverJuice>();

            if(hover != null)
            {
                hover.ReInit();
                hover.disabled = false;
            }

            if(hoverDel != null)
            {
                if(DeleteRects[i].gameObject.activeInHierarchy){
                    hoverDel.ReInit();
                    hoverDel.disabled = false;
                }
            }
        }

        TextRect.anchoredPosition = dir ? basePosition : new Vector2(basePosition.x, -400f);

        if (!dir)
        {
            gameObject.SetActive(false);
            if (!cc)
            {
                LevelManager.Instance.LoadScene("Demo");
            }
        }
        if (cc)
        {
            CharacterCreator.SetActive(true);
        }
    }


}
