using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalTriggerButton : MonoBehaviour
{
    public void TriggerJournalOpen()
    {
        JournalManager.Instance.Open();
    }
    public void TriggerJournalClose()
    {
        JournalManager.Instance.Close();
    }
    public void JournalNextPage()
    {
        JournalManager.Instance.NextPage();
    }
    public void JournalPrevPage()
    {
        JournalManager.Instance.PreviousPage();
    }
}
