    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using TMPro;    
    using System.IO;


[System.Serializable]
public class ProfilePart : MonoBehaviour
{
    public Sprite PartSprite;
    public Sprite HurtSprite;
}
[System.Serializable]
public class CharacterSaveData
{
    public string playerName = "";

    public int eyeLeftIndex;
    public int eyeRightIndex;
    public int mouthIndex;

    public float elScaleX = 1f, elScaleY = 1f;
    public float erScaleX = 1f, erScaleY = 1f;
    public float mouthScaleX = 1f, mouthScaleY = 1f;

    public float elOffsetX, elOffsetY;
    public float erOffsetX, erOffsetY;
    public float mouthOffsetX, mouthOffsetY;

    public float erRot, elRot, mouthRot;
}
public class CharacterCreator : MonoBehaviour
{
    [HideInInspector] public int SaveSlotIndex = 0;
    [SerializeField] public GameObject EyeLeft;
    [SerializeField] public GameObject EyeRight;
    [SerializeField] public GameObject Mouth;

    [SerializeField] public Image[] CharacterFace;

    [SerializeField] private AnimationCurve FadeCurve;
    [SerializeField] private float fadeDur;
    [SerializeField] public SpriteText EyeName;
    [SerializeField] public SpriteText MouthName;
    [SerializeField] public SpriteText PlayerName;

    [SerializeField] public RectTransform[] Rects;
    [SerializeField] public AnimationCurve ScaleCurve;
    [SerializeField] private float introDur;

    [SerializeField] public PlayerCharacterSprites sprites;
    [SerializeField] private Slider MouthScaleX;
    [SerializeField] private Slider MouthScaleY;
    [SerializeField] private Slider MouthRotation;
    private Image EyeLeftImage;
    private RectTransform EyeLeftRect;
    private Image EyeRightImage;
    private RectTransform EyeRightRect;
    private Image MouthImage;
    private RectTransform MouthRect;

    private List<GameObject> Parts = new List<GameObject>();

    private int CurrentEyeLeftIndex;

    private int CurrentEyeRightIndex;

    private int CurrentMouthIndex;

    private bool OnLeft = true;

    private float elOffsetX = 0;
    private float elOffsetY = 0;

    private float elScaleX = 1;
    private float elScaleY = 1;
    private float erScaleX = 1;
    private float erScaleY = 1;

    private float mouthScaleX = 1;
    private float mouthScaleY = 1;
    
    private float elRot = 0f;
    private float erRot = 0f;

    private float mRot = 0f;
    private Vector3 mouthRot;
    private Vector3 leftEyePos;

    private Vector3 mouthPos;

    private Vector3 leftEyeScale;
    private Vector3 mouthScale;

    private float erOffsetX = 0;
    private float erOffsetY = 0;
    private float mouthOffsetX = 0;
    private float mouthOffsetY = 0;
    private Vector3 rightEyePos;
    private Vector3 rightEyeScale;

    private Vector3 leftEyeRot;

    private Vector3 rightEyeRot;

    private bool Translating;
    private Coroutine TranslateRoutine;

    private Coroutine TranslateSound;

    Coroutine HurtState;

    private bool MouthScalesLocked = false;
    private bool EyeScalesLocked = false;

    [SerializeField] private Slider EyeScaleX;

    [SerializeField] private Slider EyeScaleY;
    [SerializeField] private Slider EyeRotation;

    [SerializeField] private SpriteAnimate MouthLock;
    [SerializeField] private SpriteAnimate EyeLock;
    [SerializeField] private SpriteAnimate Swap;

     [SerializeField] private TMP_InputField NameInput;
    private Dictionary<Sprite, string> EyeDictionary;
    private Dictionary<Sprite, string> MouthDictionary;

    void OnEnable()
    {
        FindParts();
        SetupDictionary();
        NameInput.gameObject.SetActive(false);
        NameInput.gameObject.SetActive(true);
        StartCoroutine(AnimateSequence(true));
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

        CurrentEyeLeftIndex = (int)Random.Range(0,sprites.EyeSprites.Length - 1); 
        CurrentEyeRightIndex = (int)Random.Range(0,sprites.EyeSprites.Length - 1);
        CurrentMouthIndex = (int)Random.Range(0,sprites.MouthSprites.Length - 1); 

        EyeLeftImage.sprite = sprites.EyeSprites[CurrentEyeLeftIndex];
        EyeRightImage.sprite = sprites.EyeSprites[CurrentEyeRightIndex];
        MouthImage.sprite = sprites.MouthSprites[CurrentMouthIndex];

        Parts.Add(EyeLeft);
        Parts.Add(EyeRight);
        Parts.Add(Mouth);

        InitValues();
    }


    void InitValues()
    {
        leftEyePos = EyeLeftRect.localPosition;
        rightEyePos = EyeRightRect.localPosition;

        leftEyeScale = EyeLeftRect.localScale;
        rightEyeScale = EyeRightRect.localScale;

        leftEyeRot = EyeLeftRect.localEulerAngles;
        rightEyeRot = EyeRightRect.localEulerAngles;

        mouthPos = MouthRect.localPosition;
        mouthScale = MouthRect.localScale;
        mouthRot = MouthRect.localEulerAngles;

        elOffsetX = -30f;
        erOffsetX = 30f;

        EyeLeftRect.localPosition = new Vector3(leftEyePos.x + elOffsetX, leftEyePos.y, leftEyePos.z);

        EyeRightRect.localPosition = new Vector3(rightEyePos.x + erOffsetX, rightEyePos.y, rightEyePos.z);

        foreach(var img in CharacterFace)
        {
            img.color = new Color(1f,1f,1f,0f);
        }
    }

    IEnumerator AnimateSequence(bool dir)
    {
        var baseScales = new Vector3[Rects.Length];

        for(int i = 0; i < Rects.Length; i++)
        {
            baseScales[i] = Rects[i].localScale;
            Rects[i].localScale = dir ? Vector3.zero : baseScales[i];
        }

        float t = 0f;

        while(t < introDur)
        {
            t += Time.unscaledDeltaTime;
            for(int i = 0; i < Rects.Length; i++)
            {
                float time = Mathf.Clamp01(t / introDur);

                float scale = dir ? ScaleCurve.Evaluate(time) : ScaleCurve.Evaluate(1f - time);

                Rects[i].localScale = baseScales[i] * scale;
            }
            yield return null;
        }
        for(int i = 0; i < Rects.Length; i++)
        {
            Rects[i].localScale = dir ? baseScales[i] : Vector3.zero;
            UIHoverJuice hover = Rects[i].GetComponent<UIHoverJuice>();
            if(hover != null)
            {
                hover.ReInit();
            }
        }

        StartCoroutine(FadeInCharacter());

    }

    IEnumerator FadeInCharacter()
    {
        float t = 0f;
        Color fade = new Color(1f,1f,1f,1f);
        while(t < fadeDur)
        {
            t += Time.unscaledDeltaTime;
            float time = Mathf.Clamp01(t / fadeDur);
            fade.a = FadeCurve.Evaluate(time);
            for(int i =0; i < CharacterFace.Length; i++)
            {
                CharacterFace[i].color = fade;
            }

            yield return null;
        }

        foreach(var img in CharacterFace)
        {
            img.color = new Color(1f,1f,1f,1f);
        }
    }

    void SetupDictionary()
    {
        EyeDictionary = new Dictionary<Sprite, string>
        {
            {sprites.EyeSprites[0], "Appended"},
            {sprites.EyeSprites[1], "Bloodbath"},
            {sprites.EyeSprites[2], "dilator"},
            {sprites.EyeSprites[3], "empty"},
            {sprites.EyeSprites[4], "fried"},
            {sprites.EyeSprites[5], "gouged"},
            {sprites.EyeSprites[6], "grafted"},
            {sprites.EyeSprites[7], "hive"},
            {sprites.EyeSprites[8], "hopeless"},
            {sprites.EyeSprites[9], "stressed"},
            {sprites.EyeSprites[10], "crybaby"},
            {sprites.EyeSprites[11], "x'd out"},
            {sprites.EyeSprites[12], "linear"},
            {sprites.EyeSprites[13], "crescent"},
            {sprites.EyeSprites[14], "bead"},
            {sprites.EyeSprites[15], "arrow"}
        };
        MouthDictionary = new Dictionary<Sprite, string>
        {
            {sprites.MouthSprites[0], "joker"},
            {sprites.MouthSprites[1], "leach"},
            {sprites.MouthSprites[2], "pinchpot"},
            {sprites.MouthSprites[3], "sensors"},
            {sprites.MouthSprites[4], "carnivore"},
            {sprites.MouthSprites[5], "toad"},
            {sprites.MouthSprites[6], "tonguetied"},
            {sprites.MouthSprites[7], "zipper"},
            {sprites.MouthSprites[8], "crossed"},
            {sprites.MouthSprites[9], "bored"},
            {sprites.MouthSprites[10], "crescent"},
            {sprites.MouthSprites[11], "shocked"},
            {sprites.MouthSprites[12], "mischief"}
        };

            if(EyeDictionary.TryGetValue(EyeLeftImage.sprite, out string eye))
            {
                EyeName.input = eye;
                EyeName.Refresh();
            }

        if(MouthDictionary.TryGetValue(MouthImage.sprite, out string mouth))
        {
            MouthName.input = mouth;
            MouthName.Refresh();
        }
    }

    public void UpdateName(string input)
    {
        if (input.Length > 14)
        {
            input = input.Substring(0, 14);
            NameInput.SetTextWithoutNotify(input);
        }

        PlayerName.input = string.IsNullOrEmpty(input) ? "" : input;
        PlayerName.Refresh();

        if (input.Contains("\n"))
        {
            NameInput.SetTextWithoutNotify(input.Replace("\n", ""));
            NameInput.MoveToEndOfLine(false, false);
        }
    }

    public void LockMouthScales()
    {
        if(MouthScalesLocked) {
            MouthScalesLocked = false;
            MouthLock.SetFrame(0);
        }
        else if(!MouthScalesLocked) {
            MouthScalesLocked = true;
            MouthLock.SetFrame(1);
        }
    }

    public void LockEyeScales()
    {
        if(EyeScalesLocked) {
            EyeScalesLocked = false;
            EyeLock.SetFrame(0);
        }
        else if(!EyeScalesLocked) {
            EyeScalesLocked = true;
            EyeLock.SetFrame(1);
        }
    }

    public void RotateEye(float z)
    {
        if (OnLeft)
        {
            elRot = z;
            EyeLeftRect.localEulerAngles = new Vector3(0f,0f, z);
        }
        else
        {
            erRot = z;
            EyeRightRect.localEulerAngles = new Vector3(0f, 0f, z);
        }
    }

    public void RotateMouth(float z)
    {
        mRot = z;
        MouthRect.localEulerAngles = new Vector3(0f,0f,z);
    }
    private bool scaleSyncing = false;
    public void ScaleEyeX(float x)
{
    if (OnLeft)
    {
        elScaleX = x;
        Vector3 current = EyeLeftRect.localScale;
        EyeLeftRect.localScale = new Vector3(leftEyeScale.x * x, current.y, current.z);
        if (EyeScalesLocked && !scaleSyncing) { 
            scaleSyncing = true; 
            elScaleY = x; 
            ScaleEyeY(x); 
            scaleSyncing = false; 
            }
    }
    else
    {
        erScaleX = x;
        Vector3 current = EyeRightRect.localScale;
        EyeRightRect.localScale = new Vector3(rightEyeScale.x * x, current.y, current.z);
              if (EyeScalesLocked && !scaleSyncing) { 
            scaleSyncing = true; 
            erScaleY = x; 
            ScaleEyeY(x); 
            scaleSyncing = false; 
            }
    }
    RefreshSlider();
}

public void ScaleEyeY(float y)
{
    if (OnLeft)
    {
        elScaleY = y;
        Vector3 current = EyeLeftRect.localScale;
        EyeLeftRect.localScale = new Vector3(current.x, leftEyeScale.y * y, current.z);
            if (EyeScalesLocked && !scaleSyncing) { 
            scaleSyncing = true; 
            elScaleX = y; 
            ScaleEyeX(y); 
            scaleSyncing = false; 
            }
    }
    else
    {
        erScaleY = y;
        Vector3 current = EyeRightRect.localScale;
        EyeRightRect.localScale = new Vector3(current.x, rightEyeScale.y * y, current.z);
         if (EyeScalesLocked && !scaleSyncing) { 
            scaleSyncing = true; 
            erScaleX = y; 
            ScaleEyeX(y); 
            scaleSyncing = false; 
            }
    }
    RefreshSlider();
}

public void ScaleMouthX(float x)
{
    mouthScaleX = x;
    Vector3 current = MouthRect.localScale;
    MouthRect.localScale = new Vector3(mouthScale.x * x, current.y, current.z);
    if (MouthScalesLocked && !scaleSyncing) { 
            scaleSyncing = true; 
            mouthScaleY = x; 
            ScaleMouthY(x); 
            scaleSyncing = false; 
            }
    RefreshSlider();
}

public void ScaleMouthY(float y)
{
    mouthScaleY = y;
    Vector3 current = MouthRect.localScale;
    MouthRect.localScale = new Vector3(current.x, mouthScale.y * y, current.z);
      if (MouthScalesLocked && !scaleSyncing) { 
            scaleSyncing = true; 
            mouthScaleX = y; 
            ScaleMouthX(y); 
            scaleSyncing = false; 
            }
    RefreshSlider();
}

    public void TranslateEyeX(bool dir)
    {
        if (OnLeft)
        {
            TranslateRoutine = StartCoroutine(TranslateLeftEyeX(EyeLeftRect, dir, leftEyePos));
            TranslateSound = StartCoroutine(TranslatingSound());

        }
        else
        {
            TranslateRoutine = StartCoroutine(TranslateRightEyeX(EyeRightRect, dir, rightEyePos));
             TranslateSound = StartCoroutine(TranslatingSound());
        }
    }

    public void TranslateMouthX(bool dir)
    {
        TranslateRoutine = StartCoroutine(TranslateMouthXRoutine(MouthRect, dir, mouthPos));
         TranslateSound = StartCoroutine(TranslatingSound());
    }

    public void TranslateMouthY(bool dir)
    {
        TranslateRoutine = StartCoroutine(TranslateMouthYRoutine(MouthRect, dir, mouthPos));
         TranslateSound = StartCoroutine(TranslatingSound());
    }

    IEnumerator TranslatingSound()
    {
        while (Translating)
        {
            AudioManager.Instance.ReloadTick();

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator TranslateMouthXRoutine(RectTransform rect, bool dir, Vector3 pos)
    {
        Translating = true;

        while (Translating)
        {
            float offset = dir ? 1 : -1;
            mouthOffsetX+= offset;

            if(mouthOffsetX >= 75) mouthOffsetX = 75;
            if(mouthOffsetX <= -75) mouthOffsetX = -75;

            rect.localPosition = new Vector3(pos.x + mouthOffsetX, pos.y + mouthOffsetY, pos.z);

            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator TranslateMouthYRoutine(RectTransform rect, bool dir, Vector3 pos)
    {
        Translating = true;

        while (Translating)
        {
            float offset = dir ? 1 : -1;
            mouthOffsetY+= offset;

            if(mouthOffsetY >= 75) mouthOffsetY = 75;
            if(mouthOffsetY <= -75) mouthOffsetY = -75;

            rect.localPosition = new Vector3(pos.x + mouthOffsetX, pos.y + mouthOffsetY, pos.z);

            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator TranslateLeftEyeX(RectTransform rect, bool dir, Vector3 pos)
    {
        Translating = true;

        while (Translating)
        {
            float offset = dir ? 1 : -1;
            elOffsetX+= offset;

            if(elOffsetX >= 75) elOffsetX = 75;
            if(elOffsetX <= -75) elOffsetX = -75;

            rect.localPosition = new Vector3(pos.x + elOffsetX, pos.y + elOffsetY, pos.z);

            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator TranslateRightEyeX(RectTransform rect, bool dir, Vector3 pos)
    {
        Translating = true;

        while (Translating)
        {
            float offset = dir ? 1 : -1;
            erOffsetX+= offset;

            if(erOffsetX >= 75) erOffsetX = 75;
            if(erOffsetX <= -75) erOffsetX = -75;

            rect.localPosition = new Vector3(pos.x + erOffsetX, pos.y + erOffsetY, pos.z);

            yield return new WaitForSeconds(0.02f);
        }
    }

    public void TranslateEyeY(bool dir)
    {
        if (OnLeft)
        {
            TranslateRoutine = StartCoroutine(TranslateLeftEyeY(EyeLeftRect, dir, leftEyePos));
             TranslateSound = StartCoroutine(TranslatingSound());
        }
        else
        {
            TranslateRoutine = StartCoroutine(TranslateRightEyeY(EyeRightRect, dir, rightEyePos));
             TranslateSound = StartCoroutine(TranslatingSound());
        }
    }

    IEnumerator TranslateLeftEyeY(RectTransform rect, bool dir, Vector3 pos)
    {
        Translating = true;

        while (Translating)
        {
            float offset = dir ? 1 : -1;
            elOffsetY+= offset;

            if(elOffsetY >= 75) elOffsetY = 75;
            if(elOffsetY <= -75) elOffsetY = -75;

            rect.localPosition = new Vector3(pos.x  + elOffsetX, pos.y + elOffsetY, pos.z);

            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator TranslateRightEyeY(RectTransform rect, bool dir, Vector3 pos)
    {
        Translating = true;

        while (Translating)
        {
            float offset = dir ? 1 : -1;
            erOffsetY+= offset;

            if(erOffsetY >= 75) erOffsetY = 75;
            if(erOffsetY <= -75) erOffsetY = -75;

            rect.localPosition = new Vector3(pos.x + erOffsetX, pos.y + erOffsetY, pos.z);

            yield return new WaitForSeconds(0.02f);
        }
    }

    void RefreshSlider()
    {
        if (OnLeft)
        {
            EyeScaleX.SetValueWithoutNotify(elScaleX);
            EyeScaleY.SetValueWithoutNotify(elScaleY);
            EyeRotation.SetValueWithoutNotify(elRot);
        }
        else
        {
            EyeScaleX.SetValueWithoutNotify(erScaleX);
            EyeScaleY.SetValueWithoutNotify(erScaleY);
            EyeRotation.SetValueWithoutNotify(erRot);
        }

        MouthScaleX.SetValueWithoutNotify(mouthScaleX);
        MouthScaleY.SetValueWithoutNotify(mouthScaleY);
        MouthRotation.SetValueWithoutNotify(mRot);

    }

    public void StopTranslating()
    {
        Translating = false;
        if(TranslateRoutine != null)
        {
            StopCoroutine(TranslateRoutine);
            TranslateRoutine = null;
        }
        if(TranslateSound != null)
        {
            StopCoroutine(TranslateSound);
            TranslateSound = null;
        }

    }

    // public void TranslateEyeY(float y)
    // {
    //     Vector3 original = part.localPosition;
    //     part.localPosition = new Vector3(original.x + x, original.y, original.z);
    // }

    public void Scale(RectTransform part, float x, float y)
    {
        Vector3 original = part.localScale;
        part.localPosition = new Vector3(original.x + x, original.y + y, original.z);
    }

    public void ChangeEyeLeft(float value)
    {
        int i = Mathf.Clamp((int)value, 0, sprites.EyeSprites.Length - 1);
        EyeLeftImage.sprite = sprites.EyeSprites[i];
        CurrentEyeLeftIndex = i;
        // SaveCharacterSettings();
    }

    public void SwapEye()
    {
        if (OnLeft)
        {
            OnLeft = false;

            Swap.SetFrame(1);

            if(EyeDictionary.TryGetValue(EyeRightImage.sprite, out string input))
            {
                EyeName.input = input;
                EyeName.Refresh();
            }
        } 
        else if (!OnLeft) 
        {
            OnLeft = true;
            Swap.SetFrame(0);
            if(EyeDictionary.TryGetValue(EyeLeftImage.sprite, out string input))
            {
                EyeName.input = input;
                EyeName.Refresh();
            }
        }
        RefreshSlider();
    }

    public void CycleEye(bool next)
    {
        int direction = next ? 1 : -1;
        if (OnLeft)
        {
            CurrentEyeLeftIndex+= direction;
            if(CurrentEyeLeftIndex >= sprites.EyeSprites.Length) CurrentEyeLeftIndex = 0;
            if(CurrentEyeLeftIndex < 0) CurrentEyeLeftIndex = sprites.EyeSprites.Length -1;
            EyeLeftImage.sprite = sprites.EyeSprites[CurrentEyeLeftIndex];

            if(EyeDictionary.TryGetValue(EyeLeftImage.sprite, out string input))
            {
                EyeName.input = input;
                EyeName.Refresh();
            }

        }
        else if (!OnLeft)
        {
            CurrentEyeRightIndex+= direction;
            if(CurrentEyeRightIndex >= sprites.EyeSprites.Length) CurrentEyeRightIndex = 0;
            if(CurrentEyeRightIndex < 0) CurrentEyeRightIndex = sprites.EyeSprites.Length -1;
            EyeRightImage.sprite = sprites.EyeSprites[CurrentEyeRightIndex];

            if(EyeDictionary.TryGetValue(EyeRightImage.sprite, out string input))
            {
                EyeName.input = input;
                EyeName.Refresh();
            }
        }

        if(next) AudioManager.Instance.UIOpen();
        else AudioManager.Instance.UIClose();
    }

    public void CycleMouth(bool next)
    {
        int direction = next ? 1 : -1;

        CurrentMouthIndex+= direction;
        if(CurrentMouthIndex >= sprites.MouthSprites.Length) CurrentMouthIndex = 0;
            if(CurrentMouthIndex < 0) CurrentMouthIndex = sprites.MouthSprites.Length -1;
            MouthImage.sprite = sprites.MouthSprites[CurrentMouthIndex];

        
        if(MouthDictionary.TryGetValue(MouthImage.sprite, out string input))
        {
            MouthName.input = input;
            MouthName.Refresh();
        }

          if(next) AudioManager.Instance.UIOpen();
        else AudioManager.Instance.UIClose();

    }

     public void ChangeEyeRight(float value)
    {
        int i = Mathf.Clamp((int)value, 0, sprites.EyeSprites.Length - 1);
        EyeRightImage.sprite = sprites.EyeSprites[i];
        CurrentEyeRightIndex = i;
        // SaveCharacterSettings();
    }

    public void ChangeMouth(float value)
    {
        int i = Mathf.Clamp((int)value, 0, sprites.MouthSprites.Length - 1);
        MouthImage.sprite = sprites.MouthSprites[i];
        CurrentMouthIndex = i;
        // SaveCharacterSettings();
    }

    public void TriggerHurt()
    {
        if(HurtState != null)
        {
            StopCoroutine(HurtState);
        }
        HurtState = StartCoroutine(FlashHurt());
        AudioManager.Instance.Hurt();
    }

    IEnumerator FlashHurt()
    {
        float t = 0;
        float dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float time = t / dur;

            EyeLeftImage.sprite = sprites.EyeHurtSprites[CurrentEyeLeftIndex];
            EyeRightImage.sprite = sprites.EyeHurtSprites[CurrentEyeRightIndex];
            MouthImage.sprite = sprites.MouthHurtSprites[CurrentMouthIndex];
            yield return null;
        }

        EyeLeftImage.sprite = sprites.EyeSprites[CurrentEyeLeftIndex];
        EyeRightImage.sprite = sprites.EyeSprites[CurrentEyeRightIndex];
        MouthImage.sprite = sprites.MouthSprites[CurrentMouthIndex];

    }

    public void SaveCharacterSettings()
    {
        SaveSystem.EnsureSlotExists(SaveSystem.CurrentSlot);

        string savePath = SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Save.json");

        SaveData save = File.Exists(savePath) ? JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath)) : new SaveData();

        save.playerName = PlayerName.input;
        save.hasData = true;
        File.WriteAllText(savePath, JsonUtility.ToJson(save, true));

        CharacterSaveData data = new CharacterSaveData();
        data.playerName = PlayerName.input;

        data.eyeLeftIndex = CurrentEyeLeftIndex;
        data.eyeRightIndex = CurrentEyeRightIndex;
        data.mouthIndex = CurrentMouthIndex;

        data.elScaleX = elScaleX;
        data.elScaleY = elScaleY;
        data.erScaleX = erScaleX;
        data.erScaleY = erScaleY;
        data.mouthScaleX = mouthScaleX;
        data.mouthScaleY = mouthScaleY;

        data.elOffsetX = elOffsetX;
        data.elOffsetY = elOffsetY;
        data.erOffsetX = erOffsetX;
        data.erOffsetY = erOffsetY;
        data.mouthOffsetX = mouthOffsetX;
        data.mouthOffsetY = mouthOffsetY;

        data.elRot = elRot;
        data.erRot = erRot;
        data.mouthRot = mRot;

        SaveSystem.EnsureSlotExists(SaveSystem.CurrentSlot);
        string path = SaveSystem.GetFilePath(SaveSystem.CurrentSlot, "Character.json");
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }
}
