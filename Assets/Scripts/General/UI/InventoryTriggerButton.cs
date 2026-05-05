using UnityEngine;

public class InventoryTriggerButton : MonoBehaviour
{
    public void TriggerInventoryOpen()
    {
        InventoryManager.Instance.Open();
        AudioManager.Instance.BodybagOpen();
        PauseManager.Instance.TriggerRaycasts(false);
    }
    public void TriggerInventoryClose()
    {
        InventoryManager.Instance.Close();
        AudioManager.Instance.BodybagClose();
        PauseManager.Instance.TriggerRaycasts(true);
    }
}
