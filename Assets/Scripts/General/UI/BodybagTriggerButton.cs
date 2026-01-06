using UnityEngine;

public class BodybagTriggerButton : MonoBehaviour
{
    public void Open(){
        AudioManager.Instance.BodybagOpen();
    }

     public void Close(){
        AudioManager.Instance.BodybagClose();
    }
}
