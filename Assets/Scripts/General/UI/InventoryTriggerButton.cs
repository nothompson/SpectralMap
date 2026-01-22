using UnityEngine;

public class InventoryTriggerButton : MonoBehaviour
{
    public void TriggerInventoryOpen()
    {
        InventoryManager.Instance.Open();
        AudioManager.Instance.BodybagOpen();
    }
    public void TriggerInventoryClose()
    {
        InventoryManager.Instance.Close();
        AudioManager.Instance.BodybagClose();
    }
}
