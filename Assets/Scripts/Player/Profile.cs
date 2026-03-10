    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using TMPro;    
    using System.IO;

public class Profile : MonoBehaviour
{
    [SerializeField] private GameObject EyeLeft;
    [SerializeField] private GameObject EyeRight;
    [SerializeField] private GameObject Mouth;

    [SerializeField] private PlayerCharacterSprites sprites;

    private Image EyeLeftImage;
    private Image EyeRightImage;
    private Image MouthImage;

    private int EyeLeftIndex = 0;
    private int EyeRightIndex = 0;

    private int MouthIndex = 0;


    
    private RectTransform EyeLeftRect;
    private RectTransform EyeRightRect;
    private RectTransform MouthRect;

    private Vector3 EyeLeftPos;
    private Vector3 EyeRightPos;
    private Vector3 MouthPos;

    
    private Vector3 EyeLeftScale;
    private Vector3 EyeRightScale;
    private Vector3 MouthScale;

    Coroutine HurtRoutine;

    void OnEnable()
    {
        FindParts();
        LoadProfile();
    }

    void FindParts()
    {
        if(EyeLeft == null || EyeRight == null || Mouth == null)
        {
            Debug.Log("failed to find parts");
            return;
        }
        EyeLeftImage = EyeLeft.GetComponent<Image>();
        EyeLeftRect = EyeLeft.GetComponent<RectTransform>();

        EyeRightImage = EyeRight.GetComponent<Image>();
        EyeRightRect = EyeRight.GetComponent<RectTransform>();

        MouthImage = Mouth.GetComponent<Image>();
        MouthRect = Mouth.GetComponent<RectTransform>();

        EyeLeftPos = EyeLeftRect.localPosition;
        EyeRightPos = EyeRightRect.localPosition;
        MouthPos = MouthRect.localPosition;

        EyeLeftScale = EyeLeftRect.localScale;
        EyeRightScale = EyeRightRect.localScale;
        MouthScale = MouthRect.localScale;
    }

    public void LoadProfile()
    {
        string path = SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Character.json");
        if(!File.Exists(path)) return;

        CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(File.ReadAllText(path));

        EyeLeftImage.sprite = sprites.EyeSprites[data.eyeLeftIndex];
        EyeRightImage.sprite = sprites.EyeSprites[data.eyeRightIndex];
        MouthImage.sprite = sprites.MouthSprites[data.mouthIndex];
        
        EyeLeftIndex = data.eyeLeftIndex;
        EyeRightIndex = data.eyeRightIndex;
        MouthIndex = data.mouthIndex;

        EyeLeftRect.localScale = new Vector3(EyeLeftScale.x * data.elScaleX, EyeLeftScale.y * data.elScaleY, 1f);
        EyeRightRect.localScale = new Vector3(EyeRightScale.x * data.erScaleX, EyeRightScale.y * data.erScaleY, 1f);
        MouthRect.localScale = new Vector3(MouthScale.x * data.mouthScaleX, MouthScale.y * data.mouthScaleY, 1f);

        EyeLeftRect.localPosition = new Vector3(EyeLeftPos.x + data.elOffsetX, EyeLeftPos.y + data.elOffsetY, EyeLeftPos.z);
        EyeRightRect.localPosition = new Vector3(EyeRightPos.x + data.erOffsetX, EyeRightPos.y + data.erOffsetY, EyeRightPos.z);
        MouthRect.localPosition = new Vector3(MouthPos.x + data.mouthOffsetX, MouthPos.y + data.mouthOffsetY, MouthPos.z);

        EyeLeftRect.localEulerAngles = new Vector3(0f, 0f, data.elRot);
        EyeRightRect.localEulerAngles = new Vector3(0f, 0f, data.erRot);
        MouthRect.localEulerAngles = new Vector3(0f, 0f, data.mouthRot);
    }

    public void TriggerHurt()
    {
        if(HurtRoutine != null)
        {
            StopCoroutine(HurtRoutine);
            HurtRoutine = null;
        }
        HurtRoutine = StartCoroutine(Hurt());
    }

    IEnumerator Hurt()
    {
        float t = 0;

        float hurtDur = 0.75f;

        while(t < hurtDur)
        {
            t += Time.deltaTime;
            
            EyeLeftImage.sprite = sprites.EyeHurtSprites[EyeLeftIndex];
            EyeRightImage.sprite = sprites.EyeHurtSprites[EyeRightIndex];
            MouthImage.sprite = sprites.MouthHurtSprites[MouthIndex];
            yield return null;
        }

           EyeLeftImage.sprite = sprites.EyeSprites[EyeLeftIndex];
            EyeRightImage.sprite = sprites.EyeSprites[EyeRightIndex];
            MouthImage.sprite = sprites.MouthSprites[MouthIndex];
    }
}
