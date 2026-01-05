using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance;
    public GameObject canvas;

    public TMP_Text leftSide;
    public TMP_Text rightSide;

    public TMP_Text leftPagination;
    public TMP_Text rightPagination;

    private SpriteText leftText;
    private SpriteText rightText; 
    private SpriteText leftPageNumber;
    private SpriteText rightPageNumber;

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
        canvas.SetActive(true);
    }

    public void Close()
    {
        canvas.SetActive(false);
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
        if(leftSide.pageToDisplay < 1)
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
}
