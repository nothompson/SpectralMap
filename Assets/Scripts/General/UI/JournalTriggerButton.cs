using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalTriggerButton : MonoBehaviour
{
    public void TriggerJournalOpen()
    {
        JournalManager.Instance.Open();
        PauseManager.Instance.TriggerRaycasts(false);
    }
    public void TriggerJournalClose()
    {
        JournalManager.Instance.Close();
        PauseManager.Instance.TriggerRaycasts(true);
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
