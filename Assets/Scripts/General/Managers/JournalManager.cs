using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance;
    public GameObject Container;
    public GameObject SubContainer;
    public SpriteAnimate journalSprite;
    [SerializeField] private AnimationCurve transitionCurve;

    public TMP_Text leftSide;
    public TMP_Text rightSide;

    public TMP_Text leftPagination;
    public TMP_Text rightPagination;

    private SpriteText leftText;
    private SpriteText rightText; 
    private SpriteText leftPageNumber;
    private SpriteText rightPageNumber;

    Coroutine transitionRoutine;
    bool opening = false;

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

        leftText = leftSide.GetComponent<SpriteText>();

        rightText = rightSide.GetComponent<SpriteText>();

        leftPageNumber = leftPagination.GetComponent<SpriteText>();

        rightPageNumber = rightPagination.GetComponent<SpriteText>();

        UpdatePagination();
    }

    public void Open()
    {
        if(opening) return;

        opening = true;

        Container.SetActive(true);

        StartCoroutine(Transition(true));

        StartCoroutine(journalSprite.AnimateToTarget(0, null, () =>
        {
            SubContainer.SetActive(true);
            opening = false;
        }));
        
        AudioManager.Instance.JournalOpen();
    }

    public void Close()
    {
        StartCoroutine(Transition(false));

        SubContainer.SetActive(false);
        StartCoroutine(journalSprite.AnimateToTarget(journalSprite.sprites.Length - 1, null, () =>
        {
            Container.SetActive(false);
        }));
        AudioManager.Instance.JournalClose();
    }

    public void AddText(string input)
    {
        leftText.input += input + "\n\n";
        rightText.input += input + "\n\n";

        leftText.Refresh();
        rightText.Refresh();
    }

    // private void OnValidate()
    // {
    //     UpdatePagination();

    //     if(leftSide.text == content) return;

    //     SetupContent();
    // }

    // private void SetupConent()
    // {
    //     leftSide.text = content;
    //     rightSide.text = content;
    // }

    private void UpdatePagination()
    {
        leftPageNumber.input = leftSide.pageToDisplay.ToString();
        rightPageNumber.input = rightSide.pageToDisplay.ToString();

        leftPageNumber.Refresh();
        rightPageNumber.Refresh();
    }

    public void PreviousPage()
    {
        if(leftSide.pageToDisplay <= 1)
        {
            leftSide.pageToDisplay = 1;
            return;
        }

        if(leftSide.pageToDisplay -2 > 1)
        {
            leftSide.pageToDisplay -= 2;
        }
        else
        {
            leftSide.pageToDisplay = 1;
        }

        rightSide.pageToDisplay = leftSide.pageToDisplay + 1;

        AudioManager.Instance.JournalPrevious();

        UpdatePagination();
    }

    public void NextPage()
    {
        if(rightSide.pageToDisplay >= rightSide.textInfo.pageCount) return;

        if(leftSide.pageToDisplay >= leftSide.textInfo.pageCount - 1)
        {
            leftSide.pageToDisplay = leftSide.textInfo.pageCount - 1;
            rightSide.pageToDisplay = leftSide.pageToDisplay + 1;
        }
        else
        {
            leftSide.pageToDisplay += 2;
            rightSide.pageToDisplay = leftSide.pageToDisplay + 1;
        }

        AudioManager.Instance.JournalNext();

        UpdatePagination();
    }

    public void StartTransition(bool intro)
    {
        if(transitionRoutine != null) {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
            }
        transitionRoutine = StartCoroutine(Transition(intro));
    }

    IEnumerator Transition(bool intro)
    {
        float t = 0f;
        float dur = 0.5f;

        Vector3 target = intro ? Vector3.one : Vector3.zero;
        RectTransform rect = Container.GetComponent<RectTransform>();
        Vector3 starting;

        if (intro)
        {
            rect.localScale = Vector3.zero;
            starting = Vector3.one;
        }
        else
        {
            starting = rect.localScale;
        }
        while(t < dur)
        {
            t += Time.unscaledDeltaTime;
            float time = Mathf.Clamp01(t / dur);
            float elapsed = intro ? time : 1f - time;
            float value = transitionCurve.Evaluate(elapsed);

            rect.localScale = starting * value;
            yield return null;
        }
        rect.localScale = target;
        transitionRoutine = null;
    }
}
